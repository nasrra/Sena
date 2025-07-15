using Godot;
using System;

public partial class Enemy : CharacterBody2D{ // <-- make sure to inherit from CollisionObect2D for hitbox handler and Player.
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