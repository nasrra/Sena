using Godot;
using System;

public partial class RespawnPoint : Node2D{
    public static string RespawnScene {get;private set;} = "";
    public static RespawnPoint Instance {get;private set;} = null;

    [Export] private Interactable interactable;


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


    public void Rest(){
        RespawnScene = SceneManager.Instance.Current2DScene.Name;
        Player.Instance.Health.HealToMax();
        EntityManager.Instance.PauseEntityProcesses(0.33f);
    }

    public static void Respawn(){
        SceneManager.Instance.LoadScene2D(RespawnScene, SceneLoadType.Delete, 0.5f);
        CameraController.Instance.FadeToBlack(0.33f);
    }

    ///
    /// Linkage.
    /// 

    private void LinkEvents(){
        interactable.OnInteract += Rest;
    }

    private void UnlinkEvents(){
        interactable.OnInteract -= Rest;
    }
}
