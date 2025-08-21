using Godot;
using System;

public partial class BrazierDoor : Node{
    [Export] private Door door;
    [Export] private Sprite3D doorSprite;
    [Export] private Sprite3D flameSprite;
    [Export] private Light3D flameLight;
    [Export] private Interactable hitInteractable;
    [Export] private Interactable interactable;
    [Export] private EmberHolder embers;
    [Export] private Texture2D openSprite;
    [Export] private Texture2D closedSprite;


    /// 
    /// Base. 
    /// 


    public override void _EnterTree(){
        base._EnterTree();
        // if(door.IsLocked==false){
        //     LitState();
        //     embers.LitState();
        // }
        // else{
        //     embers.UnlitState();
        //     UnlitState();
        // }
        // if(door.IsOpened==true){
        //     OnOpenCallback();
        // }
        // else{
        //     OnCloseCallback();
        // }
        LinkEvents();
    }

    public override void _ExitTree(){
        base._ExitTree();
        UnlinkEvents();
    }

    private void HitInteracted(Interactor interactor){
        if(door.IsOpened == false && door.IsLocked == false){
            door.Open();
        }
    }

    private void Interacted(Interactor interactor){
        EmberStorage interactorEmbers = interactor.GetParent().GetNode<EmberStorage>(EmberStorage.NodeName);
        if(interactorEmbers != null){
            if(interactorEmbers.NotchAmount >= 1){
                interactorEmbers.Remove(EmberStorage.NotchMaxEmberValue);
                embers.LitState();
            }
        }
        else{
            door.Unlock();
        }
    }   

    private void OpenedState(){
        doorSprite.Texture = openSprite;
    }

    private void ClosedState(){
        doorSprite.Texture = closedSprite;
    }

    private void UnlockedState(){
        flameSprite.Visible = true;
        flameLight.Visible = true;
        interactable.DisableInteraction();
    }

    private void LockedState(){
        flameSprite.Visible = false;
        flameLight.Visible = false;
        interactable.EnableInteraction();
    }

    
    /// 
    /// Linkage.
    /// 

    
    private void LinkEvents(){
        interactable.OnInteract         += Interacted; 
        hitInteractable.OnInteract      += HitInteracted;
        embers.OnLit                    += door.Unlock;
        embers.OnUnlit                  += door.Lock;
        door.OnOpened                   += OpenedState;
        door.OnClosed                   += ClosedState;
        door.OnLocked                   += LockedState;
        door.OnUnlocked                 += UnlockedState;
    }

    private void UnlinkEvents(){
        interactable.OnInteract         -= Interacted; 
        hitInteractable.OnInteract      -= HitInteracted;
        embers.OnLit                    -= door.Unlock;
        embers.OnUnlit                  -= door.Lock;
        door.OnOpened                   -= OpenedState;
        door.OnClosed                   -= ClosedState;
        door.OnLocked                   -= LockedState;
        door.OnUnlocked                 -= UnlockedState;
    }

}
