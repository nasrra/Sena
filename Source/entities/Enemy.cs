using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization.Metadata;

public partial class Enemy : CharacterBody2D{ // <-- make sure to inherit from CollisionObect2D for hitbox handler and Player.
    [Export] private Health health;
    [Export] private WayfindingAgent2D navAgent;
    [Export] private CharacterMovement characterMovement;
    [Export] private AiAttackHandler attackHandler;
    [Export] private HitBoxHandler hitBoxHandler;
    [Export] private HitFlashShaderController hitFlash;
    [Export] private Timer stunTimer;
    [Export] private Timer ignoreEnemyTimer;
    [Export] public Node2D Target;
    private EnemyState state = EnemyState.Chase;

    private Vector2 directionToTarget = Vector2.Zero;
    private Vector2 normalDirectionToTarget = Vector2.Zero;
    private float distanceToTarget = float.MaxValue;
    [Export] public float stunStateAttackHandlerStandbyAdditiveTime = 1.0f;
    [Export] public bool stunOnHit = true;

    private event Action stateProcess = null;
    private event Action statePhysicProcess = null;

    private enum EnemyState{
        Chase,
        Stunned,
    }

    private enum AttackId : byte{
        Slash    = 0,
    }

    private enum AttackHitBoxId{
        SlashDown   = 0,
        SlashLeft   = 1,
        SlashRight  = 2,
        SlashUp     = 3,
    }

    private Stack<Vector2> pathToTarget;


    ///
    /// Base
    ///


    public override void _Ready(){
        base._Ready();
        EnemyManager.Instance.AddEnemy(this);
    }


    public override void _EnterTree(){
        base._EnterTree();
        LinkEvents();
        ChaseState();
    }

    public override void _ExitTree(){
        base._ExitTree();
        UnlinkEvents();
    }

    private void Process(double delta){
        stateProcess?.Invoke();
    }

    private void PhysicsProcess(double delta){
        statePhysicProcess?.Invoke();
    }


    /// 
    /// State Machine
    /// 


    private void EvaluateState(){

        // TODO: do some recovery state code when needed.
        ChaseState();
        attackHandler.EvaluateState();
    }

    public void ChaseState(){
        stateProcess        = ChaseStateProcess;
        statePhysicProcess  = ChaseStatePhysicsProcess;
    }

    private void ChaseStateProcess(){
        if(IsInstanceValid(Target) && Target.IsInsideTree()==true){
            MoveAlongPathToTarget();
        }
    }

    private void ChaseStatePhysicsProcess(){
        if(IsInstanceValid(Target) && Target.IsInsideTree()==true){
            CalculateRelationshipToTarget();
            GetPathToTarget();
            UpdateAttackHandler();
        }
    }

    public void StunState(float time){
        state = EnemyState.Stunned;
        stateProcess        = null;
        statePhysicProcess  = null;
        attackHandler.HaltState(time+stunStateAttackHandlerStandbyAdditiveTime);
        stunTimer.WaitTime = time;
        stunTimer.Start();
    }

    private void AttackingState(){
        stateProcess = null;
        statePhysicProcess = null;
    }

    private void PauseState(){
        stateProcess = null;
        statePhysicProcess = null;
        attackHandler.PauseState();
        characterMovement.PauseState();
    }

    private void ResumeState(){
        attackHandler.ResumeState();
        characterMovement.ResumeState();
        EvaluateState();
    }


    /// 
    /// Shared State Function
    /// 


    private void MoveAlongPathToTarget(){
        if(pathToTarget != null && pathToTarget.Count > 0){
            Vector2 distance = pathToTarget.Peek() - GlobalPosition;
            characterMovement.Move(pathToTarget.Peek() - GlobalPosition);
            if(distance.LengthSquared() <= 100){
                pathToTarget.Pop();
            }
        }
    }

    private void CalculateRelationshipToTarget(){
        directionToTarget = Target.GlobalPosition- GlobalPosition;
        normalDirectionToTarget = directionToTarget.Normalized();
        distanceToTarget = directionToTarget.Length();
    }

    private void UpdateAttackHandler(){
        attackHandler.SetDirectionToTarget(directionToTarget);
        attackHandler.SetDistanceToTarget(distanceToTarget);
    }

    private void GetPathToTarget(){
        navAgent.CalculatePathToGlobalPosition(Target.GlobalPosition);
        pathToTarget = navAgent.Path; 
    }

    public void IgnoreEnemyCollisionMask(float time){
        
        ignoreEnemyTimer.WaitTime = time;
        
        if(PhysicsManager.Instance.GetPhysics2DLayerId("Enemy", out int layerid)==true){
            SetCollisionMaskValue(layerid, false);
        }
        else{
            throw new Exception("Enemy physics layer not found!");
        }

        ignoreEnemyTimer.Start();
    }

    private void RespondToEnemyCollisionMask(){
        if(PhysicsManager.Instance.GetPhysics2DLayerId("Enemy", out int layerid)==true){
            SetCollisionMaskValue(layerid, true);
        }
        else{
            throw new Exception("Enemy physics layer not found!");
        }
    }


    /// 
    /// Linkage
    /// 


    private void LinkEvents(){
        health.OnDeath  += Kill;
        health.OnDamage += hitFlash.Flash;
        
        attackHandler.OnAttack          += OnAttack;
        attackHandler.OnAttackStarted   += OnStartAttack;
        attackHandler.OnAttackEnded     += OnAttackEnded;

        hitBoxHandler.OnHit += OnAttackHit;

        stunTimer.Timeout += EvaluateState;
        ignoreEnemyTimer.Timeout += RespondToEnemyCollisionMask;

        EntityManager.Instance.LinkToProcess(Process);
        EntityManager.Instance.LinkToPhysicsProcess(PhysicsProcess);
        EntityManager.Instance.LinkToPause(PauseState);
        EntityManager.Instance.LinkToResume(ResumeState);
    }

    private void UnlinkEvents(){
        health.OnDeath  -= Kill;
        health.OnDamage -= hitFlash.Flash;

        attackHandler.OnAttack          -= OnAttack;
        attackHandler.OnAttackStarted   -= OnStartAttack;
        attackHandler.OnAttackEnded     -= OnAttackEnded;

        hitBoxHandler.OnHit -= OnAttackHit;

        stunTimer.Timeout -= EvaluateState;
        ignoreEnemyTimer.Timeout -= RespondToEnemyCollisionMask;
        EntityManager.Instance.UnlinkFromProcess(Process);
        EntityManager.Instance.UnlinkFromPhysicsProcess(PhysicsProcess);
        EntityManager.Instance.UnlinkFromPause(PauseState);
        EntityManager.Instance.UnlinkFromResume(ResumeState);
    }


    ///
    /// Linkage Functions.
    /// 


    private void OnStartAttack(byte attackId, AttackDirection attackDirection){
        AttackingState();
    }

    private void OnAttack(byte attackId, AttackDirection attackDirection){
        switch(attackId){
            case (byte)AttackId.Slash:
                switch(attackDirection){
                    case AttackDirection.Down:
                        hitBoxHandler.EnableHitBox((int)AttackHitBoxId.SlashDown, 0.33f);
                    break;
                    case AttackDirection.Left:
                        hitBoxHandler.EnableHitBox((int)AttackHitBoxId.SlashLeft, 0.33f);
                    break;
                    case AttackDirection.Right:
                        hitBoxHandler.EnableHitBox((int)AttackHitBoxId.SlashRight, 0.33f);
                    break;
                    case AttackDirection.Up:
                        hitBoxHandler.EnableHitBox((int)AttackHitBoxId.SlashUp, 0.33f);                    
                    break;
                }
                characterMovement.Impulse(normalDirectionToTarget * 50f);
                characterMovement.ZeroDirection();
            break;
            default:
            throw new Exception($"Attack id[{attackId}] has not been implemented!");
        }
    }

    private void OnAttackHit(Node other, int hitboxId){
        if(PhysicsManager.Instance.GetPhysics2DLayerName((other as CollisionObject2D).CollisionLayer, out string hitLayer) == false){
            return;
        }

        switch (hitLayer){
            case "Player":
                Health playerHealth = other.GetNode<Health>(Health.NodeName);
                playerHealth.Damage(1);
            break;
            default:
            throw new Exception($"{hitLayer} not implemented.");
        }
    }


    private void OnAttackEnded(){
        EvaluateState();
    }

    public void Kill(){
        EnemyManager.Instance.RemoveEnemy(this);
        QueueFree();
    }

}