using Godot;
using System;

public partial class LeverOperatedEnvironmentDoor : EnvironmentDoor{
    [ExportGroup(nameof(LeverOperatedEnvironmentDoor))]
    [Export] Lever lever;


    /// 
    /// Base.
    /// 


    public override void _Ready(){
        LinkEvents();
        if(IsOpened==true){
            lever.SetOn();
        }
        else{
            lever.SetOff();
        }
        if(IsLocked==true){
            lever.Lock();
        }
        else{
            lever.Unlock();
        }
        base._Ready();
    }

    public override void _ExitTree(){
        UnlinkEvents();
        base._ExitTree();
    }


    /// 
    /// Functions.
    /// 


    public override void Lock(){
        lever.Lock();
        Locked();
    }

    public override void Unlock(){
        lever.Unlock();
        Unlocked();
    }

    protected override void OpenFailed(){
        throw new NotImplementedException();
    }


    /// 
    /// Linkage.
    /// 


    private void LinkEvents(){
        lever.OnToggledOn   += Open;
        lever.OnToggledOff  += Close;
    }

    private void UnlinkEvents(){
        lever.OnToggledOn   -= Open;
        lever.OnToggledOff  -= Close;
    }
}
