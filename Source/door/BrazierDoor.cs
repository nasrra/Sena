using Godot;
using System;

public partial class BrazierDoor : Node{
    [Export] private Door door;
    [Export] private Sprite2D sprite;
    [Export] private Interactable hitInteractable;
    [Export] private Interactable interactable;
    [Export] private EmberHolder embers;
    [Export] private Texture2D litOpenSprite;
    [Export] private Texture2D litClosedSprite;
    [Export] private Texture2D unlitOpenSprite;
    [Export] private Texture2D unlitClosedSprite;


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
            OnOpenCallback();
        }
        else{
            OnCloseCallback();
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

    private void OnOpenCallback(){
        sprite.Texture = door.Locked == true? unlitOpenSprite : litOpenSprite;
    }

    private void OnCloseCallback(){
        sprite.Texture = door.Locked == true? unlitClosedSprite : litClosedSprite;
    }

    private void LitState(){
        sprite.Texture = door.Opened == true? litOpenSprite : litClosedSprite;
        interactable.DisableInteraction();
    }

    private void UnlitState(){
        sprite.Texture = door.Opened == true? unlitOpenSprite : unlitClosedSprite;
        interactable.EnableInteraction();
    }

    
    /// 
    /// Linkage.
    /// 

    
    private void LinkEvents(){
        interactable.OnInteract     += Interacted; 
        hitInteractable.OnInteract  += HitInteracted;
        embers.OnLit                += LitState;
        embers.OnUnlit              += UnlitState;
        door.OnOpen                 += OnOpenCallback;
        door.OnClose                += OnCloseCallback;
        door.OnLock                 += UnlitState;
        door.OnUnlock               += LitState;
    }

    private void UnlinkEvents(){
        interactable.OnInteract     -= Interacted;
        hitInteractable.OnInteract  -= HitInteracted;
        embers.OnLit                -= LitState;
        embers.OnUnlit              -= UnlitState;
        door.OnLock                 -= UnlitState;
        door.OnUnlock               -= LitState;
    }

}
