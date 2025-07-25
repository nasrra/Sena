using Godot;
using System;

public partial class GameplayGui : Control{


    /// 
    /// Variables
    /// 

    [Export] public DeathScreenGui DeathGui {get; private set;}
    [Export] public Control HudGui {get; private set;}
    [Export] GameplayGuiState state; 


    /// 
    /// Definitions.
    /// 


    private enum GameplayGuiState : byte{
        Death,
        Hud,
    }


    /// 
    /// Base.
    /// 

    public override void _EnterTree(){
        base._EnterTree();
        
        DeathGui.Visible    = true;
        HudGui.Visible      = true;
        
        RemoveChild(DeathGui);
        RemoveChild(HudGui);
        
        EnableCurrentGui();

        LinkEvents();
    }

    public override void _ExitTree(){
        base._ExitTree();
        
        DeathGui.QueueFree();
        HudGui.QueueFree();

        UnlinkEvents();
    }


    ///
    /// Functions.
    ///


    public void EnableDeathGui(){
        DisableCurrentGui();
        state = GameplayGuiState.Death;
        AddChild(DeathGui);
    }

    public void EnableHudGui(){
        DisableCurrentGui();
        state = GameplayGuiState.Hud;
        AddChild(HudGui);
    }

    private void DisableCurrentGui(){
        switch(state){
            case GameplayGuiState.Death:
                RemoveChild(DeathGui);
            break;
            case GameplayGuiState.Hud:
                RemoveChild(HudGui);
            break;
        }
    }

    private void EnableCurrentGui(){
        switch(state){
            case GameplayGuiState.Death:
                AddChild(DeathGui);
            break;
            case GameplayGuiState.Hud:
                AddChild(HudGui);
            break;
        }
    }


    ///
    /// Linkage.
    /// 


    private void LinkEvents(){
        // DeathGui.respawnButton.Pressed += EnableHudGui;
    }

    private void UnlinkEvents(){
        // DeathGui.respawnButton.Pressed -= EnableHudGui;
    }
}
