using Godot;
using System;

public partial class EntityManager : Node{
    public static EntityManager Instance {get;private set;}
    [Export] private Timer pausedTimer;
    private event Action<double> OnProcess;
    private event Action<double> OnPhysicsProcess;
    private event Action OnPause;
    private event Action OnResume;
    private bool paused = false;


    /// 
    /// Base.
    /// 


    public override void _EnterTree(){
        base._EnterTree();
        Instance = this;
        LinkEvents();
    }

    public override void _ExitTree(){
        base._ExitTree();
        OnProcess           = null;
        OnPhysicsProcess    = null;
        OnPause             = null;
        OnResume            = null;
        UnlinkEvents();    
    }

    public override void _Process(double delta){
        base._Process(delta);
        if(paused==false){
            OnProcess?.Invoke(delta);
        }

        if(Input.IsActionJustPressed("Debug1")){
            PauseEntityProcesses();
        }

        if(Input.IsActionJustPressed("Debug2")){
            ResumeEntityProcesses();
        }
    }

    public override void _PhysicsProcess(double delta){
        base._PhysicsProcess(delta);
        if(paused==false){
            OnPhysicsProcess?.Invoke(delta);
        }
    }


    ///
    /// Linkage Functions. 
    /// 


    public void LinkToProcess(Action<double> callback){
        OnProcess += callback;
    } 

    public void UnlinkFromProcess(Action<double> callback){
        OnProcess -= callback;
    }

    public void LinkToPhysicsProcess(Action<double> callback){
        OnPhysicsProcess += callback;
    }

    public void UnlinkFromPhysicsProcess(Action<double> callback){
        OnPhysicsProcess -= callback;
    }

    public void LinkToPause(Action callback){
        OnPause += callback;
    }

    public void UnlinkFromPause(Action callback){
        OnPause -= callback;
    }

    public void LinkToResume(Action callback){
        OnResume += callback;
    }

    public void UnlinkFromResume(Action callback){
        OnResume -= callback;
    }

    public void PauseEntityProcesses(){
        paused = true;
        OnPause?.Invoke();
    }

    public void PauseEntityProcesses(float time){
        pausedTimer.WaitTime = time;
        pausedTimer.Start();
        PauseEntityProcesses();
    }

    public void ResumeEntityProcesses(){
        paused = false;
        OnResume?.Invoke();
    }


    ///
    /// Linkage.
    /// 


    public void LinkEvents(){
        pausedTimer.Timeout += ResumeEntityProcesses;
    }

    public void UnlinkEvents(){
        pausedTimer.Timeout -= ResumeEntityProcesses;
    }
}
