using Godot;
using System;

public partial class GameManager : Node{

    public static GameManager Instance {get; private set;}
    
    public GameState State {get; private set;}

    private event Action OnPauseInput = null;

    /// 
    /// Base.
    /// 


    public override void _EnterTree(){
        base._EnterTree();
        GD.Randomize();
        Instance = this;
        LinkEvents();
    }


    public override void _Ready(){
        base._Ready();
        GameplayState();
    }

    public override void _ExitTree(){
        base._ExitTree();
        Instance = null;
        UnlinkEvents();
    }


    ///
    /// States. 
    ///


    public void DeathState(){
        
        State = GameState.Death;
        
        if(GetGameplayUi(out GameplayGui ui)==false){
            return;
        }        
        ui.HudState();
        
        ui.DeathState();

        OnPauseInput = null;
    }

    public void GameplayState(){
        
        State = GameState.Gameplay;
        
        if(GetGameplayUi(out GameplayGui ui)==false){
            return;
        }        
        ui.HudState();
        
        InputManager.Singleton.ResumeGameplayInput();
        EntityManager.Singleton.ResumeEntityProcesses();

        OnPauseInput = PauseMenuState;
    }

    public void PauseMenuState(){
        State = GameState.PauseMenu;

        if(GetGameplayUi(out GameplayGui ui)==false){
            return;
        }
        ui.PauseMenuState();

        InputManager.Singleton.PauseGameplayInput();
        EntityManager.Singleton.PauseEntityProcesses();

        OnPauseInput = GameplayState;
    }

    private bool GetGameplayUi(out GameplayGui gameplayGui){
        Node node = GetNodeOrNull("/root/Main/GUI/GameplayGui");
        if(node==null){
            gameplayGui = null;
            return false;
        }
        gameplayGui = (GameplayGui)node;
        return true;
    }


    /// 
    /// Linkage.
    /// 


    private void LinkEvents(){
        InputManager.Singleton.OnPauseInput += InvokeOnPauseInput;
    }

    private void UnlinkEvents(){
        InputManager.Singleton.OnPauseInput -= InvokeOnPauseInput;
    }


    ///
    /// Linkage functions.
    /// 


    private void InvokeOnPauseInput(){
        GD.Print("recieved");
        OnPauseInput?.Invoke();
    }
}

public enum GameState : byte{
    Gameplay,
    PauseMenu,
    MainMenu,
    Death,
}
