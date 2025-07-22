using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class HitBoxHandler : Node2D{
    [Export] private CollisionShape2D[] hitBoxes;
    [Export] private Timer[] timers;
    private HashSet<ulong>[] hits;
    private Action[] timeouts;
    private Area2D.BodyEnteredEventHandler[] bodyEnters;
    private Area2D.AreaEnteredEventHandler[] areaEnters;
    public Action<Node2D, int> OnHit;

    public override void _EnterTree(){
        base._EnterTree();
        LinkEvents();
    }

    public override void _ExitTree(){
        base._ExitTree();
        UnlinkEvents();
    }

    public void EnableHitBox(int id, float time){
        // enable the collider.

        hitBoxes[id].Disabled = false;
        hits[id].Clear();

        // start the timer for it to disable.
        Timer timer = timers[id];
        timer.WaitTime = time;
        timer.Start();        
    }

    public override void _PhysicsProcess(double delta){
        base._PhysicsProcess(delta);
        for(int i = 0; i < hitBoxes.Length; i++){
            CollisionShape2D collider = hitBoxes[i];
            if(collider.Disabled == false){
                if (collider.Shape is RectangleShape2D rectShape){
                    GodotObject debugDraw = GetNode<GodotObject>("/root/DebugDraw2D");
                    debugDraw.Call("rect",collider.GlobalPosition, rectShape.Size, new Color(1, 1f, 1), 1f, 0.0167f);
                }
            }
        }

    }

    public void DisableHitBox(int id){
        hitBoxes[id].Disabled = true;
    }

    public void DisableAllHitBoxes(){
        for(int i = 0; i < hitBoxes.Length; i++){
            hitBoxes[i].CallDeferred("set_disabled",true);
        }
        for(int i = 0; i < timers.Length; i++){
            timers[i].Stop();
        }
    }

    private void LinkEvents(){
        timeouts = new Action[timers.Length];
        for(int i = 0; i < timers.Length; i++){
            int id = i;
            timeouts[i] = () => DisableHitBox(id);
            timers[i].Timeout += timeouts[i];
        }

        hits = new HashSet<ulong>[hitBoxes.Length];
        bodyEnters = new Area2D.BodyEnteredEventHandler[hitBoxes.Length];
        areaEnters = new Area2D.AreaEnteredEventHandler[hitBoxes.Length];
        for(int i = 0; i < hitBoxes.Length; i++){
            
            int id = i;
            
            hits[i] = new HashSet<ulong>();

            bodyEnters[i] = (Node2D node) => {
                if(hits[id].Contains(node.GetInstanceId())==false){
                    hits[id].Add(node.GetInstanceId());
                    OnHit?.Invoke(node, id);
                }
            };

            areaEnters[i] = (Area2D node) => {
                if(hits[id].Contains(node.GetInstanceId())==false){
                    hits[id].Add(node.GetInstanceId());
                    OnHit?.Invoke(node, id);
                }
            };

            Area2D colliderArea = (Area2D)hitBoxes[i].GetParent();
            colliderArea.BodyEntered += bodyEnters[i];
            colliderArea.AreaEntered += areaEnters[i];
        }

    }

    private void UnlinkEvents(){
        for(int i = 0; i < timers.Length; i++){
            timers[i].Timeout -= timeouts[i];
        }
        for(int i = 0; i < hitBoxes.Length; i++){
            Area2D colliderArea = (Area2D)hitBoxes[i].GetParent();
            colliderArea.BodyEntered -= bodyEnters[i];
            colliderArea.AreaEntered -= areaEnters[i];
        }
        timeouts    = null;
        bodyEnters  = null;
        areaEnters  = null;
        hits        = null;
        OnHit       = null;
    }
}
