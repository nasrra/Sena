using Godot;
using Godot.Collections;
using System;

public partial class DoorManager : Node{


    /// 
    /// Variables.
    /// 


    public static DoorManager Instance {get;private set;}
    public static int exitDoorId = -1;
    [Export] private Array<LevelSwapDoor> levelSwapDoors;
    [Export] private Array<EnvironmentDoor> environmentDoors;
    private (bool,bool)[] environmentDoorStatesCache;
    private (bool,bool)[] levelSwapDoorStatesCache;
    private State state = State.Normal;


    /// 
    /// Definitions.
    /// 


    private enum State : byte{
        TempLocked,
        Normal,
    }


    /// 
    /// Base.
    /// 


    public override void _Ready(){
        base._Ready();
        Instance = this;
        levelSwapDoorStatesCache = new (bool, bool)[levelSwapDoors.Count];
        environmentDoorStatesCache = new (bool, bool)[environmentDoors.Count];
    }


    /// 
    /// Shared door functions.
    /// 


    public void RestoreDoorStates(){
        if(state == State.Normal){
            return;
        }
        state = State.Normal;
        RestoreEnvironmentDoorStates();
        RestoreLevelSwapDoorStates();
    }

    public void TempLockAndCloseDoors(){
        
        if(state == State.TempLocked){
            return;
        }
        state = State.TempLocked;
        TempLockAndCloseEnvironmentDoors();
        TempLockAndCloseLevelSwapDoors();        
    }


    ///
    /// Level Swap Doors. 
    ///


    private void TempLockAndCloseLevelSwapDoors(){
        for(int i = 0; i < levelSwapDoors.Count; i++){
            LevelSwapDoor door = levelSwapDoors[i];
            levelSwapDoorStatesCache[i] = (door.IsOpened, door.IsLocked);
            door.Lock();
            door.Close();
        }
    }

    private void RestoreLevelSwapDoorStates(){

        for(int i = 0; i < levelSwapDoors.Count; i++){
            LevelSwapDoor door = levelSwapDoors[i];
            door.SetState(levelSwapDoorStatesCache[i].Item1, levelSwapDoorStatesCache[i].Item2);
        }
    }

    public bool GetLevelSwapDoor(int id, out LevelSwapDoor door){
        door = null;
        if(id >= levelSwapDoors.Count){
            return false;
        }
        door = levelSwapDoors[id];
        return true;
    }

    public void SetExitDoorId(int id){
        exitDoorId = id;
    }

    public bool GetExitDoor(out LevelSwapDoor door){
        GD.Print($"exit door: {exitDoorId}");
        door = null;
        if(exitDoorId >= levelSwapDoors.Count || exitDoorId < 0){
            return false;
        }
        door = levelSwapDoors[exitDoorId];
        return true;
    }


    /// 
    /// Environmental Doors.
    /// 


    private void TempLockAndCloseEnvironmentDoors(){
        for(int i = 0; i < environmentDoors.Count; i++){
            EnvironmentDoor door = environmentDoors[i];
            environmentDoorStatesCache[i] = (door.IsOpened, door.IsLocked);
            door.Lock();
            door.Close();
        }
    }

    private void RestoreEnvironmentDoorStates(){
        for(int i = 0; i < environmentDoors.Count; i++){
            EnvironmentDoor door = environmentDoors[i];
            door.SetState(environmentDoorStatesCache[i].Item1, environmentDoorStatesCache[i].Item2);
        }
    }

}