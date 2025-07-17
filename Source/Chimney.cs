using Godot;
using System;

public partial class Chimney : Node{
    [Export] private Interactable interactable;

    public override void _EnterTree(){
        base._EnterTree();
        LinkEvents();
    }

    public override void _ExitTree(){
        base._ExitTree();
        UnlinkEvents();
    }


    private void Interacted(){
        bool playerHasEmbers = Player.Instance.EmberStorage.Value > 0;
        if(playerHasEmbers==false){
            Player.Instance.EmberStorage.Add(Player.Instance.EmberStorage.Max, out int remainder);
            GD.Print("Interacted with chimney.");
        }
    }

    private void LinkEvents(){
        interactable.OnInteract += Interacted;
    }

    private void UnlinkEvents(){
        interactable.OnInteract -= Interacted;
    }
}
