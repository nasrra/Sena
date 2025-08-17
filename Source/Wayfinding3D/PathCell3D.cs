using Godot;
using System;

namespace Entropek.Ai;

public struct PathCell3D{
    public Vector3I ParentId {get;private set;} = Vector3I.Zero;
    public int Cost {get;private set;} = -1;
    public int Heuristic {get;private set;} = -1;
    public int Total {get;private set;} = -1;

    public PathCell3D(){
        ParentId = new(-1,-1,-1);
        Cost = -1;
        Heuristic = -1;
        Total = -1;
    }

    public PathCell3D(int cost, int heuristic){
        ParentId    = new(-1,-1,-1);
        Cost        = cost;
        Heuristic   = heuristic;
        Total       = Cost + Heuristic;
    }

    public PathCell3D(Vector3I parentId, int cost, int heuristic){
        ParentId    = parentId;
        Cost        = cost;
        Heuristic   = heuristic;
        Total       = Cost + Heuristic;
    }
}
