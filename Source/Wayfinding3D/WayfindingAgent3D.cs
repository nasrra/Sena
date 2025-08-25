using Entropek.Ai;
using Godot;
using System;
using System.Collections.Generic;

// move to wayfinding grid 3d.

public partial class WayfindingAgent3D : Node3D{

    public event Action OnReachedTarget;
    private event Action<double> PhysicsProcess;
    [Export] private Node3D target;
    [Export] private Timer refreshPathTimer;
    private Stack<Vector3> path;
    private Vector3 lastValidNavigationCell;
    private Vector3 targetLastValidNavigationCell;
    private Vector3 nextPathPoint;
    public Vector3 DirectionToNextPathPoint {get; private set;}
    [Export(PropertyHint.Layers3DPhysics)] private uint floorLayer;
    int navigationLayer;
    private bool paused = false;
    [Export] byte size = 1;
    [Export] byte endPathPointTolerance = 0;
    [Export] public NavigationType Capability {get;private set;}
    private bool targetIsLinked = false;


    public override void _Ready(){
        base._Ready();
        navigationLayer = WayfindingGrid3D.Singleton.GetNavigationLayer(Capability);
        LinkEvents();
    }

    public override void _ExitTree(){
        base._ExitTree();
        UnlinkEvents();
        target.QueueFree();
    }


    public override void _PhysicsProcess(double delta){
        if(path!=null){
            foreach(Vector3 point in path){
                DebugDraw3D.DrawBox(point, Quaternion.Identity, Vector3.One * 0.01f, new Color(1, 1, 0), true, 0.0167f);
            }
        }
        
        if(paused== true){
            return;
        }

        base._PhysicsProcess(delta);
        PhysicsProcess?.Invoke(delta);
    }


    /// 
    /// last valid cell.
    /// 


    private (bool,bool) VerifyTargetPosition(){
        Vector3I cellPosition = WayfindingGrid3D.Singleton.LocalToMap(target.GlobalPosition);
        bool navigable = WayfindingGrid3D.Singleton.IsCellNavigable(cellPosition, Capability); 
        bool clearance = WayfindingGrid3D.Singleton.CellHasClearance(cellPosition, navigationLayer, size);
        bool isValid = clearance == true && navigable == true;
        if(isValid==true){
            targetLastValidNavigationCell = target.GlobalPosition;
        }
        return (navigable,clearance);
    }

    private (bool,bool) VerifyCellPosition(){
        Vector3I cellPosition = WayfindingGrid3D.Singleton.LocalToMap(GlobalPosition);
        bool navigable = WayfindingGrid3D.Singleton.IsCellNavigable(cellPosition, Capability); 
        bool clearance = WayfindingGrid3D.Singleton.CellHasClearance(cellPosition, navigationLayer, size);
        bool isValid = clearance == true && navigable == true;
        if(isValid==true){
            lastValidNavigationCell = GlobalPosition;
        }
        return (navigable,clearance);
    }



    /// 
    /// Path Handling.
    /// 


    public void StartFollowingTarget(Node3D target){
        this.target.GetParent().RemoveChild(this.target);
        this.target.Position = Vector3.Zero;
        target.AddChild(this.target);
        LinkToTargetParent();
        targetIsLinked = true;

        VerifyTargetPosition();
        VerifyCellPosition();
        CalculateNewPath();
        refreshPathTimer.Start();
        PhysicsProcess = FollowingPhysicsProcess;
    }

    public void StartFollowingTarget(Vector3 targetPosition){
        this.target.GetParent().RemoveChild(this.target);
        GetTree().Root.GetChild(0).AddChild(target);
        target.GlobalPosition = targetPosition;
        VerifyTargetPosition();
        VerifyCellPosition();
        targetIsLinked = false;

        CalculateNewPath();
        refreshPathTimer.Start();
        PhysicsProcess = FollowingPhysicsProcess;
    }

    private void FollowingPhysicsProcess(double delta){
        UpdateCurrentPathToTarget();
    }

    public void StopFollowingTarget(){
        refreshPathTimer.Stop();
        PhysicsProcess = null;
        targetIsLinked = false;
    }

    private void LinkToTargetParent(){
        target.GetParent().TreeExiting += UnlinkFromTargetParent;
    }

    private void UnlinkFromTargetParent(){

        // go to last known position before the node is freed.
        if(targetIsLinked == false){
            return;
        }        
        target.GetParent().TreeExiting -= UnlinkFromTargetParent;
    }

    public bool CalculateNewPath(){
        VerifyTargetPosition();
        (bool,bool) validity = VerifyCellPosition();
        
        switch(validity){

            // go straight to target if we are in a navigable cell with enough clearance for us.
            
            case (true,true):
                path = WayfindingGrid3D.Singleton.GetPath(GlobalPosition, targetLastValidNavigationCell, Capability, size, endPathPointTolerance);
                if(path!=null){
                    SetNextPathPoint();
                }
                return path != null;
            
            // return to our previous valid position
            
            case (true, false):
                path = WayfindingGrid3D.Singleton.GetPath(GlobalPosition, lastValidNavigationCell, Capability, 0, endPathPointTolerance);
                if(path != null){
                    SetNextPathPoint();
                    return true; 
                }
                goto default;
            

            // fall back to a straight line to last navigable cell.

            default:
                path = new Stack<Vector3>();
                path.Push(lastValidNavigationCell);
                SetNextPathPoint();
                return false;
        }
    }

    public void UpdateCurrentPathToTarget(){
        if(path != null && path.Count > 0){
            Vector3 distance = nextPathPoint - GlobalPosition;
            DirectionToNextPathPoint = distance.Normalized();
            float distSqrd = distance.LengthSquared();
            if( distSqrd <= 0.5f && path.Count > 0){
                SetNextPathPoint();
            }
            if(path.Count == 0){
                OnReachedTarget?.Invoke();
            }
        }
    }

    private void SetNextPathPoint(){
        
        nextPathPoint = path.Pop();
        
        // floor-y to the ground level.
        
        Godot.Collections.Dictionary result = GetWorld3D().DirectSpaceState.IntersectRay(
            new PhysicsRayQueryParameters3D{
            From                = nextPathPoint + (Vector3.Up * 0.01f), // add small y offset for stairs.
            To                  = new Vector3(nextPathPoint.X, -byte.MaxValue, nextPathPoint.Z),
            CollideWithAreas    = true,
            CollideWithBodies   = true,
            CollisionMask       = floorLayer
        });

        if(result.Count <= 0){
            return;
        }

        Vector3 position = (Vector3)result["position"];

        // DebugDraw3D.DrawBox(position, Quaternion.Identity, Vector3.One * 0.1f, new Color(1,1,1,1), true, 1);

        nextPathPoint = position;
    }

    public bool StartFollowingTarget(Vector3 targetPosition, Vector3 areaMin, Vector3 areaMax, uint obstructionLayers){
        List<Vector3> cellsAroundTarget = WayfindingGrid3D.Singleton.GetNavigableCellsInArea(targetPosition + areaMin, targetPosition + areaMax, navigationLayer, Capability, size);				
        
        PhysicsDirectSpaceState3D spaceState = GetWorld3D().DirectSpaceState;

        for(int i = 0; i < 6; i++){
            Vector3 chosenPosition = cellsAroundTarget[GD.RandRange(0,cellsAroundTarget.Count-1)];
            if(spaceState.IntersectRay(new PhysicsRayQueryParameters3D{
                From                = chosenPosition,
                To                  = targetPosition,
                CollideWithAreas    = true,
                CollideWithBodies   = true,
                CollisionMask       = obstructionLayers,
            }).Count==0){
                DebugDraw3D.DrawBox(chosenPosition, Quaternion.Identity, Vector3.One * 0.1f, new Color(1,1,0,1), true, 1);
                StartFollowingTarget(chosenPosition);
                UnlinkFromTargetParent();
                return true;
            }
        }
        return false;
    }


    /// 
    /// states.
    /// 


    public void Pause(){
        paused = true;
        refreshPathTimer.Paused = true;
    }

    public void Resume(){
        paused = false;
        refreshPathTimer.Paused = false;
    }


    ///
    ///  Linkage.
    /// 


    private void LinkEvents(){
        refreshPathTimer.Timeout += RefreshPath;
    }

    private void UnlinkEvents(){
        refreshPathTimer.Timeout -= RefreshPath;
        UnlinkFromTargetParent();
    }


    private void RefreshPath(){
        bool pathCalculated = CalculateNewPath();
    }
}
