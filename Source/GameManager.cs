using Godot;
using System;

public partial class GameManager : Node{

    public static GameManager Instance {get; private set;}
    
    public GameState State {get; private set;}


    /// 
    /// Base.
    /// 


    public override void _EnterTree(){
        base._EnterTree();
        Instance = this;
    }

    public override void _ExitTree(){
        base._ExitTree();
        Instance = null;
    }


    ///
    /// States. 
    ///


    public void DeathState(){
        State = GameState.Death;
        GameplayGui ui = (GameplayGui)GetNode("/root/Main/GUI/GameplayGui");
        ui.EnableDeathGui();
    }

    public void GameplayState(){
        State = GameState.Gameplay;
        GameplayGui ui = (GameplayGui)GetNode("/root/Main/GUI/GameplayGui");
        ui.EnableHudGui();
    }
}

public enum GameState : byte{
    Gameplay,
    Paused,
    MainMenu,
    Death,
}
