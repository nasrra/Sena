using Entropek.Ai;
using Godot;
using System;
using System.Collections.Generic;

public partial class WayfindingAgent2D : Node2D{
    public Stack<Vector2> Path {get; private set;}
    [Export] byte size = 1;
    [Export] byte endPathPointTolerance = 0;
    [Export(PropertyHint.Flags)] public NavigationType Capability {get;private set;}

    public override void _PhysicsProcess(double delta){
        base._PhysicsProcess(delta);
    }

    public void CalculatePathToGlobalPosition(Vector2 endGlobalPosition){
        Path = WayfindingGrid2D.Instance.GetPath(GlobalPosition, endGlobalPosition, Capability, size, endPathPointTolerance);
    }

    public override void _Process(double delta){
        base._Process(delta);
        QueueRedraw();
    }

    public override void _Draw(){
        base._Draw();
        if(Path!=null){
            foreach(Vector2 point in Path){
                GodotObject debugDraw = GetNode<GodotObject>("/root/DebugDraw2D");
                debugDraw.Call("rect",point, Vector2.One * 2f, new Color(1, 1, 0), 1f, 0.0167f);
            }
        }
    }

}
