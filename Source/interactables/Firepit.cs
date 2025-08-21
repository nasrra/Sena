using Godot;
using System;

public partial class Firepit : Node{
	
	[Export] Interactable interactable;
	[Export] private Texture2D litSprite;
	[Export] private Texture2D unlitSprite;
	[Export] private Sprite2D sprite;
	[Export] private EmberHolder embers;

	public override void _EnterTree(){
		base._EnterTree();
		LinkEvents();
		// if(embers.IsLit == true){
			// LitState();
		// }
		// else{
			UnlitState();
		// }
	}

	public override void _ExitTree(){
		base._ExitTree();
		UnlinkEvents();
	}

	private void Interacted(Interactor interactor){
		throw new NotImplementedException();
		// EmberStorage interactorEmbers = interactor.GetParent().GetNode<EmberStorage>(EmberStorage.NodeName);
		// GD.Print(embers.IsLit );
		// if(interactorEmbers != null){
		// 	if(interactorEmbers.EmptyNotches > 0 && embers.IsLit == true){
		// 		embers.UnlitState(out int giveEmberValue);
		// 		interactorEmbers.Add(giveEmberValue);
		// 		UnlitState();
		// 	}
		// 	else if(interactorEmbers.NotchAmount > 0 && embers.IsLit  == false){
		// 		interactorEmbers.Remove(EmberStorage.NotchMaxEmberValue);
		// 		LitState();
		// 	}
		// }
	}

	private void LitState(){
		sprite.Texture = litSprite;
	}

	private void UnlitState(){
		sprite.Texture = unlitSprite;
	}

	private void LinkEvents(){
		interactable.OnInteract += Interacted;
		embers.OnLit      		+= LitState;
		embers.OnUnlit        	+= UnlitState;
	}

	private void UnlinkEvents(){
		interactable.OnInteract -= Interacted;
		embers.OnLit      		-= LitState;
		embers.OnUnlit        	-= UnlitState;
	}
}
