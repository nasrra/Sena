using Godot;
using System;

public partial class GameplayGui : Control{


    public static GameplayGui Singleton {get;private set;}

    /// 
    /// Variables
    /// 

    [Export] public DeathScreenGui DeathGui {get; private set;}
    [Export] public TutorialGui TutorialGui {get; private set;}
    [Export] public BossHealthBarHud BossHealthBarHud {get; private set;}
    [Export] public Control HudGui {get; private set;}
    [Export] public Control PauseMenuGui {get; private set;}
    [Export] GameplayGuiState state; 


    /// 
    /// Definitions.
    /// 


    private enum GameplayGuiState : byte{
        Death,
        Hud,
        PauseMenu,
    }


    /// 
    /// Base.
    /// 

    public override void _EnterTree(){
        base._EnterTree();
        
        DeathGui.Visible    = true;
        HudGui.Visible      = true;
        PauseMenuGui.Visible = true;
        
        RemoveChild(DeathGui);
        RemoveChild(HudGui);
        RemoveChild(PauseMenuGui);
        
        EnableCurrentGui();
        
        Singleton = this;
    }

    public override void _ExitTree(){
        base._ExitTree();
        
        DeathGui.QueueFree();
        HudGui.QueueFree();
    
        Singleton = null;
    }


    ///
    /// Functions.
    ///


    public void DeathState(){
        DisableCurrentGui();
        state = GameplayGuiState.Death;
        AddChild(DeathGui);
    }

    public void HudState(){
        DisableCurrentGui();
        state = GameplayGuiState.Hud;
        AddChild(HudGui);
        GD.Print(state);
    }

    public void PauseMenuState(){
        DisableCurrentGui();
        state = GameplayGuiState.PauseMenu;
        AddChild(PauseMenuGui);
        GD.Print(state);
    }

    private void DisableCurrentGui(){
        switch(state){
            case GameplayGuiState.Death:
                RemoveChild(DeathGui);
            break;
            case GameplayGuiState.Hud:
                RemoveChild(HudGui);
            break;
            case GameplayGuiState.PauseMenu:
                RemoveChild(PauseMenuGui);
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
            case GameplayGuiState.PauseMenu:
                AddChild(PauseMenuGui);
            break;
        }
    }
}
