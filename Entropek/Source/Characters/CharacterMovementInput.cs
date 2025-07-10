using Godot;
using System;

public partial class CharacterMovementInput : Node{
    [Export]
    private CharacterMovement movement;
    private Vector2 movementInput = Vector2.Zero;

    public override void _Ready(){
        base._Ready();
        #if DEBUG
        Entropek.Util.Node.VerifyName(this, nameof(CharacterMovementInput));
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

        movement.Move(movementInput);
    }
}
