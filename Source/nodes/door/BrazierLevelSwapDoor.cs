using Godot;
using System;

public partial class BrazierLevelSwapDoor : LevelSwapDoor{
    [ExportGroup("BrazierLevelSwapDoor")]
    [Export] private Interactable hitInteractable;
    [Export] private Interactable interactable;


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
        EmberStorage embers = interactor.GetParent().GetNode<EmberStorage>(EmberStorage.NodeName);
        if(embers != null){
            Unlock(embers);
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


    /// <summary>
    /// Unlocks the door via removing embers from an ember storage.
    /// </summary>
    /// <param name="emberStorage"></param>

    public void Unlock(EmberStorage emberStorage){
        if(Locked == true && emberStorage.NotchAmount >= 1){
            emberStorage.Remove(EmberStorage.NotchMaxEmberValue);
            Unlock();
        }
    }

    
    /// 
    /// Linkage.
    /// 

    
    private void LinkEvents(){
        interactable.OnInteract += Interacted; 
        hitInteractable.OnInteract += HitInteracted;
    }

    private void UnlinkEvents(){
        interactable.OnInteract -= Interacted;
        hitInteractable.OnInteract -= HitInteracted;
    }

}
