using Godot;
using System;

public partial class BrazierDoor : Node2D{
    [Export] 
    private Door door;
    [Export]
    private Sprite2D sprite;
    [Export]
    private Interactable interactable;

    public override void _EnterTree(){
        base._EnterTree();
        LinkEvents();
    }

    public override void _ExitTree(){
        base._ExitTree();
        UnlinkEvents();
    }

    private void LinkEvents(){
        interactable.OnInteract += Interacted;
    }

    private void UnlinkEvents(){
        interactable.OnInteract -= Interacted;
    }

    private void Interacted(){
        if(door.Opened == false){
            door.Open();
            sprite.Visible=false;
        }
        else{
            door.Close();
            sprite.Visible=true;
        }
    }
}
