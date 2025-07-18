using Godot;
using System;

public partial class Door : Node{
    public const string NodeName = nameof(Door);
    [Export]
    private CollisionObject2D collider;
    [Export]
    private AStarAgent aStarAgent;
    [Export]
    public bool Locked {get;private set;} = false;
    [Export]
    public bool Opened {get;private set;} = false;

    public event Action OnOpen;
    public event Action OnClose;
    public event Action OnLock;
    public event Action OnUnlock;

    public override void _Ready(){
        base._Ready();
        #if TOOLS
        Entropek.Util.Node.VerifyName(this, NodeName);
        #endif
    }


    public void Open(){
        Opened = true;
        CollisionShape2D shape = collider.GetNode<CollisionShape2D>("CollisionShape2D");
        shape.CallDeferred("set_disabled", true);
        aStarAgent.Disable();
        OnOpen?.Invoke();
    }

    public void Close(){
        Opened = false;
        CollisionShape2D shape = collider.GetNode<CollisionShape2D>("CollisionShape2D");
        shape.CallDeferred("set_disabled", false);
        aStarAgent.Enable();
        OnClose?.Invoke();
    }

    public void Unlock(){
        Locked = false;
        OnUnlock?.Invoke();
    }

    public void Lock(){
        Locked = true;
        OnLock?.Invoke();
    }
}