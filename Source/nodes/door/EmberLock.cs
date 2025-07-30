using Godot;
using System;

public partial class EmberLock : Node{
	public const string NodeName = nameof(EmberLock);
	public event Action OnUnlock    = null;
	public event Action OnLock      = null;
	[Export] public bool locked              = false;

	public override void _EnterTree(){
		base._EnterTree();
		#if TOOLS
			Entropek.Util.Node.VerifyName(this, NodeName);
		#endif
	}
	
	public void Unlock(){
		locked = false;
		OnUnlock?.Invoke();
	}

	public void Lock(){
		locked = true;
		OnLock?.Invoke();
	}
}
