using Godot;
using System;

public partial class SceneManager : Node{

    private const string levelsResourcePath = "res://scenes/levels/";
    private const string guiResourcePath = "res://scenes/gui/";
    public static SceneManager Instance{get; private set;}

    private event Action loadGuiDelayed;
    private event Action loadScene2DDelayed;

    [Export] private Node2D world2D;
    [Export] private CanvasLayer gui;

    [Export] private Timer loadScene2DDelayTimer;
    [Export] private Timer loadGuiDelayTimer;
    [Export] public Node2D current2DScene {get; private set;}
    [Export] public Control currentGuiScene {get; private set;}

    [Export] public string scene2DStart;
    [Export] public string guiStart;

    public override void _Ready(){
        base._Ready();
        if(guiStart != null){
            LoadGui(guiStart, SceneLoadType.DELETE);
        }
        if(scene2DStart != null){
            LoadScene2D(scene2DStart, SceneLoadType.DELETE);
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
        
        if(currentGuiScene != null){
            switch(loadType){
                case SceneLoadType.DELETE:
                    // shift hard reference so it goes out of scope for GC.
                    Node sceneToDelete  = currentGuiScene;
                    currentGuiScene     = null;
                    sceneToDelete.QueueFree();
                    await ToSignal(sceneToDelete, "tree_exited");
                break;
                case SceneLoadType.HIDE:
                    currentGuiScene.Visible = false;
                break;
                case SceneLoadType.REMOVE:
                    gui.RemoveChild(currentGuiScene);
                break;
            }
        }

        PackedScene packedScene = GD.Load<PackedScene>(guiResourcePath+sceneName+".tscn");
        Control newGui = (Control)packedScene.Instantiate();
        gui.AddChild(newGui);
        currentGuiScene = newGui;
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
    }

    public async void LoadScene2D(string sceneName, SceneLoadType loadType){
        if(current2DScene != null){
            switch(loadType){
                case SceneLoadType.DELETE:
                    // shift hard reference so it goes out of scope for GC.
                    Node sceneToDelete  = current2DScene;
                    current2DScene      = null;
                    sceneToDelete.QueueFree();
                    await ToSignal(sceneToDelete, "tree_exited");
                break;
                case SceneLoadType.HIDE:
                    current2DScene.Visible = false;
                break;
                case SceneLoadType.REMOVE:
                    world2D.RemoveChild(current2DScene);
                break;
            }
        }
        
        PackedScene packedScene = GD.Load<PackedScene>(levelsResourcePath+sceneName+".tscn");
        Node2D newWorld = (Node2D)packedScene.Instantiate();
        world2D.CallDeferred("add_child", newWorld);
        current2DScene = newWorld;
    }

    public void ReloadScene2D(){
        LoadScene2D(current2DScene.Name, SceneLoadType.DELETE);
    }
}


/// 
/// Definitions.
/// 


public enum SceneLoadType{
    DELETE, // No memory or data.
    HIDE, // in memory and runs.
    REMOVE // in memory but no longer updates.
}
