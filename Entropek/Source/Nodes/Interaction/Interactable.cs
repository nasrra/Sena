using Godot;
using System;

public partial class Interactable : Area2D{
    public const string NodeName = nameof(Interactable); 
    public event Action OnInteract;
    private bool interactable = true;

    public override void _Ready(){
        #if TOOLS
        Entropek.Util.Node.VerifyName(this, NodeName);
        #endif
    }

    public override void _PhysicsProcess(double delta){
        base._PhysicsProcess(delta);
    }

    public void Interact(){
        OnInteract?.Invoke();
    }
}
