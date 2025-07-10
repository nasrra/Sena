using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

public partial class FunctionTracker : Node{
    public static FunctionTracker Instance {get; private set;}

    private Dictionary<string, Stopwatch> timers = new Dictionary<string, Stopwatch>();
    private Dictionary<string, double> currentFrameTimes = new Dictionary<string, double>();
    private Dictionary<string, double> maxFrameTimes = new Dictionary<string, double>();

    [Export]
    private VBoxContainer labelsContainer;

    [Export]
    private Timer refreshInterval;

    public override void _Ready(){
        base._Ready();
        Instance = this;
        LinkEvents();
        refreshInterval.Start();
    }

    public override void _EnterTree(){
        base._EnterTree();
    }

    public override void _ExitTree(){
        base._ExitTree();
        refreshInterval.Stop();
    }

    public void Start([CallerFilePath] string filePath = "", [CallerMemberName] string methodName = ""){
        string className = System.IO.Path.GetFileNameWithoutExtension(filePath);
        string full = $"{className}/{methodName}";
        if(timers.ContainsKey(full)==false){
            timers.Add(full, new Stopwatch());
        }
        timers[full].Start();
    }

    public void Stop(bool cummulative, [CallerFilePath] string filePath = "", [CallerMemberName] string methodName = ""){
        string className = System.IO.Path.GetFileNameWithoutExtension(filePath);
        string full = $"{className}/{methodName}";

        if(timers.ContainsKey(full)==false){
            return;
        }        
        
        timers[full].Stop();
        
        if(currentFrameTimes.ContainsKey(full)==false){
            currentFrameTimes.Add(full, 0f);
        }

        if(cummulative == false){
            currentFrameTimes[full] = Mathf.Max(currentFrameTimes[full], timers[full].Elapsed.TotalMilliseconds);
        }
        else{
            currentFrameTimes[full] += timers[full].Elapsed.TotalMilliseconds;
        }


        timers[full].Reset();
    }

    public override void _Process(double delta){
        base._Process(delta);
        foreach(string functionName in currentFrameTimes.Keys){
            if(maxFrameTimes.ContainsKey(functionName)){
                maxFrameTimes[functionName] = Mathf.Max(currentFrameTimes[functionName], maxFrameTimes[functionName]);
            }
            else{
                maxFrameTimes.Add(functionName, currentFrameTimes[functionName]);
            }
        }

        currentFrameTimes.Clear();
    }


    public void Refresh(){
        // display the data.

        Godot.Collections.Array<Node> labels = labelsContainer.GetChildren();
        for(int i = 0; i < labels.Count; i++){
            labels[i].QueueFree();
        }

        foreach (string functionName in timers.Keys){
            Label label = new Label();
            label.Text = $"{maxFrameTimes[functionName]} : {functionName}";
            labelsContainer.AddChild(label);
        }

        // remove data for next refresh.

        timers.Clear();
        maxFrameTimes.Clear();

        // start the refresh timer.

        refreshInterval.Start();
    }

    public void Enable(){
        GetTree().Root.AddChild(this);
    }

    public void Disable(){
        GetParent().RemoveChild(this);
    }

    private void LinkEvents(){
        refreshInterval.Timeout += Refresh;
    }

    private void UnlinkEvents(){
        refreshInterval.Timeout -= Refresh;
    }
}
