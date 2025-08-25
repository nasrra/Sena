using Entropek.Collections;
using Godot;
using System;

public partial class AvoidanceAgent : Area3D{

    public const string NodeName = nameof(AvoidanceAgent);

    /// 
    /// Variables.
    /// 


    private SwapbackList<AvoidancePoint> avoidancePoints = new SwapbackList<AvoidancePoint>();
    [Export] CollisionShape3D collisionShape;
    public Vector3 AvoidanceDirection {get;private set;} = Vector3.Zero;
    public float ProximityStrength {get;private set;} = 0.0f;
    private float radiusSqrd = 0.0f;
    [Export] private float smoothFactor = 1;


    /// 
    /// Base.
    /// 


    public override void _EnterTree(){
        base._EnterTree();
        LinkEvents();
        if(collisionShape.Shape is SphereShape3D shape){
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

    public override void _PhysicsProcess(double delta){
        AvoidanceDirection = AvoidanceDirection.MoveToward(CalculatAvoidanceDirection(), smoothFactor * (float)delta);
    }


    /// 
    /// Functions.
    /// 


    private void CollisionEntered(Node3D node){
        avoidancePoints.Add(node as AvoidancePoint);
    }

    private void CollisionExited(Node3D node){
        avoidancePoints.Remove(node as AvoidancePoint);
    }


    private Vector3 CalculatAvoidanceDirection() {
        if (avoidancePoints.Count == 0) {
            ProximityStrength = 0f;
            return Vector3.Zero;
        }

        Vector3 totalDir = Vector3.Zero;
        float totalStrength = 0f;

        foreach (AvoidancePoint point in avoidancePoints) {
            if (point == null) continue;

            // Direction from point to agent
            Vector3 dir = GlobalPosition - point.GlobalPosition;
            float distanceSqrd = dir.LengthSquared();

            if (distanceSqrd <= 0.0001f) {
                // Avoid division by zero if exactly overlapping
                dir = new Vector3(
                    (float)(GD.Randf() * 2 - 1),
                    (float)(GD.Randf() * 2 - 1),
                    (float)(GD.Randf() * 2 - 1)
                ).Normalized();
                distanceSqrd = 0.0001f;
            }

            // Compute influence: fully inside radius = full strength, ramps up outside radius
            float influence = 0f;
            float radiusSq = point.Radius * point.Radius;

            if (distanceSqrd < radiusSq) {
                influence = point.Strength;
            } else {
                // ramp from 0 at distance = radius * 2, to full at distance = radius
                float distance = MathF.Sqrt(distanceSqrd);
                float ramp = Mathf.Clamp((point.Radius * 2 - distance) / point.Radius, 0f, 1f);
                influence = ramp * point.Strength;
            }

            totalDir += dir.Normalized() * influence;
            totalStrength += influence;
        }

        ProximityStrength = totalStrength;

        if (totalDir == Vector3.Zero) return Vector3.Zero;

        // Optional smoothing
        return totalDir.Normalized() * ProximityStrength;
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
