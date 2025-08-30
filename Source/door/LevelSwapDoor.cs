using Godot;
using System;

public abstract partial class LevelSwapDoor : Door{
    [ExportGroup(nameof(LevelSwapDoor))]
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
        DoorManager.Instance.SetExitDoorId(doorToLoadTo);
        EntityManager.Singleton.PauseEntityProcesses();
        SceneManager.Singleton.Swap3D(levelToLoad, 0.5f);
    }

    protected override void Opened(){
        base.Opened();
        EnableEnterZone();
    }

    protected override void Closed(){
        base.Closed();
        DisableEnterZone();
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
