using System;
using System.Collections.Generic;
using Godot;

public partial class CharacterMovement : Node{
    [Export] CharacterBody2D character;    
    public const string NodeName = nameof(CharacterMovement);
    
    private Vector2 pausedVelocity = Vector2.Zero;
    private Vector2 pausedDirection = Vector2.Zero;
    private Vector2 moveDirection  = Vector2.Zero;
    public  Vector2 MoveDirection {
        get => moveDirection;
        private set{
            moveDirection = value;
        }
    }
    public Vector2 Velocity {
        get => character.Velocity;
        private set{
            character.Velocity = value;
        }
    }

    public event Action OnMoveDirectionUpdated;

    [Export] public float BaseTopSpeed {get; private set;}
    [Export] public float BaseAcceleration {get; private set;}
    [Export] public float BaseDeceleration {get; private set;}

    public float SpeedModifier {get; private set;} = 0f;
    public float Deceleration;
    public float Acceleration;
    public float TopSpeed;
    [Export] public float gravityModifier = 1f;
    
    [Export] private bool lockYAxis = false;
    [Export] private bool gravityAffected = false;
    private bool paused = false;


    /// 
    /// Base.
    /// 


    public override void _Ready(){
        base._Ready();
    
        #if DEBUG
        Entropek.Util.Node.VerifyName(this, NodeName);
        #endif
    
        // set the base speeds.

        ScaleSpeed(1);
    }


    public override void _Process(double delta){
        base._Process(delta);

        if(paused == true){
            return;
        }

        Vector2 trueDirection = moveDirection;

        // if there is movement.

        if (moveDirection.Length() > 0f){
            if (lockYAxis==true){
                trueDirection.Y = 0.0f;
            }

            trueDirection = trueDirection.Normalized();

            // convert movement direction into velocity.

            Vector2 newVelocity = Velocity.MoveToward(trueDirection * TopSpeed, Acceleration * (float)delta);


            if(gravityAffected == true){
                // apply gravity.
                
                Velocity = new Vector2(
                    newVelocity.X, 
                    Velocity.Y - PhysicsManager.Singleton.Gravity * (float)delta * gravityModifier
                ); 
            }
            else{
                Velocity = newVelocity;
            }
        }

        // if there is no movement.

        else{
            Vector2 newVelocity = Velocity.MoveToward(Vector2.Zero, Deceleration * (float)delta);
            if(gravityAffected == true){
                newVelocity = new Vector2(
                    newVelocity.X, 
                    Velocity.Y - PhysicsManager.Singleton.Gravity * (float)delta * gravityModifier
                ); 
            }

            Velocity = newVelocity;
        }

        character.MoveAndSlide();
    }


    /// 
    /// Functions.
    /// 


    public void Move(Vector2 direction){
        moveDirection = direction.Normalized();
        OnMoveDirectionUpdated?.Invoke();
    }

    public void Impulse(Vector2 velocity){
        character.Velocity += velocity;
    }

    public void ZeroDirection(){
        moveDirection = Vector2.Zero;
        OnMoveDirectionUpdated?.Invoke();
    }

    public void ZeroVelocity(){
        character.Velocity = Vector2.Zero;
    }

    public void ScaleSpeed(float amount){
        SpeedModifier += amount;
        Acceleration = Mathf.Clamp(BaseAcceleration * SpeedModifier, 0, float.MaxValue);
        Deceleration = Mathf.Clamp(BaseDeceleration * SpeedModifier, 0, float.MaxValue);
        TopSpeed     = Mathf.Clamp(BaseTopSpeed     * SpeedModifier, 0, float.MaxValue);
    }

    public float GetMoveAngleRadians(){        
        float angle = Mathf.Atan2(moveDirection.Y, moveDirection.X);
        return angle;
    }

    public float GetMoveAngleDegrees(){
        float angle = Mathf.Atan2(moveDirection.Y, moveDirection.X);
        angle = Mathf.RadToDeg(angle);
        return angle;
    }

    public float GetVelocityAngleDegrees(){
        float angle = Mathf.Atan2(Velocity.Y, Velocity.X);
        angle = Mathf.RadToDeg(angle);
        return angle;
    }

    public bool HasCollisions(out List<KinematicCollision2D> collisions){
        collisions = null;
        
        int collisionCount = character.GetSlideCollisionCount();

        if(collisionCount <= 0){
            return false;
        }
        
        collisions = new List<KinematicCollision2D>();

        for(int i = 0; i < collisionCount; i++){
            KinematicCollision2D collision = character.GetSlideCollision(i);
            collisions.Add(collision);
        }

        return true;
    }



    /// 
    /// States.
    /// 


    public void PauseState(){
        paused = true;
        pausedDirection = moveDirection;
        pausedVelocity  = Velocity;
        moveDirection   = Vector2.Zero;
        Velocity        = Vector2.Zero;
    }

    public void ResumeState(){
        paused = false;
        moveDirection   = pausedDirection;
        Velocity        = pausedVelocity;
        pausedDirection = Vector2.Zero;
        pausedVelocity  = Vector2.Zero;
    }
}