using System;
using Godot;

public partial class CharacterMovement : Node{
    [Export] CharacterBody2D character;
    public const string NodeName = nameof(CharacterMovement);
    public Vector2 Velocity {
        get => character.Velocity;
        private set{
            character.Velocity = value;
        }
    }
    private Vector2 direction  = Vector2.Zero;
    public Vector2 Direction {
        get => direction;
        private set{
            direction = value;
        }
    }
    [Export] public float BaseTopSpeed {get; private set;}
    [Export] public float BaseAcceleration {get; private set;}
    [Export] public float BaseDeceleration {get; private set;}

    public float SpeedModifier {get; private set;} = 0f;

    public float Deceleration;
    public float Acceleration;
    public float TopSpeed;

    [Export] public float gravityModifier = 1f;
    [Export] private bool lockYAxis = true;
    [Export] private bool gravityAffected = true;

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

        Vector2 trueDirection = direction;

        // if there is movement.

        if (direction.Length() > 0f){
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
                    Velocity.Y - PhysicsManager.Instance.Gravity * (float)delta * gravityModifier
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
                    Velocity.Y - PhysicsManager.Instance.Gravity * (float)delta * gravityModifier
                ); 
            }

            Velocity = newVelocity;
        }
        character.MoveAndSlide();
    }

    public override void _PhysicsProcess(double delta){
        base._PhysicsProcess(delta);
        direction = Vector2.Zero;
    }


    public void Move(Vector2 direction){
        this.direction = direction;
    }

    public void Impulse(Vector2 velocity){
        character.Velocity += velocity;
    }

    public void ScaleSpeed(float amount){
        SpeedModifier += amount;
        Acceleration = Mathf.Clamp(BaseAcceleration * SpeedModifier, 0, float.MaxValue);
        Deceleration = Mathf.Clamp(BaseDeceleration * SpeedModifier, 0, float.MaxValue);
        TopSpeed     = Mathf.Clamp(BaseTopSpeed     * SpeedModifier, 0, float.MaxValue);
    }
}