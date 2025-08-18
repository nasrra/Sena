using Entropek.Ai;
using Godot;
using System;
using System.Collections.Generic;

public partial class WayfindingAgent3D : Node3D{
    public Stack<Vector3> Path {get; private set;}
    [Export] public Node3D target;
    public Vector3 TargetPosition {get;private set;}
    public Vector3 NextPathPoint {get;private set;}
    public Vector3 DistanceToPathPoint {get;private set;}
    public Vector3 DirectionToPathPoint {get;private set;}
    [Export(PropertyHint.Layers3DPhysics)] private uint floorLayer;
    public event Action OnReachedTarget;
    private bool noPathPathAssignedLastTick = true;
    [Export] byte size = 1;
    [Export] byte endPathPointTolerance = 0;
    [Export] public NavigationType Capability {get;private set;}

    private double deltaCummulative = 0f;
    private const float PathTick = 1.67f * 2;

    public override void _PhysicsProcess(double delta){
        base._PhysicsProcess(delta);

        TargetPosition = target.GlobalPosition;
        CalculateNewPath();

        if(Path!=null){
            foreach(Vector3 point in Path){
                DebugDraw3D.DrawBox(point, Quaternion.Identity, Vector3.One * 0.01f, new Color(1, 1, 0), true, 0.0167f);
            }
        }
    }

    public bool CalculateNewPath(){
        Path = WayfindingGrid3D.Singleton.GetPath(GlobalPosition, TargetPosition, Capability, size, endPathPointTolerance);
        return Path != null;
    }

    // public bool SetTargetPosition(Vector3 targetGlobalPosition, Vector3I randomStartOffset, Vector3I randomEndOffset, uint obstructionLayers){

    //     List<Vector3I> cellsAroundTarget = WayfindingGrid3D.Singleton.GetCellsInArea(targetGlobalPosition, randomStartOffset, randomEndOffset);		
	// 	cellsAroundTarget = WayfindingGrid3D.Singleton.GetGroundClearanceCells(cellsAroundTarget, 2);
		
    //     PhysicsDirectSpaceState2D spaceState = GetWorld2D().DirectSpaceState;

    //     for(int i = 0; i < 6; i++){
    //         if(spaceState.IntersectRay(new PhysicsRayQueryParameters2D{
    //             From                = GlobalPosition,
    //             To                  = targetGlobalPosition,
    //             CollideWithAreas    = true,
    //             CollideWithBodies   = true,
    //             CollisionMask       = obstructionLayers,
    //         }).Count==0){
    //             // debug draw. 
    //             // GodotObject debugDraw = GetNode<GodotObject>("/root/DebugDraw2D");
    //             for(int j = 0; j < cellsAroundTarget.Count; j++){			
    //                 Vector2 cellPosition = WayfindingGrid2D.Singleton.IdToGlobalPosition(cellsAroundTarget[j]);
    //                 // GodotObject debugDraw = GetNode<GodotObject>("/root/DebugDraw2D");
    //                 // debugDraw.Call("rect",cellPosition, Vector2.One, new Color(0,0.5f,1f), 8f, 1f);
    //             }

    //             TargetPosition = WayfindingGrid2D.Singleton.IdToGlobalPosition(cellsAroundTarget[GD.RandRange(0, cellsAroundTarget.Count-1)]);
    //             // debugDraw.Call("rect",TargetPosition, Vector2.One, new Color(1,0,0), 8f, 1f);
    //             return true;
    //         }
    //     }

    //     TargetPosition = targetGlobalPosition;

    //     return false;
    // }

    public void SetTargetPosition(Vector3 targetGlobalPosition){
        TargetPosition = targetGlobalPosition;
    }

    public void UpdateCurrentPathToTarget(){
        if(Path != null && Path.Count > 0){
            if(noPathPathAssignedLastTick == true){
                SetNextPathPoint();
            }
            Vector3 distance = NextPathPoint - GlobalPosition;
            float distSqrd = distance.LengthSquared();
            if( distSqrd <= 1f && Path.Count > 0){
                SetNextPathPoint();
            }
            if(Path.Count == 0){
                OnReachedTarget?.Invoke();
            }
        }
    }

    private void SetNextPathPoint(){
        
        NextPathPoint = Path.Pop();
        
        // floor-y to the ground level.
        
        Godot.Collections.Dictionary result = GetWorld3D().DirectSpaceState.IntersectRay(
            new PhysicsRayQueryParameters3D{
            From                = NextPathPoint + (Vector3.Up * 0.01f), // add small y offset for stairs.
            To                  = new Vector3(NextPathPoint.X, -byte.MaxValue, NextPathPoint.Z),
            CollideWithAreas    = true,
            CollideWithBodies   = true,
            CollisionMask       = floorLayer
        });

        if(result.Count <= 0){
            return;
        }

        Vector3 position = (Vector3)result["position"];

        DebugDraw3D.DrawBox(position, Quaternion.Identity, Vector3.One * 0.1f, new Color(1,1,1,1), true, 1);

        NextPathPoint = position;
    }
}
