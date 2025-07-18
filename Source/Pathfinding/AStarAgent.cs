using Godot;
using System;
using System.Collections.Generic;

public partial class AStarAgent : Area2D{
    private const string NodeName = nameof(AStarAgent);
    [Export] AStarTileType agentType;
    private List<Vector2I> occupiedTiles = new List<Vector2I>();
    public bool Enabled {get;private set;} = true;
    private Rect2 globalAABB;

    public override void _Ready(){
        base._Ready();
        #if TOOLS
        Entropek.Util.Node.VerifyName(this, NodeName);
        #endif
        if(Enabled == true){
            Enable();
        }
    }

    public void Disable(){
        Remove();
    }

    public void Enable(){
        Insert();
    }

    private void Remove(){
        if(occupiedTiles.Count>0){
            PathfindingGrid.Instance.Remove(occupiedTiles, agentType);
            GD.Print("remove door");
        }
    }

    private void Insert(){
        CollisionShape2D shape = GetChild<CollisionShape2D>(0);
        RectangleShape2D rectShape = (RectangleShape2D)shape.Shape;

        Vector2 center = shape.GlobalPosition + rectShape.Size * 0.5f; // The world position of the shape
        Vector2 extents = rectShape.Size * 0.5f;

        globalAABB = new Rect2(center - extents, rectShape.Size);

        PathfindingGrid.Instance.Insert(globalAABB, agentType, out occupiedTiles);
        GD.Print("insert door");
    }

    public Queue<Vector2> GetPathToPosition(Vector2 targetGlobalPosition){
        return PathfindingGrid.Instance.GetPathToPoint(GlobalPosition, targetGlobalPosition);
    }

    public void Update(){
        Remove();
        
        if(Enabled == true){
            Insert();
        }
    }

    public override void _Draw(){
        base._Draw();
        // GodotObject debugDraw = GetNode<GodotObject>("/root/DebugDraw2D");
        // debugDraw.Call("rect",globalAABB.Position, globalAABB.Size, new Color(0, 1, 1), 2f, 20f);
        // GD.Print("draw");
    }

}
