using Godot;
using System;

public partial class CameraController : Camera2D{
    [Export] public Node2D Target;
    [Export] public Vector2 FollowOffset = Vector2.Zero;
    [Export] private float followSpeed = 0.88f;

    public override void _Ready(){
        base._Ready();
    }


    public override void _Process(double delta){
        base._Process(delta);

        // call on late.

        CallDeferred("UpdateCamera", (float)delta);
    }

    private void UpdateCamera(float delta){
        GlobalPosition = GlobalPosition.Lerp(Target.GlobalPosition + Offset + FollowOffset, followSpeed * delta);
    }
}
