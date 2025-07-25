using Godot;
using System;

public partial class Firepit : Node{
    
    [Export] Interactable interactable;
    [Export] private Texture2D litSprite;
    [Export] private Texture2D unlitSprite;
    [Export] private Sprite2D sprite;
    [Export] private bool ContainsEmbers = false;

    public override void _EnterTree(){
        base._EnterTree();
        LinkEvents();

        if(ContainsEmbers == false){
            UnlitState();
        }
        else{
            LitState();
        }
    }

    public override void _ExitTree(){
        base._ExitTree();
        UnlinkEvents();
    }

    private void Interacted(Interactor interactor){
        EmberStorage embers = interactor.GetParent().GetNode<EmberStorage>(EmberStorage.NodeName);
        if(embers != null){
            GD.Print(embers.EmptyNotches);
            if(embers.EmptyNotches > 0 && ContainsEmbers == true){
                embers.Add(EmberStorage.NotchMaxEmberValue);
                GD.Print("pass.");
                UnlitState();
            }
            else if(embers.EmptyNotches > 0 && ContainsEmbers == false){
                embers.Remove(EmberStorage.NotchMaxEmberValue);
                GD.Print("pass.");
                LitState();
            }
        }
    }

    private void LitState(){
        ContainsEmbers = true;
        sprite.Texture = litSprite;
    }

    private void UnlitState(){
        ContainsEmbers = false;
        sprite.Texture = unlitSprite;
    }

    private void LinkEvents(){
        interactable.OnInteract += Interacted; 
    }

    private void UnlinkEvents(){
        interactable.OnInteract -= Interacted;
    }
}
