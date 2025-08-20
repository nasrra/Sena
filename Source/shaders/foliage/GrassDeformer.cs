using Godot;
using System;

public partial class GrassDeformer : Node3D{
    [Export] public float Radius {get; private set;} = 1f;
    [Export] public float Strength {get; private set;} = 0.1f;

    public override void _Ready(){
        base._Ready();
        GrassDeformerManager.Singleton?.AddDeformer(this);
    }

    public override void _ExitTree(){
        base._ExitTree();
        GrassDeformerManager.Singleton?.RemoveDeformer(this);
    }
}
