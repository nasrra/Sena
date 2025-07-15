// using Godot;
// using System;
// using Godot.Collections;

// public partial class Interactor : Node3D{
//     private Interactable previous = null;
//     private Interactable current = null;
//     [Export] private Array<Interactable> inRange;
//     [Export] Area3D area;
//     [Export(PropertyHint.Layers2DPhysics)]
//     public uint ObstructionLayer;

//     public override void _Ready(){
//         base._Ready();
//         LinkEvents();
//     }

//     public override void _ExitTree(){
//         base._ExitTree();
//         UnlinkEvents();
//     }

//     public void Interact(){
//         if(previous != null){
//             previous.Interact();
//     }
//     }

//     private void OnAreaEnter(Node3D other){
//         Interactable interactable = (Interactable)other;
//         if(interactable != null){
//             inRange.Add(interactable);
//         }
//     } 

//     private void OnAreaExit(Node3D other){
//         Interactable interactable = (Interactable)other;
//         if(interactable != null){
//             inRange.Remove(interactable);
//         }
//     }

//     public override void _PhysicsProcess(double delta)
//     {
//         base._PhysicsProcess(delta);

//         if (inRange.Count > 0)
//         {
//             Camera3D camera = Player.Camera;
//             Interactable closestValid = null;
//             float minDistance = float.MaxValue;

//             foreach (Interactable other in inRange)
//             {
//                 Vector3 direction = other.GlobalPosition - camera.GlobalPosition;
//                 float dot = -camera.GlobalBasis.Z.Normalized().Dot(direction.Normalized());

//                 // check if the camera is pointing at the interactable.

//                 if (dot < other.cameraDotThreshold)
//                     continue;

//                 // check if there is an obstruction.

//                 var result = GetWorld3D().DirectSpaceState.IntersectRay(new PhysicsRayQueryParameters3D
//                 {
//                     From = camera.GlobalPosition,
//                     To = other.GlobalPosition,
//                     CollisionMask = ObstructionLayer,
//                     HitFromInside = true
//                 });

//                 if (result.Count > 0)
//                     continue;

//                 // check if the distance to the item is less than the previous iteration.

//                 float distSquared = direction.LengthSquared();
//                 if (distSquared < minDistance){
//                     minDistance = distSquared;
//                     closestValid = other;
//                 }
//             }

//             // Handle state transition

//             if (closestValid != previous){
//                 previous?.IdleState();
//                 closestValid?.HoveredState();
//                 previous = closestValid;
//             }
//         }

//         // if we have left all interactables.

//         else if (previous != null){
//             previous.IdleState();
//             previous = null;
//         }
//     }

//     private void LinkEvents(){
//         area.AreaEntered    += OnAreaEnter;
//         area.AreaExited     += OnAreaExit;
//     }

//     private void UnlinkEvents(){
//         area.AreaEntered    -= OnAreaEnter;
//         area.AreaExited     -= OnAreaExit;
//     }
// }
