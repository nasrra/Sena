using Godot;
using System;

public partial class InputActionLabel : Label{

	[Export] string inputAction;


	/// 
	/// Base.
	/// 


	public override void _EnterTree(){
		base._EnterTree();
		GetInputActionIcon();
		LinkEvents();
	}

	public override void _ExitTree(){
		base._ExitTree();
	}


	///
	/// Functions.
	/// 


	private void GetInputActionIcon(){
		Text = InputManager.Singleton.GetInputActionGlyphUnicode(inputAction);
	}


	/// 
	/// Linkage.
	/// 


	private void LinkEvents(){
		InputManager.Singleton.OnGamepadState   += GetInputActionIcon;
		InputManager.Singleton.OnKeyboardState  += GetInputActionIcon;
	}

	private void UnlinkEvents(){
		InputManager.Singleton.OnGamepadState   -= GetInputActionIcon;
		InputManager.Singleton.OnKeyboardState  -= GetInputActionIcon;
	}

}
