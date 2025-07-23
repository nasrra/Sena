using Godot;
using System;

public partial class SceneManager : Node{

    public static SceneManager Instance{get; private set;}

    [Export] private Node2D world2D;
    [Export] private CanvasLayer gui;

    [Export] public Node2D current2DScene {get; private set;}
    [Export] public Control currentGuiScene {get; private set;}

    [Export] public PackedScene scene2DStart;
    [Export] public PackedScene guiStart;
    private PackedScene current2DPackedScene;
    private PackedScene currentGuiPackedScene;

    public override void _Ready(){
        base._Ready();
        if(guiStart != null){
            LoadGUI(guiStart, SceneLoadType.DELETE);
        }
        if(scene2DStart != null){
            LoadScene2D(scene2DStart, SceneLoadType.DELETE);
        }
    }


    public override void _EnterTree(){
        base._EnterTree();
        Instance = this;
    }

    public override void _ExitTree(){
        base._ExitTree();
        Instance = null;
    }

    public void LoadGUI(PackedScene scene, SceneLoadType loadType){
        
        if(currentGuiScene != null){
            switch(loadType){
                case SceneLoadType.DELETE:
                    currentGuiScene.QueueFree();
                break;
                case SceneLoadType.HIDE:
                    currentGuiScene.Visible = false;
                break;
                case SceneLoadType.REMOVE:
                    gui.RemoveChild(currentGuiScene);
                break;
            }
        }

        currentGuiPackedScene = scene;
        Control newGui = (Control)currentGuiPackedScene.Instantiate();
        gui.AddChild(newGui);
        currentGuiScene = newGui;
    }

    public void LoadScene2D(PackedScene scene, SceneLoadType loadType){
        if(current2DScene != null){
            switch(loadType){
                case SceneLoadType.DELETE:
                    current2DScene.QueueFree();
                break;
                case SceneLoadType.HIDE:
                    current2DScene.Visible = false;
                break;
                case SceneLoadType.REMOVE:
                    world2D.RemoveChild(current2DScene);
                break;
            }
        }
        
        current2DPackedScene = scene;
        Node2D newWorld = (Node2D)current2DPackedScene.Instantiate();
        world2D.AddChild(newWorld);
        current2DScene = newWorld;
    }

    public void ReloadScene2D(){
        LoadScene2D(current2DPackedScene, SceneLoadType.DELETE);
    }
}

public enum SceneLoadType{
    DELETE, // No memory or data.
    HIDE, // in memory and runs.
    REMOVE // in memory but no longer updates.
}
