using Godot;
using System;

public partial class SegmentedDoorPiece : Node3D{


    /// 
    /// variables.
    /// 


    [Export] AnimationPlayer animator;
    [Export] Timer delayTimer;
    private DelayIntention delayIntention;
    public event Action OnFinishedOpening;
    const string OpenAnimation = "Open";
    const string CloseAnimation = "Close";


    /// 
    /// Definitions.
    /// 


    enum DelayIntention : byte{
        Open,
        Close,
    }


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
    /// functions.
    /// 


    public void Open(float time){
        delayIntention = DelayIntention.Open;
        delayTimer.Start(time);
    }

    public void Open(){
        animator.Play(OpenAnimation);
    }

    public void Close(float time){
        delayIntention = DelayIntention.Close;
        delayTimer.Start(time);
    }

    public void Close(){
        animator.Play(CloseAnimation);
    }

    private void OnDelayTimeoutCallback(){
        switch(delayIntention){
            case DelayIntention.Open:
                Open();
            break;
            case DelayIntention.Close:
                Close();
            break;
        }
    }

    private void OnAnimationFinished(StringName animation){
        if(animation == OpenAnimation){
            OnFinishedOpening?.Invoke();
            GD.Print($"animation finished {animation}");
        }
    }

    private void OnPauseCallback(){
        delayTimer.Paused = true;
        animator.SpeedScale = 0;
    }

    private void OnResumeCallback(){
        delayTimer.Paused = false;
        animator.SpeedScale = 1;
    }

    /// 
    /// Linkage.
    /// 

    private void LinkEvents(){
        delayTimer.Timeout += OnDelayTimeoutCallback;
        animator.AnimationFinished += OnAnimationFinished; 
        EntityManager.Singleton.OnPause += OnPauseCallback;
        EntityManager.Singleton.OnResume += OnResumeCallback;
    }

    private void UnlinkEvents(){
        delayTimer.Timeout -= OnDelayTimeoutCallback;
        animator.AnimationFinished -= OnAnimationFinished; 
        EntityManager.Singleton.OnPause -= OnPauseCallback;
        EntityManager.Singleton.OnResume -= OnResumeCallback;
    }
}
