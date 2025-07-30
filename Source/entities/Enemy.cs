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
    [Export] private AnimatedSprite2D animator;
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

    ///
    /// Base
    ///


    public override void _Ready(){
        base._Ready();
        EnemyManager.Instance.AddEnemy(this);
        animator.Play("IdleBackward");
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
        stateProcess        = null;
        statePhysicProcess  = ChaseStatePhysicsProcess;
    }

    private void ChaseStatePhysicsProcess(){
        if(IsInstanceValid(Target) && Target.IsInsideTree()==true){
            CalculateRelationshipToTarget();
            UpdateAttackHandler();
            MoveAlongPathToTarget();
            RunAnimation();
        }
    }

    public void StunState(float time){
        state = EnemyState.Stunned;
        stateProcess        = null;
        statePhysicProcess  = null;
        attackHandler.HaltState(time+stunStateAttackHandlerStandbyAdditiveTime);
        hitBoxHandler.DisableAllHitBoxes();

        float angle = characterMovement.GetVelocityAngleDegrees();
        GD.Print(angle);
        if(angle > -135 && angle < -45){
            animator.Play("HitBackward");
            animator.FlipH = false;
        }
        else if(angle > 45 && angle < 135){
            animator.Play("HitForward");
            animator.FlipH = false;
        }
        else if(angle > -45 && angle < 45){
            animator.Play("HitSide");
            animator.FlipH = false;
        }
        else{
            animator.Play("HitSide");
            animator.FlipH = true;
        }

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
        navAgent.CalculateNewPath(Target.GlobalPosition);
        navAgent.UpdateCurrentPathToTarget();
        characterMovement.Move(navAgent.CurrentPathPoint - GlobalPosition);
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

    public void IgnoreEnemyCollisionMask(float time){
        
        ignoreEnemyTimer.WaitTime = time;
        SetCollisionMaskValue(PhysicsManager.Singleton.GetPhysics2DLayerId("Enemy"), false);
        ignoreEnemyTimer.Start();
    }

    private void RespondToEnemyCollisionMask(){
        SetCollisionMaskValue(PhysicsManager.Singleton.GetPhysics2DLayerId("Enemy"), true);
    }

    private void RunAnimation(){
        float angle = characterMovement.GetMoveAngleDegrees();
        if(characterMovement.MoveDirection == Vector2.Zero){
            return;
        }
        if(angle >= -155 && angle <= -25){
            animator.Play("RunForward");
            animator.FlipH = false;
            return;
        }
        else if(angle >= 25 && angle <= 155){
            animator.Play("RunBackward");
            animator.FlipH = false;
            return;
        }
        else if(angle > -45 && angle < 45){
            animator.Play("RunSide");
            animator.FlipH = true;
            return;
        }
        else{
            animator.Play("RunSide");
            animator.FlipH = false;
            return;
        }
    }

    /// 
    /// Linkage
    /// 


    private void LinkEvents(){
        health.OnDeath  += Kill;
        health.OnDamage += OnDamaged;
        
        attackHandler.OnAttack          += OnAttack;
        attackHandler.OnAttackStarted   += OnStartAttack;
        attackHandler.OnAttackEnded     += OnAttackEnded;

        hitBoxHandler.OnHit += OnAttackHit;

        stunTimer.Timeout += EvaluateState;
        ignoreEnemyTimer.Timeout += RespondToEnemyCollisionMask;

        EntityManager.Singleton.OnProcess += Process;
        EntityManager.Singleton.OnPhysicsProcess += PhysicsProcess;
        EntityManager.Singleton.OnPause += PauseState;
        EntityManager.Singleton.OnResume += ResumeState;
    }

    private void UnlinkEvents(){
        health.OnDeath  -= Kill;
        health.OnDamage -= OnDamaged;

        attackHandler.OnAttack          -= OnAttack;
        attackHandler.OnAttackStarted   -= OnStartAttack;
        attackHandler.OnAttackEnded     -= OnAttackEnded;

        hitBoxHandler.OnHit -= OnAttackHit;

        stunTimer.Timeout -= EvaluateState;
        ignoreEnemyTimer.Timeout -= RespondToEnemyCollisionMask;

        EntityManager.Singleton.OnProcess -= Process;
        EntityManager.Singleton.OnPhysicsProcess -= PhysicsProcess;
        EntityManager.Singleton.OnPause -= PauseState;
        EntityManager.Singleton.OnResume -= ResumeState;
    }


    ///
    /// Linkage Functions.
    /// 


    private void OnStartAttack(byte attackId, AttackDirection attackDirection){
        AttackingState();
        switch(attackId){
            case (byte)AttackId.Slash:
                switch(attackDirection){
                    case AttackDirection.Down:
                        animator.Play("AttackBackward");
                        animator.FlipH = false;
                    break;
                    case AttackDirection.Left:
                        animator.Play("AttackSide");
                        animator.FlipH = false;
                    break;
                    case AttackDirection.Right:
                        animator.Play("AttackSide");
                        animator.FlipH = true;
                    break;
                    case AttackDirection.Up:
                        animator.Play("AttackForward");                 
                        animator.FlipH = false;
                    break;
                }
            break;
            default:
                throw new Exception($"Attack id[{attackId}] has not been implemented!");
        }
        characterMovement.ZeroDirection();
    }

    private void OnAttack(byte attackId, AttackDirection attackDirection){
        switch(attackId){
            case (byte)AttackId.Slash:
                switch(attackDirection){
                    case AttackDirection.Down:
                        hitBoxHandler.EnableHitBox((int)AttackHitBoxId.SlashDown, 0.33f);
                        animator.Play("AttackBackward");
                    break;
                    case AttackDirection.Left:
                        hitBoxHandler.EnableHitBox((int)AttackHitBoxId.SlashLeft, 0.33f);
                        animator.Play("AttackSide");
                    break;
                    case AttackDirection.Right:
                        hitBoxHandler.EnableHitBox((int)AttackHitBoxId.SlashRight, 0.33f);
                        animator.Play("AttackSide");
                    break;
                    case AttackDirection.Up:
                        hitBoxHandler.EnableHitBox((int)AttackHitBoxId.SlashUp, 0.33f);
                        animator.Play("AttackForward");                 
                    break;
                }
                characterMovement.Impulse(normalDirectionToTarget * 100f);
            break;
            default:
            throw new Exception($"Attack id[{attackId}] has not been implemented!");
        }
    }

    private void OnAttackHit(Node other, int hitboxId){
        string hitLayer = PhysicsManager.Singleton.GetPhysics2DLayerName((other as CollisionObject2D).CollisionLayer);
        switch (hitLayer){
            case "Player":
                Health playerHealth = other.GetNode<Health>(Health.NodeName);
                playerHealth.Damage(1);
            break;
            default:
            throw new Exception($"{hitLayer} not implemented.");
        }
    }

    private void OnDamaged(){
        float stunTime = 0.66f;
        hitFlash.Flash();
        StunState(stunTime);
        IgnoreEnemyCollisionMask(stunTime);
    }


    private void OnAttackEnded(){
        EvaluateState();
    }

    public void Kill(){
        EnemyManager.Instance.RemoveEnemy(this);
        QueueFree();
    }

}