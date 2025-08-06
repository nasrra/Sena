using Entropek.Ai;
using Godot;
using System;
using System.Collections.Generic;

public partial class WayfindingAgent2D : Node2D{
    public Stack<Vector2> Path {get; private set;}
    public Vector2 CurrentPathPoint {get;private set;}
    private bool noPathPathAssignedLastTick = true;
    [Export] byte size = 1;
    [Export] byte endPathPointTolerance = 0;
    [Export] public NavigationType Capability {get;private set;}

    private double deltaCummulative = 0f;
    private const float PathTick = 1.67f * 2;

    public override void _PhysicsProcess(double delta){
        base._PhysicsProcess(delta);
    }

    public bool CalculateNewPath(Vector2 endGlobalPosition){
        Path = WayfindingGrid2D.Singleton.GetPath(GlobalPosition, endGlobalPosition, Capability, size, endPathPointTolerance);
        return Path != null;
    }

    public void UpdateCurrentPathToTarget(){
        if(Path != null && Path.Count > 0){
            if(noPathPathAssignedLastTick == true){
                CurrentPathPoint = Path.Pop();
            }
            Vector2 distance = CurrentPathPoint - GlobalPosition;
            if(distance.LengthSquared() <= 50f && Path.Count > 0){
                CurrentPathPoint = Path.Pop();
            }
        }
    }

    public override void _Process(double delta){
        base._Process(delta);
        QueueRedraw();
    }

    public override void _Draw(){
        base._Draw();
        // if(Path!=null){
        //     foreach(Vector2 point in Path){
        //         GodotObject debugDraw = GetNode<GodotObject>("/root/DebugDraw2D");
        //         debugDraw.Call("rect",point, Vector2.One * 2f, new Color(1, 1, 0), 1f, 0.0167f);
        //     }
        // }
    }

}
