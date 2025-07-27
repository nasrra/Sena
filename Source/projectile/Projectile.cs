using Godot;
using System;
using System.Collections.Generic;

public partial class Projectile : CharacterBody2D{
    [ExportGroup(nameof(Projectile))]
    [Export] public CharacterMovement Movement {get; private set;}
    [Export] private Sprite2D sprite;
    [Export] private Area2D collider;
    [Export] private float knockback;     
    [Export] private int damage;


    /// 
    /// Base.
    /// 

    public override void _EnterTree(){
        base._EnterTree();
        ProjectileManager.Instance.AddProjectile(this);
        LinkEvents();
    }

    public override void _Ready(){
        base._Ready();
        RotateToMoveDirection();
    }


    public override void _ExitTree(){
        base._ExitTree();
        ProjectileManager.Instance.RemoveProjectile(this);
        UnlinkEvents();
    }


    ///
    /// Functions.
    /// 


    protected void RotateToMoveDirection(){
        float angle = Movement.GetMoveAngleRadians();
        GlobalRotation = angle;
        GD.Print($"{GlobalRotation} {angle}");
    } 


    /// 
    /// Linkage.
    /// 


    protected virtual void LinkEvents(){
        
        EntityManager.Instance.OnPause  += HandlePause;
        EntityManager.Instance.OnResume += HandleResume;
        
        Movement.OnMoveDirectionUpdated += RotateToMoveDirection;

        collider.BodyEntered += OnCollision;
        collider.AreaEntered += OnCollision;
    }

    protected virtual void UnlinkEvents(){
        
        EntityManager.Instance.OnPause  -= HandlePause;
        EntityManager.Instance.OnResume -= HandleResume;

        Movement.OnMoveDirectionUpdated -= RotateToMoveDirection;

        collider.BodyEntered -= OnCollision;
        collider.AreaEntered -= OnCollision;
    }

    ///
    /// Linkage Functions.
    /// 

    private void HandlePause(){
        Movement.PauseState();
    }

    private void HandleResume(){
        Movement.ResumeState();
    }

    private void OnCollision(Node2D node){
        if(PhysicsManager.Instance.GetPhysics2DLayerName((node as CollisionObject2D).CollisionLayer, out string layerName) == false){
            return;
        }

        switch (layerName){
            case "Enemy":
                HandleOnHitEnemy(node as Enemy);
            break;
        }

        QueueFree();
    }

    private void HandleOnHitEnemy(Enemy enemy){
        Vector2 directionToHit = (enemy.GlobalPosition - GlobalPosition).Normalized();
        
        float stunTime = 0.33f;
        enemy.StunState(stunTime);
        enemy.IgnoreEnemyCollisionMask(stunTime);
        
        enemy.GetNode<Health>(Health.NodeName).Damage(damage);
        
        CharacterMovement enemyMovement = enemy.GetNode<CharacterMovement>(CharacterMovement.NodeName); 
        enemyMovement.ZeroVelocity();
        enemyMovement.Impulse(directionToHit * knockback);
    } 
}
