using Godot;
using System;

public partial class SceneManager : Node{

    private const string levelsResourcePath = "res://scenes/levels/";
    private const string guiResourcePath = "res://scenes/gui/";
    public static SceneManager Instance{get; private set;}

    private event Action loadGuiDelayed;
    private event Action loadScene2DDelayed;
    public event Action OnScene2DLoaded;
    public event Action OnScene2DDelayedLoadSet;

    [Export] private Node2D world2D;
    [Export] private CanvasLayer gui;

    [Export] private Timer loadScene2DDelayTimer;
    [Export] private Timer loadGuiDelayTimer;
    [Export] public Node2D Current2DScene {get; private set;}
    [Export] public Control CurrentGuiScene {get; private set;}

    [Export] public string scene2DStart;
    [Export] public string guiStart;

    public override void _Ready(){
        base._Ready();
        if(guiStart != null){
            LoadGui(guiStart, SceneLoadType.Delete);
        }
        if(scene2DStart != null){
            LoadScene2D(scene2DStart, SceneLoadType.Delete);
        }
    }


    /// 
    /// Base.
    /// 


    public override void _EnterTree(){
        base._EnterTree();
        Instance = this;
    }

    public override void _ExitTree(){
        base._ExitTree();
        Instance = null;
    }


    /// 
    /// Functions.
    /// 


    public void LoadGui(string sceneName, SceneLoadType loadType, float delayTime){
        loadGuiDelayed = () => {
            LoadGui(sceneName, loadType);
            loadGuiDelayTimer.Timeout -= loadGuiDelayed;
            loadGuiDelayed = null;
        };

        loadGuiDelayTimer.Timeout += loadGuiDelayed;
        loadGuiDelayTimer.WaitTime = delayTime;
        loadGuiDelayTimer.Start();
    }

    public async void LoadGui(string sceneName, SceneLoadType loadType){
        
        if(CurrentGuiScene != null){
            switch(loadType){
                case SceneLoadType.Delete:
                    // shift hard reference so it goes out of scope for GC.
                    Node sceneToDelete  = CurrentGuiScene;
                    CurrentGuiScene     = null;
                    sceneToDelete.QueueFree();
                    await ToSignal(sceneToDelete, "tree_exited");
                break;
                case SceneLoadType.Hide:
                    CurrentGuiScene.Visible = false;
                break;
                case SceneLoadType.Remove:
                    gui.RemoveChild(CurrentGuiScene);
                break;
            }
        }

        PackedScene packedScene = GD.Load<PackedScene>(guiResourcePath+sceneName+".tscn");
        Control newGui = (Control)packedScene.Instantiate();
        gui.AddChild(newGui);
        CurrentGuiScene = newGui;
    }

    public void LoadScene2D(string sceneName, SceneLoadType loadType, float delayTime){
        loadScene2DDelayed = () => {
            LoadScene2D(sceneName, loadType);
            loadScene2DDelayTimer.Timeout -= loadScene2DDelayed;
            loadScene2DDelayed = null;
        };
        
        loadScene2DDelayTimer.Timeout += loadScene2DDelayed;
        loadScene2DDelayTimer.WaitTime = delayTime;
        loadScene2DDelayTimer.Start();
        OnScene2DDelayedLoadSet?.Invoke();
    }

    public async void LoadScene2D(string sceneName, SceneLoadType loadType){
        if(Current2DScene != null){
            switch(loadType){
                case SceneLoadType.Delete:
                    // shift hard reference so it goes out of scope for GC.
                    Node sceneToDelete  = Current2DScene;
                    Current2DScene      = null;
                    sceneToDelete.QueueFree();
                    await ToSignal(sceneToDelete, "tree_exited");
                break;
                case SceneLoadType.Hide:
                    Current2DScene.Visible = false;
                break;
                case SceneLoadType.Remove:
                    world2D.RemoveChild(Current2DScene);
                break;
            }
        }
        
        PackedScene packedScene = GD.Load<PackedScene>(levelsResourcePath+sceneName+".tscn");
        Node2D newWorld = (Node2D)packedScene.Instantiate();
        // world2D.CallDeferred("add_child", newWorld);
        world2D.AddChild(newWorld);
        Current2DScene = newWorld;

        CallDeferred(nameof(InvokeScene2DLoaded));
    }

    public void ReloadScene2D(){
        LoadScene2D(Current2DScene.Name, SceneLoadType.Delete);
    }
    
    private void InvokeScene2DLoaded() {
        OnScene2DLoaded?.Invoke();
    }
}


/// 
/// Definitions.
/// 


public enum SceneLoadType{
    Delete, // No memory or data.
    Hide, // in memory and runs.
    Remove // in memory but no longer updates.
}
