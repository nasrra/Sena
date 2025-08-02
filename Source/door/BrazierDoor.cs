using Godot;
using System;

public partial class BrazierDoor : Node{
    [Export] private Door door;
    [Export] private Sprite2D sprite;
    [Export] private Interactable hitInteractable;
    [Export] private Interactable interactable;
    [Export] private EmberHolder embers;
    [Export] private Texture2D litFire;
    [Export] private Texture2D unlitFire;
    [Export] private Sprite2D fireRight;
    [Export] private Sprite2D fireLeft;
    [Export] private Texture2D openedSprite;
    [Export] private Texture2D closedSprite;


    /// 
    /// Base. 
    /// 


    public override void _EnterTree(){
        base._EnterTree();
        if(door.Locked==false){
            LitState();
            embers.LitState();
        }
        else{
            embers.UnlitState();
            UnlitState();
        }
        if(door.Opened==true){
            HandleOpen();
        }
        else{
            HandleClose();
        }
        LinkEvents();
    }

    public override void _ExitTree(){
        base._ExitTree();
        UnlinkEvents();
    }

    private void HitInteracted(Interactor interactor){
        if(door.Locked==true){
            return;
        }
        if(door.Opened == false && embers.IsLit == true){
            door.Open();
        }
    }

    private void Interacted(Interactor interactor){
        EmberStorage interactorEmbers = interactor.GetParent().GetNode<EmberStorage>(EmberStorage.NodeName);
        if(interactorEmbers != null && interactorEmbers.NotchAmount >= 1){
            interactorEmbers.Remove(EmberStorage.NotchMaxEmberValue);
            embers.LitState();
        }
        else{
            door.Unlock();
        }
    }   

    private void HandleOpen(){
        sprite.Texture = openedSprite;
    }

    private void HandleClose(){
        sprite.Texture = closedSprite;
    }

    private void LitState(){
        interactable.DisableInteraction();
        fireLeft.Texture    = litFire;
        fireRight.Texture   = litFire;
    }

    private void UnlitState(){
        interactable.EnableInteraction();        
        fireLeft.Texture    = unlitFire;
        fireRight.Texture   = unlitFire;
    }

    
    /// 
    /// Linkage.
    /// 

    
    private void LinkEvents(){
        interactable.OnInteract     += Interacted; 
        hitInteractable.OnInteract  += HitInteracted;
        embers.OnLit                += LitState;
        embers.OnUnlit              += UnlitState;
        door.OnOpen                 += HandleOpen;
        door.OnClose                += HandleClose;
        door.OnLock                 += UnlitState;
        door.OnUnlock               += LitState;
    }

    private void UnlinkEvents(){
        interactable.OnInteract     -= Interacted;
        hitInteractable.OnInteract  -= HitInteracted;
        embers.OnLit                -= LitState;
        embers.OnUnlit              -= UnlitState;
        door.OnOpen                 -= HandleOpen;
        door.OnClose                -= HandleClose;
        door.OnLock                 -= UnlitState;
        door.OnUnlock               -= LitState;
    }

}
