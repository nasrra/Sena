using Godot;
using System;

public partial class DeathScreenGui : Control{
    
    [Export] private Button respawnButton;


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


    private void RespawnButtonPressed(){
        GD.Print("respawn!");
        SceneManager.Instance.ReloadScene2D();
    }

    public void LinkToRespawnButtonPressed(Action callback){
        respawnButton.Pressed += callback;
    }

    public void UnlinkFromRespawnButtonPressed(Action callback){
        respawnButton.Pressed -= callback;
    }

    /// 
    /// Linkage.
    /// 


    private void LinkEvents(){
        respawnButton.Pressed += RespawnButtonPressed;
    }

    private void UnlinkEvents(){
        respawnButton.Pressed -= RespawnButtonPressed;
    }
}
