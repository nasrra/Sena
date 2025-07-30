using Godot;
using System;

public partial class BrazierLevelSwapDoor : LevelSwapDoor{
    [ExportGroup("BrazierLevelSwapDoor")]
    [Export] private Interactable hitInteractable;
    [Export] private Interactable interactable;
    [Export] private EmberHolder embers;
    [Export] private Texture2D litFire;
    [Export] private Texture2D unlitFire;
    [Export] private Sprite2D fireRight;
    [Export] private Sprite2D fireLeft;


    /// 
    /// Base. 
    /// 


    public override void _EnterTree(){
        base._EnterTree();
        LinkEvents();
    }

    public override void _ExitTree(){
        base._ExitTree();
        UnlinkEvents();
    }

    private void HitInteracted(Interactor interactor){
        if(Locked==true){
            return;
        }
        if(Opened == false){
            Open();
            }
        else{
            Close();
        }
    }

    private void Interacted(Interactor interactor){
        EmberStorage interactorEmbers = interactor.GetParent().GetNode<EmberStorage>(EmberStorage.NodeName);
        if(interactorEmbers != null && interactorEmbers.NotchAmount >= 1){
            interactorEmbers.Remove(EmberStorage.NotchMaxEmberValue);
            embers.LitState();
        }
        else{
            Unlock();
        }
    }   

    public override void Open(){
        sprite.Texture = openedSprite;
        base.Open();
    }

    public override void Close(){
        sprite.Texture = closedSprite;
        base.Close();
    }

    private void LitState(){
        Unlock();
        interactable.DisableInteraction();
        fireLeft.Texture    = litFire;
        fireRight.Texture   = litFire;
    }

    private void UnlitState(){
        Lock();
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
    }

    private void UnlinkEvents(){
        interactable.OnInteract     -= Interacted;
        hitInteractable.OnInteract  -= HitInteracted;
        embers.OnLit                -= LitState;
        embers.OnUnlit              -= UnlitState;
    }

}
