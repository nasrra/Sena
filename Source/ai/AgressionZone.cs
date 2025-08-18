using Entropek.Collections;
using Godot;
using System;
using System.Collections.Generic;

public partial class AgressionZone : Node3D{
    [Export] private Area3D passiveZone;
    [Export] private Area3D activeZone;
    [Export(PropertyHint.Layers3DPhysics)] private uint obstructionLayer;

    private HashSet<Node3D> collisions = new HashSet<Node3D>();
    private HashSet<Node3D> isInSight  = new HashSet<Node3D>();
    private HashSet<Node3D> notInSight = new HashSet<Node3D>();
    public event Action<Node3D> OnInSight;
    public event Action<Node3D> OnLeftSight;
    public event Action<Node3D> OnEnteredZone;
    public event Action<Node3D> OnExitedZone;
    public event Action<double> PhysicsProcess;
    private bool paused = false;


    ///
    /// Base.
    /// 


    public override void _EnterTree(){
        base._EnterTree();
        LinkEvents();
        ResumeState();
    }

    public override void _ExitTree(){
        base._ExitTree();
        UnlinkEvents();
    }

    private void InvokePhysicsProcess(double delta){
        PhysicsProcess?.Invoke(delta);
    }


    /// 
    /// Fuinctions.
    /// 


    private void Evaluate(double delta){

        PhysicsDirectSpaceState3D spaceState = GetWorld3D().DirectSpaceState;

        foreach(Node3D node in collisions){
            if(IsInstanceValid(node)==false){
                continue;
            }

            Godot.Collections.Dictionary result = spaceState.IntersectRay(
                new PhysicsRayQueryParameters3D{
                From                = GlobalPosition,
                To                  = node.GlobalPosition,
                CollideWithAreas    = true,
                CollideWithBodies   = true,
                CollisionMask       = obstructionLayer,
            });
        
            if(result.Count>0){
                if(notInSight.Contains(node)==false){
                    isInSight.Remove(node);
                    notInSight.Add(node);
                    OnLeftSight?.Invoke(node);
                }
                continue;
            }
            else{
                if(isInSight.Contains(node)==false){
                    notInSight.Remove(node);
                    isInSight.Add(node);
                    OnInSight?.Invoke(node);
                }
            }
        }
    }

    private void HandlePassiveZoneEntered(Node3D node){
        
        collisions.Add(node);
        if(paused==false){
            OnEnteredZone?.Invoke(node);
        }
    }

    private void HandleActiveZoneExited(Node3D node){
        
        collisions.Remove(node);
        
        if(paused==false){
            OnExitedZone?.Invoke(node);
        }
    }

    public bool IsInSight(Node3D node){
        return isInSight.Contains(node);
    }

    public void PauseState(){
        PhysicsProcess = null;
        paused = true;
    }

    public void ResumeState(){
        PhysicsProcess = Evaluate;
        paused = false;
    }


    /// 
    /// Linkage.
    /// 


    public void LinkEvents(){

        EntityManager.Singleton.OnPhysicsProcess += InvokePhysicsProcess;

        passiveZone.BodyEntered += HandlePassiveZoneEntered;
        passiveZone.AreaEntered += HandlePassiveZoneEntered;
        // activeZone.BodyExited   += HandleActiveZoneExited;
        // activeZone.AreaExited   += HandleActiveZoneExited;
    }

    public void UnlinkEvents(){

        EntityManager.Singleton.OnPhysicsProcess -= InvokePhysicsProcess;

        passiveZone.BodyEntered -= HandlePassiveZoneEntered;
        passiveZone.AreaEntered -= HandlePassiveZoneEntered;
        // activeZone.BodyExited   -= HandleActiveZoneExited;
        // activeZone.AreaExited   -= HandleActiveZoneExited;
    }
}
