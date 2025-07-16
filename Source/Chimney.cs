using Godot;
using System;

public partial class Chimney : Node{
    [Export] private EmberStorage emberStorage;
    [Export] private Interactable interactable;
    [Export] private Timer rechargeTimer;
    private const int emberToGive       = 20;
    private const int emberToRecharge   = 20;

    public override void _EnterTree(){
        base._EnterTree();
        LinkEvents();
    }

    public override void _ExitTree(){
        base._ExitTree();
        UnlinkEvents();
    }


    private void Interacted(){
        int remainder = 0;
        int amount = emberToGive;
        emberStorage.Remove(amount, out remainder);
        amount -= remainder;
        Player.Instance.EmberStorage.Add(amount, out remainder);
        rechargeTimer.Start();
        interactable.IsInteractable = false;
        GD.Print("Interacted with chimney.");
    }

    private void Recharge(){
        emberStorage.Add(emberToRecharge, out int remainder);
        interactable.IsInteractable = true;
    }

    private void LinkEvents(){
        interactable.OnInteract += Interacted;
        rechargeTimer.Timeout += Recharge;
    }

    private void UnlinkEvents(){
        interactable.OnInteract -= Interacted;
        rechargeTimer.Timeout -= Recharge;
    }
}
