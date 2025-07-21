// using Godot;
// using System;
// using System.Collections;
// using System.Collections.Generic;

// public partial class TestPathinder : CharacterBody2D{
//     Queue<Vector2> path;
//     [Export] CharacterMovement characterMovement;

//     public override void _Input(InputEvent @event){
//         base._Input(@event);
//         if(Input.IsActionJustPressed("Mouse2")){
//             path = PathfindingGrid.Instance.GetPathToPoint(GlobalPosition, GetGlobalMousePosition());
//         }
//     }

//     public override void _Process(double delta){
//         base._Process(delta);
//         if(path != null && path.Count > 0){
//             Vector2 distance = path.Peek() - GlobalPosition;
//             // GD.Print(distance.LengthSquared());
//             characterMovement.Move(path.Peek() - GlobalPosition);
//             if(distance.LengthSquared() <= 100){
//                 path.Dequeue();
//             }
//         }
//     }
// }
