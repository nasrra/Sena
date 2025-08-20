using Entropek.Ai;
using Godot;
using System;

// // seperate into scene transition and environmental doors.

public abstract partial class Door : Node{
    [ExportGroup("Door")]
    [Export] protected CollisionObject3D collider;
    [Export] public bool Locked {get;private set;} = false;
    [Export] public bool Opened {get;private set;} = false;

    public event Action OnOpen;
    public event Action OnClose;
    public event Action OnLock;
    public event Action OnUnlock;

    public override void _Ready(){
        base._Ready();
        // #if TOOLS
        //     Entropek.Util.Node.VerifyName(this, NodeName);
        // #endif
        if(Opened==true){
            Open();
        }
        else{
            Close();
        }
    }

    public override void _EnterTree(){
        base._EnterTree();
    }


    public virtual void Open(){
        Opened = true;
        CollisionShape3D shape = collider.GetNode<CollisionShape3D>("CollisionShape3D");
        shape.CallDeferred("set_disabled", true);
        OnOpen?.Invoke();
    }

    public virtual void Close(){
        Opened = false;
        CollisionShape3D shape = collider.GetNode<CollisionShape3D>("CollisionShape3D");
        shape.CallDeferred("set_disabled", false);
        OnClose?.Invoke();
    }

    public virtual void Unlock(){
        Locked = false;
        OnUnlock?.Invoke();
    }

    public virtual void Lock(){
        Locked = true;
        OnLock?.Invoke();
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