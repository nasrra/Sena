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

    private void OnOpenCallback(){
        doorSprite.Texture = openSprite;
    }

    private void OnCloseCallback(){
        doorSprite.Texture = closedSprite;
    }

    private void LitState(){
        flameSprite.Visible = true;
        flameLight.Visible = true;
        interactable.DisableInteraction();
    }

    private void UnlitState(){
        flameSprite.Visible = false;
        flameLight.Visible = false;
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
