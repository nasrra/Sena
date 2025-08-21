using Godot;
using System;

public partial class EmberHolder : Node{
	public const string NodeName 		= nameof(EmberHolder);
	public event Action OnLit    		= null;
	public event Action OnUnlit      	= null;

	public override void _EnterTree(){
		base._EnterTree();
		#if TOOLS
			Entropek.Util.Node.VerifyName(this, NodeName);
		#endif
	}
	
	public void LitState(){
		OnLit?.Invoke();
	}

	public void UnlitState(){
		OnUnlit?.Invoke();
	}

	public void UnlitState(out int embers){
		embers = EmberStorage.NotchMaxEmberValue; // return out a notch of embers.
		UnlitState();
	}
}
