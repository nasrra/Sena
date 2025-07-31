using Godot;
using System;

public partial class InputIconGui : Label{
    [Export] string inputAction;
    [Export] bool isGamepad = false;

    public override void _EnterTree(){
        base._EnterTree();
        Refresh();
    }

    public override void _ExitTree(){
        base._ExitTree();
    }

    private void Refresh(){
        if(inputAction == null){
            return;
        }
        Text = InputManager.Singleton.GetInputActionGlyphUnicode(inputAction, isGamepad);
    }

    public void LinkEvents(){
        InputManager.Singleton.OnGamepadState += Refresh;
        InputManager.Singleton.OnKeyboardState += Refresh;
    }

    public void UnlinkEvents(){
        InputManager.Singleton.OnGamepadState += Refresh;
        InputManager.Singleton.OnKeyboardState += Refresh;
    }
}
