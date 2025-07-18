using Godot;
using System;
using System.Collections.Generic;

public partial class Enemy : CharacterBody2D{ // <-- make sure to inherit from CollisionObect2D for hitbox handler and Player.
    [Export] private Health health;
    [Export] private AStarAgent aStarAgent;
    [Export] private CharacterMovement characterMovement;
    [Export] private Node2D target;
    private Queue<Vector2> pathToTarget;

    public override void _EnterTree(){
        base._EnterTree();
        LinkEvents();
    }

    public override void _ExitTree(){
        base._ExitTree();
        UnlinkEvents();
    }

    public override void _Process(double delta){
        base._Process(delta);
        if(pathToTarget != null && pathToTarget.Count > 0){
            Vector2 distance = pathToTarget.Peek() - GlobalPosition;
            characterMovement.Move(pathToTarget.Peek() - GlobalPosition);
            if(distance.LengthSquared() <= 100){
                pathToTarget.Dequeue();
            }
            // GD.Print(distance.LengthSquared());
        }
    }

    public override void _PhysicsProcess(double delta){
        base._PhysicsProcess(delta);
        pathToTarget = aStarAgent.GetPathToPosition(target.GlobalPosition);
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