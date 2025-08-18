using Godot;
using System;
using Godot.Collections;
using Entropek.Collections;

public partial class Interactor : Area3D{
    private Interactable previous = null;
    private Interactable current = null;
    private SwapbackList<Interactable> inRange = new SwapbackList<Interactable>();
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
        if(current != null){
            current.Interact(this);
        }
    }

    private void OnAreaEnter(Node3D other){
        Interactable interactable = (Interactable)other;
        if(interactable != null){
            inRange.Add(interactable);
        }
    } 

    private void OnAreaExit(Node3D other){
        Interactable interactable = (Interactable)other;
        if(interactable != null){
            inRange.Remove(interactable);
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        Interactable closestValid = null;
        float minDistance = float.MaxValue;

        for(int i = 0; i < inRange.Count; i++){
            Interactable other = inRange[i];
            Vector3 direction = other.GlobalPosition - GlobalPosition;

            // check if there is an obstruction.
            var result = GetWorld3D().DirectSpaceState.IntersectRay(new PhysicsRayQueryParameters3D
            {
                From = GlobalPosition,
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

        if (closestValid != current){
            current = closestValid;
        }

        // if we have left all interactables.

        if (current != previous){
            current?.InteractorPriorityState(this);
            previous?.IdleState();
        }

        previous = current;
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
