using Godot;
using System;

public partial class EntityManager : Node{
    public static EntityManager Singleton {get;private set;}
    [Export] private Timer pausedTimer;
    public event Action<double> OnProcess;
    public event Action<double> OnPhysicsProcess;
    public event Action OnPause;
    public event Action OnResume;
    private bool paused = false;


    /// 
    /// Base.
    /// 


    public override void _EnterTree(){
        base._EnterTree();
        Singleton = this;
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


    private void LinkEvents(){
        pausedTimer.Timeout += ResumeEntityProcesses;
    }

    private void UnlinkEvents(){
        pausedTimer.Timeout -= ResumeEntityProcesses;
    }
}
