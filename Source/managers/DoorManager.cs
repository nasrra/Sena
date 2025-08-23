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
    [Export] private Array<Array<NodePath>> tempLockAreas = new Array<Array<NodePath>>();
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
        LinkEvents();
    }

    public override void _ExitTree(){
        base._ExitTree();
        UnlinkEvents();
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


    /// 
    /// Linkage.
    /// 


    private void LinkEvents(){
        LinkTempLockAreas();
    }

    private void UnlinkEvents(){
        UnlinkTempLockAreas();
    }


    /// 
    /// Temp Lock Areas.
    /// 


    private void LinkTempLockAreas(){
        for(int i = 0; i < tempLockAreas.Count; i++){
        for(int j = 0; j < tempLockAreas[i].Count; j++){
            Area3D area = GetNode<Area3D>(tempLockAreas[i][j]);
            area.BodyEntered += OnTempLockAreaEnteredCallback;
            area.AreaEntered += OnTempLockAreaEnteredCallback;
        }}
    }

    private void UnlinkTempLockAreas(){
        for(int i = 0; i < tempLockAreas.Count; i++){
        for(int j = 0; j < tempLockAreas[i].Count; j++){
            Area3D area = GetNode<Area3D>(tempLockAreas[i][j]);
            area.BodyEntered -= OnTempLockAreaEnteredCallback;
            area.AreaEntered -= OnTempLockAreaEnteredCallback;
        }}
    }

    private void OnTempLockAreaEnteredCallback(Node3D other){
        TempLockAndCloseDoors();
    }

    public void DisableAllTemplockAreas(){
        for(int i = 0; i < tempLockAreas.Count; i++){
        for(int j = 0; j < tempLockAreas[i].Count; j++){
            Area3D area = GetNode<Area3D>(tempLockAreas[i][j]);
            area.GetNode<CollisionShape3D>("CollisionShape3D").Disabled = true;
        }}
    }

    public void EnableAllTemplockAreas(){
        for(int i = 0; i < tempLockAreas.Count; i++){
        for(int j = 0; j < tempLockAreas[i].Count; j++){
            Area3D area = GetNode<Area3D>(tempLockAreas[i][j]);
            area.GetNode<CollisionShape3D>("CollisionShape3D").Disabled = false;
        }}
    }

    public void DisableTemplockAreas(int areaGroup){
        if(tempLockAreas.Count <= areaGroup){
            return;
        }

        for(int i = 0; i < tempLockAreas[areaGroup].Count; i++){
            Area3D area = GetNode<Area3D>(tempLockAreas[areaGroup][i]);
            area.GetNode<CollisionShape3D>("CollisionShape3D").Disabled = true;
        }
    }

    public void EnableTemplockAreas(int areaGroup){
        if(tempLockAreas.Count <= areaGroup){
            return;
        }
        
        for(int i = 0; i < tempLockAreas[areaGroup].Count; i++){
            Area3D area = GetNode<Area3D>(tempLockAreas[areaGroup][i]);
            area.GetNode<CollisionShape3D>("CollisionShape3D").Disabled = false;
        }
    }
}