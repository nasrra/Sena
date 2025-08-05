using Godot;
using Godot.Collections;
using System;

public partial class HitBoxHandler : Node2D{

    [Export] private Array<HitBox> hitBoxes;
    public Action<Node2D, int> OnHit;


    ///
    /// Base.
    ///


    public override void _EnterTree(){
        base._EnterTree();
        LinkEvents();
    }

    public override void _Ready(){
        base._Ready();
        for(int i = 0; i < hitBoxes.Count; i++){
            hitBoxes[i].SetId(i);
        }
    }


    public override void _ExitTree(){
        base._ExitTree();
        UnlinkEvents();
    }


    /// 
    /// Functions.
    /// 


    public void EnableHitBox(int id, float time){
        // enable the collider.
        hitBoxes[id].Enable(time);
    }

    public void DisableHitBox(int id){
        hitBoxes[id].Disable();
    }

    public void DisableAllHitBoxes(){
        for(int i = 0; i < hitBoxes.Count; i++){
            hitBoxes[i].HaltState();
        }
    }

    private void OnHitCallback(Node2D node, int hitBoxId){
        OnHit?.Invoke(node, hitBoxId);
    }


    /// 
    /// State Machine.
    /// 


    public void PauseState(){
        for(int i = 0; i < hitBoxes.Count; i++){
            hitBoxes[i].PauseState();
        }
    }

    public void ResumeState(){
        for(int i = 0; i < hitBoxes.Count; i++){
            hitBoxes[i].ResumeState();
        }
    }


    /// 
    /// Linkage.
    /// 


    private void LinkEvents(){
        for(int i = 0; i < hitBoxes.Count; i++){
            hitBoxes[i].OnHit += OnHitCallback;
        }
    }

    private void UnlinkEvents(){
        for(int i = 0; i < hitBoxes.Count; i++){
            hitBoxes[i].OnHit -= OnHitCallback;
        }
    }
}
