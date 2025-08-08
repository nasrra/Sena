using System;
using Godot;

public partial class PlayerActionAcquirer : Node2D{
    

    /// 
    /// Variables.
    /// 
    

    [Export] private Interactable interactable;
    [Export] private Label interactIcon;
    [Export] private PlayerActions playerActions;


    /// 
    /// Base.
    /// 


    public override void _EnterTree(){
        base._EnterTree();
        LinkEvents();
    }

    public override void _Ready(){
        base._Ready();
        interactable.SetInteractIcon(interactIcon);
    }

    public override void _ExitTree(){
        base._ExitTree();
        UnlinkEvents();
    }


    /// 
    /// Functions.
    /// 


    private void OnInteractCallback(Interactor interactor){
        Player player = interactor.GetParent() as Player;
        if(player == null || player.IsPlayerActionEnabled(playerActions) == true){
            return;
        }
        player.EnablePlayerAction(playerActions);
        interactable.DisableInteraction();
    }

    private void OnInteractorPriorityStateCallback(Interactor interactor){
        if(interactor.GetParent() is Player player && player.IsPlayerActionEnabled(playerActions)==true){
            interactable.DisableInteraction();
        }
    }


    /// 
    /// Linkage.
    /// 


    private void LinkEvents(){
        interactable.OnInteract                 += OnInteractCallback;
        interactable.OnInteractorPriorityState  += OnInteractorPriorityStateCallback;
    }

    private void UnlinkEvents(){
        interactable.OnInteract                 -= OnInteractCallback;
        interactable.OnInteractorPriorityState  -= OnInteractorPriorityStateCallback;
    }
}