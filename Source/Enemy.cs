using Godot;
using System;

public partial class Enemy : Node2D{
    [Export] Health health;

    public override void _EnterTree(){
        base._EnterTree();
        LinkEvents();
    }

    public override void _ExitTree(){
        base._ExitTree();
        UnlinkEvents();
    }

    public void Kill(){
        QueueFree();
    }

    private void LinkEvents(){
        health.OnDeath += Kill;
    }

    private void UnlinkEvents(){
        health.OnDeath -= Kill;
    }
}
