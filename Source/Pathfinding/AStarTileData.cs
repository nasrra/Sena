using Godot;
using System;

public partial class AStarTileData{
    public bool Locked {get;private set;} = false;
    public int PassInhabitants      {get;private set;} = 0; 
    public int BlockInhabitants     {get;private set;} = 0;

    public void AddPassInhabitant(){
        PassInhabitants += 1;
        GD.Print(PassInhabitants);
    } 

    public void RemovePassInhabitant(){
        PassInhabitants -= 1;
        GD.Print(PassInhabitants);
    }

    public void AddBlockInhabitant(){
        BlockInhabitants += 1;
        GD.Print(BlockInhabitants);
    }

    public void RemoveBlockInhabitant(){
        BlockInhabitants -= 1;
        GD.Print(BlockInhabitants);
    }

    public void UnlockData(){
        Locked = false;
    }

    public void LockData(){
        Locked = true;
    }

    public AStarTileType EvaluateType(){
        if(PassInhabitants + BlockInhabitants <= 0){
            // GD.Print("Set Open");
            return AStarTileType.Open;
        }
        else if(PassInhabitants > BlockInhabitants){
            // GD.Print("Set Pass");
            return AStarTileType.Pass;
        }
        else{
            // GD.Print("Set Blocked");
            return AStarTileType.Block;
        }
    }
}
