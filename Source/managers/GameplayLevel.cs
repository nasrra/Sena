using Godot;
using System;

public partial class GameplayLevel : Node3D{
	[ExportGroup("Nodes")]
	[Export] private EnemyManager enemyManager;
	[Export] private DoorManager doors;

	[ExportGroup("Variables")]
	[Export] public GameplayLevelState state {get; private set;} = GameplayLevelState.InProgress;


	/// 
	/// Base.
	/// 


	public override void _EnterTree(){
		base._EnterTree();
		LinkEvents();
	}

	public override void _Ready(){
		base._Ready();
		switch (state){
			case GameplayLevelState.Cleared:
				ClearedState();
			break;
			case GameplayLevelState.InProgress:
				InProgressState();
			break;
		}
	}   

	public override void _ExitTree(){
		base._ExitTree();
		UnlinkEvents();
	}


	/// 
	/// States.
	/// 

	public void ClearedState(){
		doors.RestoreDoorStates();
		state = GameplayLevelState.Cleared;
	}

	public void InProgressState(){
		doors.TempLockAndCloseDoors();
		state = GameplayLevelState.InProgress;
	}


	/// 
	/// Linkage.
	/// 


	private void LinkEvents(){
		enemyManager.OnEnemyGroupKilled += OnEnemyGroupKilledCallback;
	}

	private void UnlinkEvents(){        
		enemyManager.OnEnemyGroupKilled -= OnEnemyGroupKilledCallback;
	}


	///
	/// Callbacks.
	/// 


	private void OnEnemyGroupKilledCallback(int enemyGroup){
		doors.DisableTemplockAreas(enemyGroup);
		ClearedState();
	}
}


/// 
/// Definitions.
/// 


public enum GameplayLevelState : byte{
	Cleared,
	InProgress,
}
