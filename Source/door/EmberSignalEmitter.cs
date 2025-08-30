using Godot;
using System;

public partial class EmberSignalEmitter : Area3D{
	public const string NodeName 		= nameof(EmberSignalEmitter);
	public event Action OnActivate    	= null;
	public event Action OnDeactivate    = null;

	public override void _EnterTree(){
		base._EnterTree();
		#if TOOLS
			Entropek.Util.Node.VerifyName(this, NodeName);
		#endif
	}
	
	public void Activate() => OnActivate?.Invoke();

	public void Deactivate() => OnDeactivate?.Invoke();

	public void Deactivate(out int embers){
		embers = EmberStorage.NotchMaxEmberValue; // return out a notch of embers.
		Deactivate();
	}
}
