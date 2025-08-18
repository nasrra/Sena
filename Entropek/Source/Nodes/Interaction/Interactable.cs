using Godot;
using System;

public partial class Interactable : Area3D{
    public const string NodeName = nameof(Interactable);
    [Export] private Label interactIcon; 
    public event Action<Interactor> OnInteract;
    public event Action<Interactor> OnInteractorPriorityState;
    public event Action OnIdleState;
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
        if(IsInteractable == true && interactIcon != null){
            interactIcon.Visible = true;
        }
    }

    public void InteractorPriorityState(Interactor interactor){ // the interactable that is targeted by an interactor.
        
        if(IsInteractable==false){
            return;
        }

        EnableInteractableIcon();
        OnInteractorPriorityState?.Invoke(interactor);
    }

    public void IdleState(){
        
        if(IsInteractable==false){
            return;
        }

        DisableInteractableIcon();
        OnIdleState?.Invoke();
    }

    public void DisableInteractableIcon(){
        if(interactIcon != null){
            interactIcon.Visible = false;
        }
    }

    public void EnableInteraction(){
        IsInteractable = true;
    }

    public void DisableInteraction(){
        IsInteractable = false;
        DisableInteractableIcon();
    }

    public void SetInteractIcon(Label label){
        interactIcon = label;
    }
}
