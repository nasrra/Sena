using Godot;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;

public partial class HitBox : Area2D{
    private HashSet<ulong> hits = new HashSet<ulong>();
    [Export] Timer enabledTimer;
    [Export] CollisionShape2D collsionShape;
    public event Action<Node2D, int> OnHit;
    private int id;

    /// 
    /// Base.
    /// 

    public override void _Ready(){
        base._Ready();
        LinkEvents();
    }

    public override void _ExitTree(){
        base._ExitTree();
        UnlinkEvents();
    }

    public override void _PhysicsProcess(double delta){
        base._PhysicsProcess(delta);
        if (collsionShape.Disabled == false && collsionShape.Shape is RectangleShape2D rectShape){
            GodotObject debugDraw = GetNode<GodotObject>("/root/DebugDraw2D");
            debugDraw.Call("rect",collsionShape.GlobalPosition, rectShape.Size* Scale, new Color(1, 1f, 1), 1f, 0.0167f);
        }
    }


    /// 
    /// Functions.
    /// 

    public void SetId(int id){
        this.id = id;
    }

    public void Enable(){
        collsionShape.Disabled = false;
        hits.Clear();
    }

    public void Enable(float time){
        Enable();
        enabledTimer.Start(time);
    }

    public void Disable(){
        // collsionShape.Disabled = true;
        collsionShape.CallDeferred("set_disabled", true);
    }

    public void PauseState(){
        enabledTimer.Paused = true;
    }

    public void ResumeState(){
        enabledTimer.Paused = false;
    }

    public void HaltState(){
        Disable();
        enabledTimer.Stop();
    }

    private void CollisionCallback(Node2D node){
        ulong instanceId = node.GetInstanceId();
        if(hits.Contains(instanceId)){
            return;
        }
        hits.Add(instanceId);
        OnHit?.Invoke(node, id);
    }


    /// 
    /// Linkage. 
    /// 


    private void LinkEvents(){
        enabledTimer.Timeout    += Disable;
        BodyEntered             += CollisionCallback;
        AreaEntered             += CollisionCallback;
    }

    private void UnlinkEvents(){
        enabledTimer.Timeout    -= Disable; 
        BodyEntered             -= CollisionCallback;
        AreaEntered             -= CollisionCallback;
    }
}
