using Godot;
using System;

public partial class Player : CharacterBody2D{
    [Export] private CameraController camera;
    [Export] private CharacterMovement movement;
    private Vector2 movementInput = Vector2.Zero;

    public override void _Ready(){
        base._Ready();
        #if DEBUG
        Entropek.Util.Node.VerifyName(this, nameof(Player));
        #endif
    }


    public override void _PhysicsProcess(double delta){
        base._PhysicsProcess(delta);
        movementInput = Vector2.Zero;

        if(Input.IsActionPressed("MoveUp")){
            movementInput.Y -= 1;
        }
        if(Input.IsActionPressed("MoveDown")){
            movementInput.Y += 1;
        }
        if(Input.IsActionPressed("MoveLeft")){
            movementInput.X -= 1;
        }
        if(Input.IsActionPressed("MoveRight")){
            movementInput.X += 1;
        }
        if(movementInput.LengthSquared() >= 1){
            camera.FollowOffset = movementInput*50;
        }
    }

    public override void _Process(double delta){
        base._Process(delta);
        movement.Move(movementInput);
    }

}
