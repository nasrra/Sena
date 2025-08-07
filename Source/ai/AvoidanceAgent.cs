using Entropek.Collections;
using Godot;
using System;

public partial class AvoidanceAgent : Area2D{


    /// 
    /// Variables.
    /// 


    private SwapbackList<AvoidancePoint> avoidancePoints = new SwapbackList<AvoidancePoint>();


    /// 
    /// Base.
    /// 


    public override void _EnterTree(){
        base._EnterTree();
        LinkEvents();
    }

    public override void _ExitTree(){
        base._ExitTree();
        UnlinkEvents();
    }


    /// 
    /// Functions.
    /// 


    private void CollisionEntered(Node2D node){
        avoidancePoints.Add(node as AvoidancePoint);
    }

    private void CollisionExited(Node2D node){
        avoidancePoints.Remove(node as AvoidancePoint);
    }

    public Vector2 GetAvoidanceDirection(){
        
        if(avoidancePoints.Count == 0){
            return Vector2.Zero;
        }

        Vector2 total = Vector2.Zero;
        float totalWeight = 0f;

        foreach(AvoidancePoint point in avoidancePoints){
            Vector2 offset = GlobalPosition - point.GlobalPosition;
            float distSqrd = offset.LengthSquared();

            // avoid divid by zero;
            if(distSqrd < 0.0001f){
                continue;
            }

            // stronger + closer = more influence.
            float weight = point.Strength / distSqrd;
            total += offset * weight;
            totalWeight += weight;
        }

        if(totalWeight == 0f){
            return Vector2.Zero;
        }

        // already blended and directionally biased.

        return total / totalWeight; 
    }


    /// 
    /// Linkage.
    /// 


    private void LinkEvents(){
        BodyEntered += CollisionEntered;
        AreaEntered += CollisionEntered;
        BodyExited  += CollisionExited;
        AreaExited  += CollisionExited;
    }

    private void UnlinkEvents(){
        BodyEntered -= CollisionEntered;
        AreaEntered -= CollisionEntered;
        BodyExited  -= CollisionExited;
        AreaExited  -= CollisionExited;
    }

}
