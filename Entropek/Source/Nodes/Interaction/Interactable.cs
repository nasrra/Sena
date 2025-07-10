// using Godot;
// using System;

// public partial class Interactable : Area3D{
//     public const string NodeName = "Interactable"; 
//     [Export] private Node3D text;
//     [Export] public float cameraDotThreshold {get; private set;} = 0.99f;
//     public event Action OnInteract;
    
//     private bool hovered = false; // <-- if the interactable is being hovered over;
//     private bool interactable = true;

//     public override void _Ready(){
//         #if DEBUG
//         Entropek.Util.Node.VerifyName(this, NodeName);
//         #endif
//         IdleState();
//     }


//     public override void _PhysicsProcess(double delta){
//         base._PhysicsProcess(delta);
//         if(hovered == true){
//             text.LookAt(new Vector3(Player.Camera.GlobalPosition.X, Player.Camera.GlobalPosition.Y, Player.Camera.GlobalPosition.Z));
//         }
//     }

//     public void HoveredState(){
//         hovered = true;
//         text.Visible = true;
//     }

//     public void IdleState(){
//         hovered = false;
//         text.Visible = false;
//     }

//     public void Interact(){
//         OnInteract?.Invoke();
//     }
// }
