using Godot;
using System;

public partial class HitBoxHandler : Node2D{
    [Export] CollisionShape2D[] hitBoxes;
    [Export] Timer[] timers;
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
                    debugDraw.Call("rect",collider.GlobalPosition, rectShape.Size, new Color(1, 1, 1), 0.25f, 0.0167f);
                }
            }
        }

    }

    public void DisableHitBox(int id){
        hitBoxes[id].Disabled = true;
    }

    private void LinkEvents(){
        timeouts = new Action[timers.Length];
        for(int i = 0; i < timers.Length; i++){
            int id = i;
            timeouts[i] = () => DisableHitBox(id);
            timers[i].Timeout += timeouts[i];
        }

        bodyEnters = new Area2D.BodyEnteredEventHandler[hitBoxes.Length];
        areaEnters = new Area2D.AreaEnteredEventHandler[hitBoxes.Length];
        for(int i = 0; i < hitBoxes.Length; i++){
            int id = i;
            bodyEnters[i] = (Node2D node) => OnHit?.Invoke(node, id);
            areaEnters[i] = (Area2D node) => OnHit?.Invoke(node, id);
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
        OnHit       = null;
    }
}
