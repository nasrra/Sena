using Entropek.Ai;
using Godot;
using System;
using System.Collections.Generic;

public partial class WayfindingAgent2D : Node2D{
    public Stack<Vector2> Path {get; private set;}
    public Vector2 TargetPosition {get;private set;}
    public Vector2 CurrentPathPoint {get;private set;}
    public Vector2 DistanceToPathPoint {get;private set;}
    public event Action OnReachedTarget;
    private bool noPathPathAssignedLastTick = true;
    [Export] byte size = 1;
    [Export] byte endPathPointTolerance = 0;
    [Export] public NavigationType Capability {get;private set;}

    private double deltaCummulative = 0f;
    private const float PathTick = 1.67f * 2;

    public override void _PhysicsProcess(double delta){
        base._PhysicsProcess(delta);
    
    
        // if(Path!=null){
        //     foreach(Vector2 point in Path){
        //         GodotObject debugDraw = GetNode<GodotObject>("/root/DebugDraw2D");
        //         debugDraw.Call("rect",point, Vector2.One * 2f, new Color(1, 1, 0), 1f, 0.0167f);
        //     }
        // }
    }

    public bool CalculateNewPath(){
        Path = WayfindingGrid2D.Singleton.GetPath(GlobalPosition, TargetPosition, Capability, size, endPathPointTolerance);
        return Path != null;
    }

    public bool SetTargetPosition(Vector2 targetGlobalPosition, Vector2I randomStartOffset, Vector2I randomEndOffset, uint obstructionLayers){

        List<Vector2I> cellsAroundTarget = WayfindingGrid2D.Singleton.GetCellsInArea(targetGlobalPosition, randomStartOffset, randomEndOffset);		
		cellsAroundTarget = WayfindingGrid2D.Singleton.GetGroundClearanceCells(cellsAroundTarget, 2);
		
        PhysicsDirectSpaceState2D spaceState = GetWorld2D().DirectSpaceState;

        for(int i = 0; i < 6; i++){
            if(spaceState.IntersectRay(new PhysicsRayQueryParameters2D{
                From                = GlobalPosition,
                To                  = targetGlobalPosition,
                CollideWithAreas    = true,
                CollideWithBodies   = true,
                CollisionMask       = obstructionLayers,
            }).Count==0){
                // debug draw. 
                // GodotObject debugDraw = GetNode<GodotObject>("/root/DebugDraw2D");
                for(int j = 0; j < cellsAroundTarget.Count; j++){			
                    Vector2 cellPosition = WayfindingGrid2D.Singleton.IdToGlobalPosition(cellsAroundTarget[j]);
                    // debugDraw.Call("rect",cellPosition, Vector2.One*8, new Color(1,1,0f), 1f, 1f);
                }

                TargetPosition = WayfindingGrid2D.Singleton.IdToGlobalPosition(cellsAroundTarget[GD.RandRange(0, cellsAroundTarget.Count-1)]);
                // debugDraw.Call("rect",TargetPosition, Vector2.One*8, new Color(1,0,0), 1f, 1f);
                return true;
            }
        }

        TargetPosition = targetGlobalPosition;

        return false;
    }

    public void SetTargetPosition(Vector2 targetGlobalPosition){
        TargetPosition = targetGlobalPosition;
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
            if(Path.Count == 0){
                OnReachedTarget?.Invoke();
            }
        }
    }

}
