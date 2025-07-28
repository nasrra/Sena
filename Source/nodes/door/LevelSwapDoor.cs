using Godot;
using System;

public partial class LevelSwapDoor : Door{

    [ExportGroup("LevelSwapDoor")]
    [Export] private Area2D enterZone;
    [Export] public Node2D ExitPoint {get; private set;}
    [Export] private string levelToLoad;
    [Export] private int doorToLoadTo; // <-- id in the door manager array. 


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
    /// Functions.
    /// 


    public void Enter(){
        LevelSwapDoorManager.Instance.SetExitDoorId(doorToLoadTo);
        SceneManager.Instance.LoadScene2D(levelToLoad, SceneLoadType.Delete, 0.5f);
        EntityManager.Singleton.PauseEntityProcesses();
    }

    public override void Open(){
        CallDeferred(nameof(EnableEnterZone));
        base.Open();
    }

    public override void Close(){
        CallDeferred(nameof(DisableEnterZone));
        base.Close();
    }

    private void EnableEnterZone(){
        enterZone.Monitorable = true;
        enterZone.Monitoring = true;
        enterZone.GetNode<CollisionShape2D>("CollisionShape2D").Disabled = false;
    }

    private void DisableEnterZone(){
        enterZone.Monitorable = false;
        enterZone.Monitoring = false;
        enterZone.GetNode<CollisionShape2D>("CollisionShape2D").Disabled = true;
    }


    /// 
    /// Linkage.
    /// 


    private void LinkEvents(){
        enterZone.BodyEntered += OnEnterZone;
    }

    private void UnlinkEvents(){
        enterZone.BodyEntered -= OnEnterZone;        
    }


    ///
    /// Linkage Functions.
    /// 


    private void OnEnterZone(Node2D other){
        string hitLayer = PhysicsManager.Singleton.GetPhysics2DLayerName((other as CollisionObject2D).CollisionLayer);
        switch(hitLayer){
            case "Player":
                Enter();
                break;
            default:
                throw new Exception($"{hitLayer} not implemented.");
        }
    }
}
