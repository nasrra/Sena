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

    private void Interacted(){
        bool playerHasEmbers = Player.Instance.EmberStorage.Value > 0;
        if(playerHasEmbers==true){
            Player.Instance.EmberStorage.Remove(Player.Instance.EmberStorage.Max, out int remainder);
            LitState();
        }
        else{
            Player.Instance.EmberStorage.Add(Player.Instance.EmberStorage.Max, out int remainder);
            UnlitState();
        }
    }

    private void LitState(){
        ContainsEmbers = true;
        sprite.Texture = litSprite;
        GD.Print("lit state");
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
