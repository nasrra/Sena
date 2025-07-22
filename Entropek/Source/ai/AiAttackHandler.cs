using Godot;
using System;
using Godot.Collections;
using System.Collections.Generic;

public partial class AiAttackHandler : Node{

    public const string NodeName = nameof(AiAttackHandler);

    private enum TargetDirection : byte{
        Up,
        Right,
        Down,
        Left
    }
     
    [Export] private Array<AiAttack> upAttacks;
    [Export] private Array<AiAttack> rightAttacks;
    [Export] private Array<AiAttack> downAttacks;
    [Export] private Array<AiAttack> leftAttacks;
    [Export] private Array<AiAttack> omniAttacks;
    private Array<AiAttack> availableUpAttacks = new Array<AiAttack>();
    private Array<AiAttack> availableRightAttacks = new Array<AiAttack>();
    private Array<AiAttack> availableDownAttacks = new Array<AiAttack>();
    private Array<AiAttack> availableLeftAttacks = new Array<AiAttack>();
    private Array<AiAttack> availableOmniAttacks = new Array<AiAttack>();
    [Export] private Array<Timer> attackCooldowns; // <-- one timer per unique attack.

    public event Action<byte, AttackDirection> OnAttackStarted;
    public event Action<byte, AttackDirection> OnLeadIn;
    public event Action<byte, AttackDirection> OnAttack;
    public event Action<byte, AttackDirection> OnFollowThrough;
    public event Action OnAttackEnded;
    private event Action statePhysicsProcess = null;

    [Export] private HitBoxHandler hitBoxHandler;
    [Export] private Timer leadInStateTimer;
    [Export] private Timer attackStateTimer;
    [Export] private Timer followThroughStateTimer;
    [Export] private Timer standByCooldown;
    
    private RandomNumberGenerator rng = new RandomNumberGenerator();
    private TargetDirection targetDirection = TargetDirection.Right;
    
    private AiAttack chosenAttack = null;
    private AttackDirection chosenAttackDirection;

    private float angleToTarget     = 0;
    private float distanceToTarget  = float.MaxValue;
    private float maxMinTargetDistanceAttack = float.MinValue;

    private AiAttackHandlerState state;


    /// 
    /// Base.
    /// 


    public override void _Ready(){
        base._Ready();

        #if TOOLS
        Entropek.Util.Node.VerifyName(this, NodeName);
        #endif

        rng.Randomize();

        // set max range attack for perf reasons.

        CalcMaxRange();

        // GD.Print(attacks.Count);
        // GD.Print(maxRangeAttack);
    }

    public override void _PhysicsProcess(double delta){
        base._PhysicsProcess(delta);
        statePhysicsProcess?.Invoke();
    }

    public override void _EnterTree(){
        base._EnterTree();
        LinkEvents();
        StandbyState();
    }

    public override void _ExitTree(){
        base._ExitTree();
        UnlinkEvents();
    }



    /// 
    /// Initialisation.
    /// 


    private void CalcMaxRange(){
        
        // up attacks.

        for(int i = 0; i < upAttacks.Count; i++){
            AiAttack attack = upAttacks[i];
            if(attack.MinTargetDistance > maxMinTargetDistanceAttack){
                maxMinTargetDistanceAttack = attack.MinTargetDistance;
            }
        }

        // right attacks.

        for(int i = 0; i < rightAttacks.Count; i++){
            AiAttack attack = rightAttacks[i];
            if(attack.MinTargetDistance > maxMinTargetDistanceAttack){
                maxMinTargetDistanceAttack = attack.MinTargetDistance;
            }
        }

        // down attacks.

        for(int i = 0; i < downAttacks.Count; i++){
            AiAttack attack = downAttacks[i];
            if(attack.MinTargetDistance > maxMinTargetDistanceAttack){
                maxMinTargetDistanceAttack = attack.MinTargetDistance;
            }
        }

        // left attacks.

        for(int i = 0; i < leftAttacks.Count; i++){
            AiAttack attack = leftAttacks[i];
            if(attack.MinTargetDistance > maxMinTargetDistanceAttack){
                maxMinTargetDistanceAttack = attack.MinTargetDistance;
            }
        }

        // omni directional attacks.

        for(int i = 0; i < omniAttacks.Count; i++){
            AiAttack attack = omniAttacks[i];
            if(attack.MinTargetDistance > maxMinTargetDistanceAttack){
                maxMinTargetDistanceAttack = attack.MinTargetDistance;
            }
        }
    }

    ///
    /// State Machine.
    ///

    
    public void StandbyState(){
        if(standByCooldown.TimeLeft <= 0){
            
            state = AiAttackHandlerState.StandBy;

            statePhysicsProcess = StandByStatePhysicsProccess;
        }
    }

    private void StandByStatePhysicsProccess(){
        DetermineAvailableAttacks();
        ExecuteRandomAvailableAttack();
    }

    public void HaltState(){
        
        state = AiAttackHandlerState.Halted;
        
        leadInStateTimer.Stop();
        attackStateTimer.Stop();
        followThroughStateTimer.Stop();
        
        hitBoxHandler.DisableAllHitBoxes();
        
        statePhysicsProcess = null;
        
        StartChoseAttackCooldown();
    }

    public void HaltState(float time){
        HaltState();
        standByCooldown.WaitTime = time;
        standByCooldown.Start();
    } 

    public void PauseState(){

        state = AiAttackHandlerState.Paused;

        leadInStateTimer.Paused         = true;
        attackStateTimer.Paused         = true;
        followThroughStateTimer.Paused  = true;
        standByCooldown.Paused          = true;
        statePhysicsProcess = null;
        hitBoxHandler.PauseState();
    }

    public void ResumeState(){
        leadInStateTimer.Paused         = false;
        attackStateTimer.Paused         = false;
        followThroughStateTimer.Paused  = false;
        standByCooldown.Paused          = false;
        hitBoxHandler.ResumeState();
    }

    public void EvaluateState(){
        // go into standby if we are not currently attacking.

        if(leadInStateTimer.TimeLeft == 0
        && attackStateTimer.TimeLeft == 0
        && followThroughStateTimer.TimeLeft == 0){
            StandbyState();
        }
    }

    private void LeadInState(){
        
        state = AiAttackHandlerState.LeadIn;

        leadInStateTimer.WaitTime = chosenAttack.LeadInTime;
        leadInStateTimer.Start();
        
        OnLeadIn?.Invoke(chosenAttack.Id, chosenAttackDirection);
    }

    private void AttackState(){
        
        state = AiAttackHandlerState.Attack;

        attackStateTimer.WaitTime = chosenAttack.AttackTime;
        attackStateTimer.Start();
        
        OnAttack?.Invoke(chosenAttack.Id, chosenAttackDirection);
    }

    private void FollowThroughState(){
        
        state = AiAttackHandlerState.FollowThrough;

        followThroughStateTimer.WaitTime = chosenAttack.FollowThroughTime;
        followThroughStateTimer.Start();
        
        OnFollowThrough?.Invoke(chosenAttack.Id, chosenAttackDirection);
    }

    private void CooldownState(){
        state = AiAttackHandlerState.Cooldown;

        standByCooldown.WaitTime = chosenAttack.HandlerCooldown;
        standByCooldown.Start();
        StartChoseAttackCooldown();
        OnAttackEnded?.Invoke();
    }


    /// 
    /// Functions.
    /// 


    private void DetermineAvailableAttacks(){

        ClearAvailableAttacks();
        chosenAttack = null;

        // return if we are not in the max range attack.

        if(distanceToTarget > maxMinTargetDistanceAttack){
            // GD.Print(maxRangeAttack);
            return;
        }

        // directional attacks.

        switch(targetDirection){
            case TargetDirection.Up:
                GetAvailableAttacks(upAttacks, availableUpAttacks);
            break;
            case TargetDirection.Right:
                GetAvailableAttacks(rightAttacks, availableRightAttacks);
            break;
            case TargetDirection.Down:
                GetAvailableAttacks(downAttacks, availableDownAttacks);
            break;
            case TargetDirection.Left:
                GetAvailableAttacks(leftAttacks, availableLeftAttacks);
            break;
        }

        // omni directional attacks

        GetAvailableAttacks(omniAttacks, availableOmniAttacks);
    }

    private void ClearAvailableAttacks(){
        availableDownAttacks.Clear();
        availableLeftAttacks.Clear();
        availableOmniAttacks.Clear();
        availableRightAttacks.Clear();
        availableUpAttacks.Clear();
    }

    private void GetAvailableAttacks(Array<AiAttack> attackOptions, Array<AiAttack> availableAttackOptions){
        for(int i = 0; i < attackOptions.Count; i++){
            AiAttack attack = attackOptions[i];
            if(attack.MinTargetDistance < distanceToTarget
            || attackCooldowns[attack.Id].TimeLeft > 0){
                continue;
            }
            availableAttackOptions.Add(attack);
        }
    }

    private void ExecuteRandomAvailableAttack(){
        
        // return if there are no available attacks.
        if(availableDownAttacks.Count == 0 
        && availableLeftAttacks.Count == 0
        && availableOmniAttacks.Count == 0
        && availableRightAttacks.Count == 0
        && availableUpAttacks.Count == 0){
            return;
        }

        bool isAlternate = false;

        switch(targetDirection){
            case TargetDirection.Up:
                if(availableOmniAttacks.Count > 0){
                    ChooseRandomAttack(availableUpAttacks, availableOmniAttacks, out chosenAttack, out isAlternate);
                }
                else{
                    ChooseRandomAttack(availableUpAttacks, out chosenAttack);
                }
                chosenAttackDirection = isAlternate == false? AttackDirection.Up : AttackDirection.Omni;
                break;
            case TargetDirection.Right:
                if(availableOmniAttacks.Count > 0){
                    ChooseRandomAttack(availableRightAttacks, availableOmniAttacks, out chosenAttack, out isAlternate);
                }
                else{
                    ChooseRandomAttack(availableRightAttacks, out chosenAttack);
                }
                chosenAttackDirection = isAlternate == false? AttackDirection.Right : AttackDirection.Omni;
                break;
            case TargetDirection.Down:
                if(availableOmniAttacks.Count > 0){
                    ChooseRandomAttack(availableDownAttacks, availableOmniAttacks, out chosenAttack, out isAlternate);
                }
                else{
                    ChooseRandomAttack(availableDownAttacks, out chosenAttack);
                }
                chosenAttackDirection = isAlternate == false? AttackDirection.Down : AttackDirection.Omni;
                break;
            case TargetDirection.Left:
                if(availableOmniAttacks.Count > 0){
                    ChooseRandomAttack(availableLeftAttacks, availableOmniAttacks, out chosenAttack, out isAlternate);
                }
                else{
                    ChooseRandomAttack(availableLeftAttacks, out chosenAttack);                    
                }
                chosenAttackDirection = isAlternate == false? AttackDirection.Left : AttackDirection.Omni;
                break;
        }

        // choose a random attack.

        
        StartAttacking();
    }

    private void ChooseRandomAttack(Array<AiAttack> availableAttackOptions, Array<AiAttack> alternateAvailableAttackOptions, out AiAttack chosen, out bool isAlternate){
        int r = rng.RandiRange(0,1);
        if(r==0){
            ChooseRandomAttack(availableAttackOptions, out chosen);
            isAlternate = false;
        }
        else{
            ChooseRandomAttack(alternateAvailableAttackOptions, out chosen);
            isAlternate = true;
        }
    }

    private void ChooseRandomAttack(Array<AiAttack> availableAttackOptions, out AiAttack chosen){
        int r = rng.RandiRange(0,availableAttackOptions.Count-1);
        chosen = availableAttackOptions[r];
    }

    public void SetDistanceToTarget(float distanceToTarget){
        this.distanceToTarget = distanceToTarget;
    }

    public void SetDirectionToTarget(Vector2 directionToTarget){
        angleToTarget = Mathf.Atan2(directionToTarget.Y, directionToTarget.X);
        angleToTarget = Mathf.RadToDeg(angleToTarget);
    
        if(angleToTarget >= -135 && angleToTarget <= -45){
            targetDirection = TargetDirection.Up;
        }
        else if(angleToTarget >= -45 && angleToTarget <= 45){
            targetDirection = TargetDirection.Right;
        }
        else if(angleToTarget >= 45 && angleToTarget <= 135){
            targetDirection = TargetDirection.Down;
        }
        else{
            targetDirection = TargetDirection.Left;
        }
    }

    private void StartChoseAttackCooldown(){
        if(chosenAttack != null){
            Timer cooldownTimer = attackCooldowns[chosenAttack.Id]; 
            cooldownTimer.WaitTime = chosenAttack.Cooldown;
            cooldownTimer.Start();
        }
    }

    private void StartAttacking(){
        statePhysicsProcess = null;
        OnAttackStarted?.Invoke(chosenAttack.Id, chosenAttackDirection);
        LeadInState();
    }

    /// 
    /// Linkage
    /// 


    public void LinkEvents(){
        leadInStateTimer.Timeout            += AttackState;
        attackStateTimer.Timeout            += FollowThroughState;
        followThroughStateTimer.Timeout     += CooldownState; 
        standByCooldown.Timeout             += StandbyState;
    }

    private void UnlinkEvents(){
        leadInStateTimer.Timeout            -= AttackState;
        attackStateTimer.Timeout            -= FollowThroughState;
        followThroughStateTimer.Timeout     -= CooldownState; 
        standByCooldown.Timeout             -= StandbyState;
    }

    private enum AiAttackHandlerState : byte{
        Paused,
        StandBy,
        LeadIn,
        Attack,
        FollowThrough,
        Cooldown,
        Halted
    }
}
