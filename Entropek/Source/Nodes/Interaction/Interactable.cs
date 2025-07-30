using Godot;
using System;

public partial class Interactable : Area2D{
    public const string NodeName = nameof(Interactable);
    [Export] private Label InteractIcon; 
    public event Action<Interactor> OnInteract;
    public bool IsInteractable = true;

    public override void _Ready(){
        #if TOOLS
        Entropek.Util.Node.VerifyName(this, NodeName);
        #endif
    }

    public override void _PhysicsProcess(double delta){
        base._PhysicsProcess(delta);
    }

    public void Interact(Interactor interactor){
        if(IsInteractable == true){
            OnInteract?.Invoke(interactor);
        }
    }

    public void EnableInteractableIcon(){
        if(IsInteractable == true && InteractIcon != null){
            InteractIcon.Visible = true;
        }
    }

    public void DisableInteractableIcon(){
        if(InteractIcon != null){
            InteractIcon.Visible = false;
        }
    }

    public void EnableInteraction(){
        IsInteractable = true;
    }

    public void DisableInteraction(){
        IsInteractable = false;
        DisableInteractableIcon();
    }
}
