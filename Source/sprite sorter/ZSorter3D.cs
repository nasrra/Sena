using Godot;
using System;

public partial class ZSorter3D : Node{
    [Export] VisualInstance3D visualInstance;
    [Export] private float zOffset;

    public override void _EnterTree(){
        base._EnterTree();
        UpdateSortingOffset();
    }

    public void UpdateSortingOffset(){
        visualInstance.SortingOffset = visualInstance.GlobalPosition.Z + zOffset;
    }
}
