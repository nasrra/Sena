using Godot;
using System;

public partial class PlayerAimCursour : Node2D{
    [Export]
    public Node2D cursour {get;private set;}
    [Export]
    public Vector2 aimDirection {get;private set;} = Vector2.Zero;
    [Export]
    public float cursourDistance {get; private set;} = 2;
    [Export]
    public float aimAngle {get; private set;} = 0f;

    public override void _Process(double delta){
        base._PhysicsProcess(delta);
        
        aimDirection = (GetGlobalMousePosition()-GlobalPosition).Normalized();

        aimAngle = Mathf.Atan2(aimDirection.Y, aimDirection.X);
        aimAngle = Mathf.RadToDeg(aimAngle);

        cursour.GlobalPosition = GlobalPosition+aimDirection * cursourDistance;
    }
}
