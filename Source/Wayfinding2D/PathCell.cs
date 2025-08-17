using Godot;
using System;

namespace Entropek.Ai;

public struct PathCell2D{
    public Vector2I Id {get;private set;} = Vector2I.Zero;
    public Vector2I ParentId {get;private set;} = Vector2I.Zero;
    public int Cost {get;private set;} = -1;
    public int Heuristic {get;private set;} = -1;
    public int Total => Cost + Heuristic;

    public PathCell2D(){
        Id = new(-1,-1);
        ParentId = new(-1,-1);
        Cost = -1;
        Heuristic = -1;
    }

    public PathCell2D(Vector2I id, int cost, int heuristic){
        Id          = id;
        ParentId    = new(-1,-1);
        Cost        = cost;
        Heuristic   = heuristic;
    }

    public PathCell2D(Vector2I id, Vector2I parentId, int cost, int heuristic){
        Id          = id;
        ParentId    = parentId;
        Cost        = cost;
        Heuristic   = heuristic;
    }
}
