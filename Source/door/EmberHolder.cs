using Godot;
using System;

public partial class EmberHolder : Node{
	public const string NodeName 		= nameof(EmberHolder);
	public event Action OnLit    		= null;
	public event Action OnUnlit      	= null;
	[Export] public bool IsLit = false;

	public override void _EnterTree(){
		base._EnterTree();
		#if TOOLS
			Entropek.Util.Node.VerifyName(this, NodeName);
		#endif

		if(IsLit == true){
			LitState();
		}
		else{
			UnlitState();
		}
	}
	
	public void LitState(){
		IsLit = true;
		OnLit?.Invoke();
	}

	public void UnlitState(){
		IsLit = false;
		OnUnlit?.Invoke();
	}

	public void UnlitState(out int embers){
		embers = EmberStorage.NotchMaxEmberValue; // return out a notch of embers.
		UnlitState();
	}
}
