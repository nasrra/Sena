using Godot;
using System;
using System.Collections.Generic;

public partial class Enemy : CharacterBody2D{ // <-- make sure to inherit from CollisionObect2D for hitbox handler and Player.
    [Export] private Health health;
    [Export] private AStarAgent aStarAgent;
    [Export] private CharacterMovement characterMovement;
    [Export] private AiAttackHandler attackHandler;
    [Export] private HitFlashShaderController hitFlash;
    [Export] private Timer stunTimer;
    [Export] public Node2D Target;
    private EnemyState state = EnemyState.Chase;

    private Vector2 directionToTarget = Vector2.Zero;
    private float distanceToTarget = float.MaxValue;

    private event Action stateProcess = null;
    private event Action statePhysicProcess = null;

    private enum EnemyState{
        Chase,
        Stunned,
    }

    private enum AttackId : byte{
        Down    = 0,
        Left    = 1,
        Right   = 2,
        Up      = 3,
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
        stunTimer.WaitTime = time;
        stunTimer.Start();
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
    /// Functions.
    /// 


    private void Attack(byte attack){
        switch(attack){
            case (byte)AttackId.Down:
                GD.Print("down");
            break;
            case (byte)AttackId.Left:
                GD.Print("left");
            break;
            case (byte)AttackId.Right:
                GD.Print("Right");
            break;
            case (byte)AttackId.Up:
                GD.Print("Up");
            break;
        }
    }

    public void Kill(){
        EnemyManager.Instance.RemoveEnemy(this);
        QueueFree();
    }


    /// 
    /// Linkage
    /// 


    private void LinkEvents(){
        health.OnDeath += Kill;
        health.OnDamage += hitFlash.Flash;
        attackHandler.OnAttackChosen += Attack;
        stunTimer.Timeout += EvaluateState;
    }

    private void UnlinkEvents(){
        health.OnDeath -= Kill;
        health.OnDamage -= hitFlash.Flash;
        attackHandler.OnAttackChosen -= Attack;
        stunTimer.Timeout -= EvaluateState;
    }
}