using Godot;
using System;

public partial class Firepit : Node{
    
    [Export] Interactable interactable;
    [Export] private Texture2D litSprite;
    [Export] private Texture2D unlitSprite;
    [Export] private Sprite2D sprite;
    [Export] private EmberStorage emberStorage;

    public override void _EnterTree(){
        base._EnterTree();
        LinkEvents();
        if(emberStorage.NotchAmount >= 1){
            LitState();
        }
        else{
            UnlitState();
        }
    }

    public override void _ExitTree(){
        base._ExitTree();
        UnlinkEvents();
    }

    private void Interacted(Interactor interactor){
        EmberStorage embers = interactor.GetParent().GetNode<EmberStorage>(EmberStorage.NodeName);
        if(embers != null){
            if(embers.EmptyNotches > 0 && emberStorage.NotchAmount >= 1){
                embers.Add(EmberStorage.NotchMaxEmberValue);
                UnlitState();
            }
            else if(embers.NotchAmount > 0 && emberStorage.NotchAmount >= 1){
                embers.Remove(EmberStorage.NotchMaxEmberValue);
                LitState();
            }
        }
    }

    private void LitState(){
        sprite.Texture = litSprite;
    }

    private void UnlitState(){
        sprite.Texture = unlitSprite;
    }

    private void HandleOnNotchesUpdated(int notchAmount, int remainderEmbers){
        if(notchAmount >= 1){
            LitState();
        }
        else{
            UnlitState();
        }
    }

    private void LinkEvents(){
        interactable.OnInteract += Interacted;
        emberStorage.OnNotchesUpdated +=  HandleOnNotchesUpdated;
    }

    private void UnlinkEvents(){
        interactable.OnInteract -= Interacted;
        emberStorage.OnNotchesUpdated -= HandleOnNotchesUpdated;
    }
}
