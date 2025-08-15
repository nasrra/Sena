using Godot;
using System;

public partial class SpriteSorter3D : Node3D{
    [Export] private SpriteBase3D sprite;

    public override void _Ready(){
        base._Ready();
        UpdateRenderPriority();
    }

    public override void _PhysicsProcess(double delta){
        base._PhysicsProcess(delta);
    }


    public void UpdateRenderPriority(){
        sprite.SortingOffset = GlobalPosition.Z;
    }
}
