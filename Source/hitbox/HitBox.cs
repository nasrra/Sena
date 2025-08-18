using Godot;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;

public partial class HitBox : Area3D{
    private HashSet<ulong> hits = new HashSet<ulong>();
    [Export] Timer enabledTimer;
    [Export] CollisionShape3D collsionShape;
    public event Action<Node3D, int> OnHit;
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
        #if TOOLS
        if(collsionShape.Disabled==false){
            if (collsionShape.Shape is BoxShape3D boxShape){
                DebugDraw3D.DrawBox(GlobalPosition, Quaternion.Identity, boxShape.Size * Scale, new Color(1,1,1,1), true, 0.0167f);
            }
        }
        #endif
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

    private void CollisionCallback(Node3D node){
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
