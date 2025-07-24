using Godot;
using System;

public partial class LevelSwapDoor : Door{

    [Export] private Area2D enterZone;
    [Export] public Node2D ExitPoint {get; private set;}
    [Export] Timer enterZoneDisableTimer;
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
        SceneManager.Instance.LoadScene2D(levelToLoad, SceneLoadType.DELETE, 0.5f);
        CameraController.Instance.FadeToBlack(0.33f);
        EntityManager.Instance.PauseEntityProcesses();
    }

    public void Exit(float exitTime){
        enterZoneDisableTimer.WaitTime = exitTime;
        enterZoneDisableTimer.Start();
        CameraController.Instance.FadeFromBlack(0.33f);
        DisableEnterZone();
    }

    public override void Open(){
        GD.Print(enterZoneDisableTimer.TimeLeft);
        if(enterZoneDisableTimer.TimeLeft <= 0){
            EnableEnterZone();
        }
        base.Open();
    }

    public override void Close(){
        DisableEnterZone();
        base.Close();
    }

    private void EnableEnterZone(){
        GD.Print("enable enter zone!");
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
        enterZoneDisableTimer.Timeout += EnableEnterZone;
    }

    private void UnlinkEvents(){
        enterZone.BodyEntered -= OnEnterZone;        
        enterZoneDisableTimer.Timeout -= EnableEnterZone;
    }


    ///
    /// Linkage Functions.
    /// 


    private void OnEnterZone(Node2D other){
        
        if(PhysicsManager.Instance.GetPhysics2DLayerName((other as CollisionObject2D).CollisionLayer, out string hitLayer) == false){
            return;
        }

        switch(hitLayer){
            case "Player":
                Enter();
                break;
            default:
                throw new Exception($"{hitLayer} not implemented.");
        }
    }
}
