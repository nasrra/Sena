using Godot;
using Godot.Collections;
using System;

public partial class LevelSwapDoorManager : Node{
    public static LevelSwapDoorManager Instance {get;private set;}
    public static int exitDoorId = -1;
    [Export] private Array<LevelSwapDoor> doors;
    private (bool,bool)[] doorStateCache;

    private State state = State.Normal;

    private enum State : byte{
        TempLocked,
        Normal,
    }

    public override void _Ready(){
        base._Ready();
        Instance = this;
        doorStateCache = new (bool, bool)[doors.Count];
    }

    public bool GetDoor(int id, out LevelSwapDoor door){
        door = null;
        if(id >= doors.Count){
            return false;
        }
        door = doors[id];
        return true;
    }

    public void SetExitDoorId(int id){
        exitDoorId = id;
    }

    public bool GetExitDoor(out LevelSwapDoor door){
        GD.Print($"exit door: {exitDoorId}");
        door = null;
        if(exitDoorId >= doors.Count || exitDoorId < 0){
            return false;
        }
        door = doors[exitDoorId];
        return true;
    }

    public void TempLockAndCloseDoors(){
        
        if(state == State.TempLocked){
            return;
        }
        state = State.TempLocked;
        
        for(int i = 0; i < doors.Count; i++){
            LevelSwapDoor door = doors[i];
            doorStateCache[i] = (door.IsOpened, door.IsLocked);
            door.Lock();
            door.Close();
            GD.Print("temp lock");
        }
    }

    public void RestoreDoorStates(){

        if(state == State.Normal){
            return;
        }
        state = State.Normal;

        for(int i = 0; i < doors.Count; i++){
            LevelSwapDoor door = doors[i];
            door.SetState(doorStateCache[i].Item1, doorStateCache[i].Item2);
        }
    }
}