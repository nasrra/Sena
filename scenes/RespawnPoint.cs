using Godot;
using System;

public partial class RespawnPoint : Node2D{
    public static string RespawnScene {get;private set;} = "Level0";
    public static RespawnPoint Instance {get;private set;} = null;

    [Export] private Interactable interactable;


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
        Instance = null;
        UnlinkEvents();
    }


    /// 
    /// Functions.
    /// 


    public void Rest(){
        RespawnScene = SceneManager.Instance.Current2DSceneName;
        GD.Print("respawn point set: "+RespawnScene);
        Player.Instance.Health.HealToMax();
        EntityManager.Instance.PauseEntityProcesses(0.33f);
    }

    public static void Respawn(){
        GD.Print("Respawn At: "+RespawnScene);
        SceneManager.Instance.LoadScene2D(RespawnScene, SceneLoadType.Delete, 0.5f);
    }


    ///
    /// Linkage.
    /// 


    private void LinkEvents(){
        interactable.OnInteract += OnInteract;
    }

    private void UnlinkEvents(){
        interactable.OnInteract -= OnInteract;
    }

    private void OnInteract(Interactor interactor){
        Rest();
    }

}
