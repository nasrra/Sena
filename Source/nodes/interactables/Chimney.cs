using Godot;
using System;

public partial class Chimney : Node{
    [Export] private Interactable interactable;

    public override void _EnterTree(){
        base._EnterTree();
        LinkEvents();
    }

    public override void _ExitTree(){
        base._ExitTree();
        UnlinkEvents();
    }


    private void Interacted(Interactor interactor){
        EmberStorage embers = interactor.GetParent().GetNode<EmberStorage>(EmberStorage.NodeName);
        if(embers != null){
            embers.Add(EmberStorage.NotchMaxEmberValue);
        }
    }

    private void LinkEvents(){
        interactable.OnInteract += Interacted;
    }

    private void UnlinkEvents(){
        interactable.OnInteract -= Interacted;
    }
}
