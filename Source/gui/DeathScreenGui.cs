using Godot;
using System;

public partial class DeathScreenGui : Control{
    
    [Export] public Button respawnButton;


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
        RespawnPoint.Respawn();
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
