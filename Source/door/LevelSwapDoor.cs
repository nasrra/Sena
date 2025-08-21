using Godot;
using System;

public partial class LevelSwapDoor : Door{

    [ExportGroup("LevelSwapDoor")]
    [Export] private Area3D enterZone;
    [Export] public Node3D ExitPoint {get; private set;}
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
        SceneManager.Instance.LoadScene3D(levelToLoad, SceneLoadType.Delete, 0.5f);
        EntityManager.Singleton.PauseEntityProcesses();
    }

    public override void Open(){
        EnableEnterZone();
        Opened();
    }

    public override void Close(){
        DisableEnterZone();
        Closed();
    }

    public override void Unlock(){
        Unlocked();
    }

    public override void Lock(){
        Locked();
    }

    private void EnableEnterZone(){
        CollisionShape3D shape = enterZone.GetNode<CollisionShape3D>("CollisionShape3D");
        shape.CallDeferred("set_disabled", false);
    }

    private void DisableEnterZone(){
        CollisionShape3D shape = enterZone.GetNode<CollisionShape3D>("CollisionShape3D");
        shape.CallDeferred("set_disabled", true);
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


    private void OnEnterZone(Node3D other){
        string hitLayer = PhysicsManager.Singleton.GetPhysics3DLayerName((other as CollisionObject3D).CollisionLayer);
        switch(hitLayer){
            case "Player":
                Enter();
                break;
            default:
                throw new Exception($"{hitLayer} not implemented.");
        }
    }
}
