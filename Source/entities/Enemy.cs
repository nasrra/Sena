using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization.Metadata;

public partial class Enemy : CharacterBody2D{ // <-- make sure to inherit from CollisionObect2D for hitbox handler and Player.
    [Export] private Health health;
    [Export] private AStarAgent aStarAgent;
    [Export] private CharacterMovement characterMovement;
    [Export] private AiAttackHandler attackHandler;
    [Export] private HitBoxHandler hitBoxHandler;
    [Export] private HitFlashShaderController hitFlash;
    [Export] private Timer stunTimer;
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
        SlashDown = 0,
        SlashLeft = 1,
        SlashRight = 2,
        SlashUp = 3,
    }

    private Queue<Vector2> pathToTarget;


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

    public override void _Process(double delta){
        base._Process(delta);
        stateProcess?.Invoke();
    }

    public override void _PhysicsProcess(double delta){
        base._PhysicsProcess(delta);
        statePhysicProcess?.Invoke();
    }


    /// 
    /// State Machine
    /// 


    private void EvaluateState(){

        // TODO: do some recovery state code when needed.
        ChaseState();        
    }

    public void ChaseState(){
        GD.Print(state = EnemyState.Chase);
        stateProcess        = ChaseStateProcess;
        statePhysicProcess  = ChaseStatePhysicsProcess;
        attackHandler.ResumeState();
    }

    private void ChaseStateProcess(){
        MoveAlongPathToTarget();
    }

    private void ChaseStatePhysicsProcess(){
        CalculateRelationshipToTarget();
        GetPathToTarget();
        UpdateAttackHandler();
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


    /// 
    /// Shared State Function
    /// 


    private void MoveAlongPathToTarget(){
        if(pathToTarget != null && pathToTarget.Count > 0){
            Vector2 distance = pathToTarget.Peek() - GlobalPosition;
            characterMovement.Move(pathToTarget.Peek() - GlobalPosition);
            if(distance.LengthSquared() <= 100){
                pathToTarget.Dequeue();
            }
            // GD.Print(distance.LengthSquared());
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
        pathToTarget = aStarAgent.GetPathToPosition(Target.GlobalPosition);
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

        stunTimer.Timeout += EvaluateState;
    }

    private void UnlinkEvents(){
        health.OnDeath  -= Kill;
        health.OnDamage -= hitFlash.Flash;

        attackHandler.OnAttack          -= OnAttack;
        attackHandler.OnAttackStarted   -= OnStartAttack;
        attackHandler.OnAttackEnded     -= OnAttackEnded;

        stunTimer.Timeout -= EvaluateState;
    }

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
                characterMovement.Impulse(normalDirectionToTarget * 100f);
            break;
            default:
            throw new Exception($"Attack id[{attackId}] has not been implemented!");
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