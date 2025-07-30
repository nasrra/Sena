using Godot;
using System;

public partial class BrazierLevelSwapDoor : LevelSwapDoor{
    [ExportGroup("BrazierLevelSwapDoor")]
    [Export] private Interactable hitInteractable;
    [Export] private Interactable interactable;
    [Export] private EmberHolder embers;


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
        if(interactorEmbers != null){
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
    
    /// 
    /// Linkage.
    /// 

    
    private void LinkEvents(){
        interactable.OnInteract     += Interacted; 
        hitInteractable.OnInteract  += HitInteracted;
        embers.OnLit                += Unlock;
        embers.OnLit                += interactable.DisableInteraction;
        embers.OnUnlit              += Lock;
        embers.OnUnlit              += interactable.EnableInteraction;
    }

    private void UnlinkEvents(){
        interactable.OnInteract     -= Interacted;
        hitInteractable.OnInteract  -= HitInteracted;
        embers.OnLit                -= Unlock;
        embers.OnLit                -= interactable.DisableInteraction;
        embers.OnUnlit              -= Lock;
        embers.OnUnlit              -= interactable.EnableInteraction;
    }

}
