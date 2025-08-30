using Godot;
using System;

public partial class SpriteSorter3D : Node3D{
    [Export] private VisualInstance3D sprite;

    public override void _Ready(){
        base._Ready();
        UpdateSortingOffset();
    }

    public void UpdateSortingOffset(){
        sprite.SortingOffset = GlobalPosition.Z;
    }
}
