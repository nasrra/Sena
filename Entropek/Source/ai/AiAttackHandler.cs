using Godot;
using System;
using Godot.Collections;

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
    [Export] private Array<AiAttack> omniDirectionalAttacks;
    private Array<AiAttack> availableAttacks = new Array<AiAttack>(); // available attacks to use in a given frame.
    
    public event Action<byte> OnAttackStarted;
    public event Action<byte> OnLeadIn;
    public event Action<byte> OnAttack;
    public event Action<byte> OnFollowThrough;
    public event Action OnAttackEnded;
    private event Action statePhysicsProcess = null;

    [Export] private HitBoxHandler hitBoxHandler;
    [Export] private Timer leadInStateTimer;
    [Export] private Timer attackStateTimer;
    [Export] private Timer followThroughStateTimer;
    [Export] private Timer cooldown;
    private AiAttack chosenAttack = null;
    private RandomNumberGenerator rng = new RandomNumberGenerator();
    private TargetDirection targetDirection = TargetDirection.Right;


    private float angleToTarget     = 0;
    private float distanceToTarget  = float.MaxValue;
    private float maxMinTargetDistanceAttack = float.MinValue;


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

        for(int i = 0; i < omniDirectionalAttacks.Count; i++){
            AiAttack attack = omniDirectionalAttacks[i];
            if(attack.MinTargetDistance > maxMinTargetDistanceAttack){
                maxMinTargetDistanceAttack = attack.MinTargetDistance;
            }
        }
    }


    ///
    /// State Machine.
    ///

    
    private void StandbyState(){
        statePhysicsProcess = StandByStatePhysicsProccess;
    }

    private void StandByStatePhysicsProccess(){
        DetermineAvailableAttacks();
        ExecuteRandomAvailableAttack();
    }

    private void AttackingState(){
        statePhysicsProcess = null;
    }

    public void HaltState(){
        leadInStateTimer.Stop();
        attackStateTimer.Stop();
        followThroughStateTimer.Stop();
        hitBoxHandler.DisableAllHitBoxes();
        statePhysicsProcess = null;
    }

    public void ResumeState(){
        StandbyState();
    }


    /// 
    /// Functions.
    /// 


    private void DetermineAvailableAttacks(){
        availableAttacks.Clear();
        
        // return if we are not in the max range attack.

        if(distanceToTarget > maxMinTargetDistanceAttack){
            // GD.Print(maxRangeAttack);
            return;
        }

        // directional attacks.

        switch(targetDirection){
            case TargetDirection.Up:
                for(int i = 0; i < upAttacks.Count; i++){
                    AiAttack attack = upAttacks[i];
                    if(attack.MinTargetDistance < distanceToTarget){
                        continue;
                    }
                    availableAttacks.Add(attack);
                }
            break;
            case TargetDirection.Right:
                for(int i = 0; i < rightAttacks.Count; i++){
                    AiAttack attack = rightAttacks[i];
                    if(attack.MinTargetDistance < distanceToTarget){
                        continue;
                    }
                    availableAttacks.Add(attack);
                }
            break;
            case TargetDirection.Down:
                for(int i = 0; i < downAttacks.Count; i++){
                    AiAttack attack = downAttacks[i];
                    if(attack.MinTargetDistance < distanceToTarget){
                        continue;
                    }
                    availableAttacks.Add(attack);
                }
            break;
            case TargetDirection.Left:
                for(int i = 0; i < leftAttacks.Count; i++){
                    AiAttack attack = leftAttacks[i];
                    if(attack.MinTargetDistance < distanceToTarget){
                        continue;
                    }
                    availableAttacks.Add(attack);
                }
            break;
        }

        // omni directional attacks

        for(int i = 0; i < omniDirectionalAttacks.Count; i++){
            AiAttack attack = omniDirectionalAttacks[i];
            if(attack.MinTargetDistance < distanceToTarget){
                continue;
            }
            availableAttacks.Add(attack);
        }
    }

    private void ExecuteRandomAvailableAttack(){
        
        // return if there are no available attacks.
        
        if(availableAttacks.Count <= 0){
            return;
        }

        // choose a random attack.

        int r = rng.RandiRange(0,availableAttacks.Count-1);
        chosenAttack = availableAttacks[r];
        
        StartAttacking();
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


    /// 
    /// Attacking States.
    /// 


    private void StartAttacking(){
        AttackingState();
        OnAttackStarted?.Invoke(chosenAttack.Id);
        StartLeadInTimer();
    }

    private void StartLeadInTimer(){
        leadInStateTimer.WaitTime = chosenAttack.LeadInTime;
        leadInStateTimer.Start();
        OnLeadIn?.Invoke(chosenAttack.Id);
    }

    private void StartAttackStateTimer(){
        attackStateTimer.WaitTime = chosenAttack.AttackTime;
        hitBoxHandler.EnableHitBox(chosenAttack.HitBoxId, chosenAttack.AttackTime);
        attackStateTimer.Start();
        OnAttack?.Invoke(chosenAttack.Id);
    }

    private void StartFollowThroughStateTimer(){
        followThroughStateTimer.WaitTime = chosenAttack.FollowThroughTime;
        followThroughStateTimer.Start();
        OnFollowThrough?.Invoke(chosenAttack.Id);
    }

    private void AttackEnded(){
        StandbyState();
        OnAttackEnded?.Invoke();
    }


    /// 
    /// Linkage
    /// 


    public void LinkEvents(){
        leadInStateTimer.Timeout    += StartAttackStateTimer;
        attackStateTimer.Timeout    += StartFollowThroughStateTimer;
        followThroughStateTimer.Timeout  += AttackEnded; 
    }

    private void UnlinkEvents(){
        leadInStateTimer.Timeout    -= StartAttackStateTimer;
        attackStateTimer.Timeout    -= StartFollowThroughStateTimer;
        followThroughStateTimer.Timeout  -= AttackEnded; 
    }
}
