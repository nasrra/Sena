using Godot;
using System;

public partial class BrazierLevelSwapDoor : LevelSwapDoor{
    
    [ExportGroup(nameof(BrazierLevelSwapDoor))]
    [Export] private Sprite3D doorSprite;
    [Export] private Sprite3D flameSprite;
    [Export] private Light3D flameLight;
    [Export] private Interactable hitInteractable;
    [Export] private Interactable interactable;
    [Export] private EmberSignalEmitter emberSignals;
    [Export] private Texture2D openSprite;
    [Export] private Texture2D closedSprite;


    /// 
    /// Base. 
    /// 


    public override void _EnterTree(){
        LinkEvents();
        base._EnterTree();
        if(IsLocked==false){
            Unlocked();
        }
        else{
            Locked();
        }
    }

    public override void _ExitTree(){
        base._ExitTree();
        UnlinkEvents();
    }

    private void HitInteracted(Interactor interactor){
        if(IsOpened == false && IsLocked == false){
            Open();
        }
    }

    private void Interacted(Interactor interactor){
        EmberStorage interactorEmbers = interactor.GetParent().GetNode<EmberStorage>(EmberStorage.NodeName);
        if(interactorEmbers != null){
            if(interactorEmbers.NotchAmount >= 1){
                interactorEmbers.Remove(EmberStorage.NotchMaxEmberValue);
            }
        }
        Unlocked();
    }   

    protected override void Opened(){
        base.Opened();
        doorSprite.Texture = openSprite;
    }

    protected override void Closed(){
        doorSprite.Texture = closedSprite;
    }

    protected override void Unlocked(){
        flameSprite.Visible = true;
        flameLight.Visible = true;
        interactable.DisableInteraction();
    }

    protected override void Locked(){
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
        emberSignals.OnActivate         += Unlocked;
        emberSignals.OnDeactivate       += Locked;
    }

    private void UnlinkEvents(){
        interactable.OnInteract         -= Interacted; 
        hitInteractable.OnInteract      -= HitInteracted;
        emberSignals.OnActivate         -= Unlocked;
        emberSignals.OnDeactivate       -= Locked;
    }

    protected override void OpenFailed(){

    }

    public override void Open(){
        if(TryOpen()==false){
            return;
        }
        Opened();
    }

    public override void Close(){
        Closed();
    }

    public override void Lock(){
        Locked();
    }

    public override void Unlock(){
        Unlocked();
    }
}
