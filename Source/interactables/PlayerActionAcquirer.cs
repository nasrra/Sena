using System;
using Godot;

public partial class PlayerActionAcquirer : Node2D{
    [Export] private Interactable interactable;
    [Export] private Label interactIcon;
    [Export] private PlayerActions playerActions;

    public override void _EnterTree(){
        base._EnterTree();
        interactable.OnInteract += OnInteractCallback;
    }

    public override void _Ready(){
        base._Ready();
        interactable.SetInteractIcon(interactIcon);
    }

    public override void _ExitTree(){
        base._ExitTree();
        interactable.OnInteract -= OnInteractCallback;
    }

    private void OnInteractCallback(Interactor interactor){
        Player player = interactor.GetParent() as Player;
        if(player == null || player.IsPlayerActionEnabled(playerActions) == true){
            return;
        }
        player.EnablePlayerAction(playerActions);
    }
}