using Entropek.Collections;
using Godot;
using System;

public partial class AvoidanceAgent : Area2D{

    public const string NodeName = nameof(AvoidanceAgent);

    /// 
    /// Variables.
    /// 


    private SwapbackList<AvoidancePoint> avoidancePoints = new SwapbackList<AvoidancePoint>();
    [Export] CollisionShape2D collisionShape;
    public Vector2 AvoidanceDirection = Vector2.Zero;
    public float ProximityStrength = 0.0f;
    private float radiusSqrd = 0.0f;


    /// 
    /// Base.
    /// 


    public override void _EnterTree(){
        base._EnterTree();
        LinkEvents();
        if(collisionShape.Shape is CircleShape2D shape){
            radiusSqrd = collisionShape.Scale.X * collisionShape.Scale.Y;
        }
        else{
            throw new Exception($"{NodeName} must have a circle collision shape!");

        }
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

    public void CalculatAvoidanceDirection(){
        
        if(avoidancePoints.Count == 0){
            AvoidanceDirection = Vector2.Zero;
            return;
        }

        Vector2 total = Vector2.Zero;
        float totalWeight = 0f;
        float proximityTotal = 0f;
        int countedPoints = 0;

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

            float proximity = 1f - Mathf.Clamp(distSqrd / radiusSqrd, 0f, 1f);
            proximityTotal += proximity;
            countedPoints++;
        }

        if(countedPoints > 0){
            ProximityStrength = proximityTotal / countedPoints;
        }
        else{
            ProximityStrength = 0f;
        }

        if(totalWeight == 0f){
            AvoidanceDirection = Vector2.Zero;
            return;
        }

        // already blended and directionally biased.

        AvoidanceDirection = total / totalWeight; 
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
