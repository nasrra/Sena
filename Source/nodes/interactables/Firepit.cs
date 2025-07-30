using Godot;
using System;

public partial class Firepit : Node{
	
	[Export] Interactable interactable;
	[Export] private Texture2D litSprite;
	[Export] private Texture2D unlitSprite;
	[Export] private Sprite2D sprite;
	[Export] private EmberLock emberLock;

	public override void _EnterTree(){
		base._EnterTree();
		LinkEvents();
		if(emberLock.locked == false){
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
			if(embers.EmptyNotches > 0 && emberLock.locked == false){
				embers.Add(EmberStorage.NotchMaxEmberValue);
				UnlitState();
			}
			else if(embers.NotchAmount > 0 && emberLock.locked == true){
				embers.Remove(EmberStorage.NotchMaxEmberValue);
				LitState();
			}
		}
	}

	private void LitState(){
		sprite.Texture = litSprite;
		emberLock.locked = false;
	}

	private void UnlitState(){
		sprite.Texture = unlitSprite;
		emberLock.locked = true;
	}

	private void LinkEvents(){
		interactable.OnInteract += Interacted;
		emberLock.OnUnlock      += LitState;
		emberLock.OnLock        += UnlitState;
	}

	private void UnlinkEvents(){
		interactable.OnInteract -= Interacted;
		emberLock.OnUnlock      -= LitState;
		emberLock.OnLock        -= UnlitState;
	}
}
