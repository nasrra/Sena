using Entropek.Collections;
using Godot;
using System;
using System.Collections.Generic;

public partial class AgressionZone : Node2D{
    [Export] private Area2D passiveZone;
    [Export] private Area2D activeZone;
    [Export(PropertyHint.Layers2DPhysics)] private uint obstructionLayer;

    private HashSet<Node2D> collisions = new HashSet<Node2D>();
    private HashSet<Node2D> isInSight  = new HashSet<Node2D>();
    private HashSet<Node2D> notInSight = new HashSet<Node2D>();
    public event Action<Node2D> OnInSight;
    public event Action<Node2D> OnLeftSight;
    public event Action<Node2D> OnEnteredZone;
    public event Action<Node2D> OnExitedZone;
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

        PhysicsDirectSpaceState2D spaceState = GetWorld2D().DirectSpaceState;

        foreach(Node2D node in collisions){
            PhysicsRayQueryParameters2D parameters = new PhysicsRayQueryParameters2D{
                From                = GlobalPosition,
                To                  = node.GlobalPosition,
                CollideWithAreas    = true,
                CollideWithBodies   = true,
                CollisionMask       = obstructionLayer,
            };

            Godot.Collections.Dictionary result = spaceState.IntersectRay(parameters);
        
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

    private void HandlePassiveZoneEntered(Node2D node){
        
        collisions.Add(node);

        if(paused==false){
            OnEnteredZone?.Invoke(node);
        }
    }

    private void HandleActiveZoneExited(Node2D node){
        
        collisions.Remove(node);
        
        if(paused==false){
            OnExitedZone?.Invoke(node);
        }
    }

    public bool IsInSight(Node2D node){
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
