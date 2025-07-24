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
    }

    public void GameplayState(){
        State = GameState.Gameplay;
    }


    /// 
    /// Linkage.
    /// 


    public void LinkToPlayer(){
        Player.Instance.OnDeath += DeathState;
    }

    public void UnlinkFromPlayer(){
        Player.Instance.OnDeath -= DeathState;
    }
}

public enum GameState : byte{
    Gameplay,
    Paused,
    MainMenu,
    Death,
}
