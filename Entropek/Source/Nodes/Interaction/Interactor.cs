using Godot;
using System;
using Godot.Collections;

public partial class Interactor : Area2D{
    private Interactable previous = null;
    private Interactable current = null;
    [Export] private Array<Interactable> inRange;
    [Export(PropertyHint.Layers2DPhysics)]
    public uint ObstructionLayer;

    public override void _Ready(){
        base._Ready();
        LinkEvents();
    }

    public override void _ExitTree(){
        base._ExitTree();
        UnlinkEvents();
    }

    public void Interact(){
        if(previous != null){
            previous.Interact(this);
        }
    }

    private void OnAreaEnter(Node2D other){
        Interactable interactable = (Interactable)other;
        if(interactable != null){
            inRange.Add(interactable);
        }
    } 

    private void OnAreaExit(Node2D other){
        Interactable interactable = (Interactable)other;
        if(interactable != null){
            inRange.Remove(interactable);
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        if (inRange.Count > 0)
        {
            Interactable closestValid = null;
            float minDistance = float.MaxValue;

            foreach (Interactable other in inRange){
                Vector2 direction = other.GlobalPosition - Player.Instance.GlobalPosition;

                // check if there is an obstruction.
                var result = GetViewport().World2D.DirectSpaceState.IntersectRay(new PhysicsRayQueryParameters2D
                {
                    From = Player.Instance.GlobalPosition,
                    To = other.GlobalPosition,
                    CollisionMask = ObstructionLayer,
                    HitFromInside = true
                });

                if (result.Count > 0)
                    continue;

                // check if the distance to the item is less than the previous iteration.

                float distSquared = direction.LengthSquared();
                if (distSquared < minDistance){
                    minDistance = distSquared;
                    closestValid = other;
                }
            }

            // Handle state transition

            if (closestValid != previous){
                // previous?.IdleState();
                // closestValid?.HoveredState();
                previous = closestValid;
            }
        }

        // if we have left all interactables.

        else if (previous != null){
            // previous.IdleState();
            previous = null;
        }
    }

    private void LinkEvents(){
        AreaEntered    += OnAreaEnter;
        AreaExited     += OnAreaExit;
    }

    private void UnlinkEvents(){
        AreaEntered    -= OnAreaEnter;
        AreaExited     -= OnAreaExit;
    }
}
