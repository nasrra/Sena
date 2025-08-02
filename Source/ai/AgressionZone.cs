using Entropek.Collections;
using Godot;
using System;
using System.Collections.Generic;

public partial class AgressionZone : Node2D{
    [Export] private Area2D passiveZone;
    [Export] private Area2D activeZone;
    [Export(PropertyHint.Layers2DPhysics)] private uint obstructionLayer;

    private SwapbackList<Node2D> collisions = new SwapbackList<Node2D>();
    private SwapbackList<Node2D> isInSight = new SwapbackList<Node2D>();
    private SwapbackList<Node2D> notInSight = new SwapbackList<Node2D>();
    public event Action<Node2D> OnInSight;
    public event Action<Node2D> OnLeftSight;
    public event Action<Node2D> OnEnteredZone;
    public event Action<Node2D> OnExitedZone;


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
    /// Fuinctions.
    /// 


    public void Evaluate(){

        PhysicsDirectSpaceState2D spaceState = GetWorld2D().DirectSpaceState;

        for(int i = 0; i < collisions.Count; i++){
            
            Node2D other = collisions[i];

            PhysicsRayQueryParameters2D parameters = new PhysicsRayQueryParameters2D{
                From                = GlobalPosition,
                To                  = other.GlobalPosition,
                CollideWithAreas    = true,
                CollideWithBodies   = true,
                CollisionMask       = obstructionLayer,
            };

            Godot.Collections.Dictionary result = spaceState.IntersectRay(parameters);
        
            if(result.Count>0){
                if(isInSight.Contains(other)==false){
                    isInSight.Remove(other);
                    notInSight.Add(other);
                    OnLeftSight?.Invoke(other);
                }
                continue;
            }
            else{
                if(isInSight.Contains(other)==false){
                    notInSight.Remove(other);
                    isInSight.Add(other);
                    OnInSight?.Invoke(other);
                }
            }
        }
    }

    private void HandlePassiveZoneEntered(Node2D node){
        collisions.Add(node);
        OnEnteredZone?.Invoke(node);
    }

    private void HandleActiveZoneExited(Node2D node){
        collisions.Remove(node);
        OnExitedZone?.Invoke(node);
    }


    /// 
    /// Linkage.
    /// 


    public void LinkEvents(){
        passiveZone.BodyEntered += HandlePassiveZoneEntered;
        passiveZone.AreaEntered += HandlePassiveZoneEntered;
        activeZone.BodyExited   += HandleActiveZoneExited;
        activeZone.AreaExited   += HandleActiveZoneExited;
    }

    public void UnlinkEvents(){
        passiveZone.BodyEntered -= HandlePassiveZoneEntered;
        passiveZone.AreaEntered -= HandlePassiveZoneEntered;
        activeZone.BodyExited   -= HandleActiveZoneExited;
        activeZone.AreaExited   -= HandleActiveZoneExited;
    }
}
