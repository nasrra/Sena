using Godot;
using Godot.Collections;
using System;

public partial class LevelSwapDoorManager : Node{
    public static LevelSwapDoorManager Instance {get;private set;}
    public static int exitDoorId = -1;
    [Export] private Array<LevelSwapDoor> doors;

    public override void _Ready(){
        base._Ready();
        Instance = this;
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
}
