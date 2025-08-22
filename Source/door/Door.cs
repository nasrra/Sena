using Entropek.Ai;
using Godot;
using System;

// // seperate into scene transition and environmental doors.

public abstract partial class Door : Node{
    [ExportGroup("Door")]
    [Export] private CollisionObject3D collider;
    [Export] public bool IsLocked {get;private set;} = false;
    [Export] public bool IsOpened {get;private set;} = false;

    public event Action OnOpened;
    public event Action OnClosed;
    public event Action OnLocked;
    public event Action OnUnlocked;


    /// 
    /// Base.
    /// 


    public override void _Ready(){
        base._Ready();
        // #if TOOLS
        //     Entropek.Util.Node.VerifyName(this, NodeName);
        // #endif
        if(IsOpened==true){
            Opened();
        }
        else{
            Closed();
        }
    }


    /// 
    /// 
    /// 

    public abstract void Open();
   
    protected virtual void Opened(){
        IsOpened = true;
        DisableCollider();
        OnOpened?.Invoke();
    }

    public abstract void Close();
    
    protected virtual void Closed(){
        IsOpened = false;
        EnableCollider();
        OnClosed?.Invoke();
    }

    public abstract void Unlock();
    
    protected virtual void Unlocked(){
        IsLocked = false;
        OnUnlocked?.Invoke();
    }

    public abstract void Lock();
    
    public virtual void Locked(){
        IsLocked = true;
        OnLocked?.Invoke();
    }

    protected void EnableCollider(){
        CollisionShape3D shape = collider.GetNode<CollisionShape3D>("CollisionShape3D");
        shape.CallDeferred("set_disabled", false);
    }

    protected void DisableCollider(){
        CollisionShape3D shape = collider.GetNode<CollisionShape3D>("CollisionShape3D");
        shape.CallDeferred("set_disabled", true);
    }

    public void SetState(bool opened, bool locked){
        if(opened==true){
            Open();
        }
        else{
            Close();
        }
        if(locked==true){
            Lock();
        }
        else{
            Unlock();
        }
    }
}