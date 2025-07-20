using Godot;
using System;

namespace Entropek.Ai;

public struct CellData{
    
    public CellData(){
        Locked = false;
        Blocked = false;
        Clearance = 0;
        // PassInhabitants = 0;
        // BlockInhabitants = 0;
    }
    
    public byte Clearance           {get;private set;}
    public bool Locked              {get;private set;}
    public bool Blocked             {get;private set;}
    
    // public int PassInhabitants      {get;private set;}
    // public int BlockInhabitants     {get;private set;}

    // public void AddPassInhabitant(){
    //     PassInhabitants += 1;
    //     GD.Print(PassInhabitants);
    // } 

    // public void RemovePassInhabitant(){
    //     PassInhabitants -= 1;
    //     GD.Print(PassInhabitants);
    // }

    // public void AddBlockInhabitant(){
    //     BlockInhabitants += 1;
    //     GD.Print(BlockInhabitants);
    // }

    // public void RemoveBlockInhabitant(){
    //     BlockInhabitants -= 1;
    //     GD.Print(BlockInhabitants);
    // }

    public void SetClearance(byte clearance){
        Clearance = clearance;
    }

    public void SetBlocked(bool blocked){
        Blocked = blocked;
    }

    public void UnlockData(){
        Locked = false;
    }

    public void LockData(){
        Locked = true;
    }
}
