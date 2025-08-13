using System;
using System.Collections.Generic;
using Godot;

public partial class CharacterMovement : Node{
    [Export] CharacterBody3D character;    
    public const string NodeName = nameof(CharacterMovement);
    
    private Vector3 pausedVelocity  = Vector3.Zero;
    private Vector3 pausedDirection = Vector3.Zero;
    private Vector3 moveDirection   = Vector3.Zero;
    public  Vector3 MoveDirection {
        get => moveDirection;
        private set{
            moveDirection = value;
        }
    }
    public Vector3 Velocity {
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

            Vector3 trueDirection = moveDirection;

            // if there is movement.

            if (moveDirection.Length() > 0f){
                trueDirection = trueDirection.Normalized();
                Vector3 newVelocity = Velocity.MoveToward(trueDirection * TopSpeed, Acceleration * (float)delta);
                Velocity = newVelocity;
            }
            else{
                Vector3 newVelocity = Velocity.MoveToward(Vector3.Zero, Deceleration * (float)delta);
                Velocity = newVelocity;
            }

            character.MoveAndSlide();
    }


    /// 
    /// Functions.
    /// 


    public void Move(Vector3 direction){
        moveDirection = direction.Normalized();
        OnMoveDirectionUpdated?.Invoke();
    }

    public void Impulse(Vector3 velocity){
        character.Velocity += velocity;
    }

    public void ZeroDirection(){
        moveDirection = Vector3.Zero;
        OnMoveDirectionUpdated?.Invoke();
    }

    public void ZeroVelocity(){
        character.Velocity = Vector3.Zero;
    }

    public void Knockback(Vector3 velocity){
        ZeroDirection();
        ZeroVelocity();
        character.Velocity += velocity;
    }

    public void ScaleSpeed(float amount){
        SpeedModifier += amount;
        Acceleration = Mathf.Clamp(BaseAcceleration * SpeedModifier, 0, float.MaxValue);
        Deceleration = Mathf.Clamp(BaseDeceleration * SpeedModifier, 0, float.MaxValue);
        TopSpeed     = Mathf.Clamp(BaseTopSpeed     * SpeedModifier, 0, float.MaxValue);
    }

    public float GetMoveAngleRadians(){        
        float angle = Mathf.Atan2(moveDirection.Z, moveDirection.X);
        return angle;
    }

    public float GetMoveAngleDegrees(){
        float angle = Mathf.Atan2(moveDirection.Z, moveDirection.X);
        angle = Mathf.RadToDeg(angle);
        return angle;
    }

    public float GetVelocityAngleDegrees(){
        float angle = Mathf.Atan2(Velocity.Z, Velocity.X);
        angle = Mathf.RadToDeg(angle);
        return angle;
    }

    public bool HasCollisions(out List<KinematicCollision3D> collisions){
        collisions = null;
        
        int collisionCount = character.GetSlideCollisionCount();

        if(collisionCount <= 0){
            return false;
        }
        
        collisions = new List<KinematicCollision3D>();

        for(int i = 0; i < collisionCount; i++){
            KinematicCollision3D collision = character.GetSlideCollision(i);
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
        moveDirection   = Vector3.Zero;
        Velocity        = Vector3.Zero;
    }

    public void ResumeState(){
        paused = false;
        moveDirection   = pausedDirection;
        Velocity        = pausedVelocity;
        pausedDirection = Vector3.Zero;
        pausedVelocity  = Vector3.Zero;
    }

}