using Godot;
using System;
using System.Collections.Generic;

public partial class InputManager : Node2D{
    public static InputManager Instance {get;private set;}

    /// 
    /// Variables.
    /// 

    private Vector2 moveInput = Vector2.Zero;
    private Vector2 aimInput = Vector2.Zero;

    /// 
    /// States.
    /// 

    private event Action MovementInputProcess;
    private event Action AimInputProcess;
    private event Action AttackInputProcess;
    private event Action InteractInputProcess;
    private event Action HealInputProcess;
    private event Action DashInputProcess;
    private event Action ShootInputProcess;

    /// 
    /// Callbacks.
    /// 

    public event Action<Vector2> OnMovementInput;
    public event Action<Vector2> OnAimInput;
    public event Action OnAttackInput;
    public event Action OnHealInput;
    public event Action OnInteractInput;
    public event Action OnDashInput;
    public event Action OnShootInput;

    public event Action OnGamepadState;
    public event Action OnKeyboardState;

    public bool IsGamepad {get; private set;} = false;
    public bool Paused {get; private set;} = false;


    /// 
    /// Base.
    /// 


    public override void _EnterTree(){
        base._EnterTree();
        Instance = this;
        CheckGamepad();
        LinkEvents();
    }

    public override void _ExitTree(){
        base._ExitTree();
        UnlinkEvents();
    }


    public override void _Process(double delta){
        
        if(Paused==true){
            return;
        }

        base._Process(delta);        
        InteractInputProcess();
        MovementInputProcess();
        AttackInputProcess();
        AimInputProcess();
        HealInputProcess();
        DashInputProcess();
        ShootInputProcess();
    }


    /// 
    /// States.
    /// 


    private void ClearState(){
        MovementInputProcess    = null;
        AimInputProcess         = null;
        AttackInputProcess      = null;
        HealInputProcess        = null;
        InteractInputProcess    = null;
        DashInputProcess        = null;
        ShootInputProcess       = null;
    }

    private void KeyboardState(){
        
        GD.Print("keyboard state");

        ClearState();
        
        IsGamepad = false;
        
        MovementInputProcess    = MovementInputKeyboard;
        AimInputProcess         = AimInputKeyboard;
        AttackInputProcess      = AttackInputKeyboard;
        HealInputProcess        = HealInputKeyboard;
        InteractInputProcess    = InteractInputKeyboard;
        DashInputProcess        = DashInputKeyboard;
        ShootInputProcess       = ShootInputKeyboard;
        
        OnKeyboardState?.Invoke();
    }

    private void GamepadState(){
        
        GD.Print("gamepad state");

        ClearState();
        
        IsGamepad = true;
        
        MovementInputProcess    = MovementInputGamepad;
        AimInputProcess         = AimInputGamepad;
        AttackInputProcess      = AttackInputGamepad;
        HealInputProcess        = HealInputGamepad;
        InteractInputProcess    = InteractInputGamepad;
        DashInputProcess        = DashInputGamepad;
        ShootInputProcess       = ShootInputGamepad;

        OnGamepadState?.Invoke();
    }

    public void PauseState(){
        Paused = true;
    }

    public void ResumeState(){
        Paused = false;
    }


    /// 
    /// Functions.
    /// 


    private void CheckGamepad(){
        Godot.Collections.Array<int> connectedJoypads = Input.GetConnectedJoypads();
        if(connectedJoypads.Count <= 0){
            KeyboardState();
        }
        else{
            GamepadState();
        }
    }

    private void MovementInputKeyboard(){
        Vector2 newMoveInput = Input.GetVector("MoveLeftKB", "MoveRightKB", "MoveUpKB", "MoveDownKB");
        if(moveInput.LengthSquared() > 0 || newMoveInput.LengthSquared() > 0){
            moveInput = newMoveInput;
            OnMovementInput?.Invoke(moveInput);
        }
    }

    private void MovementInputGamepad(){
        Vector2 newMoveInput = Input.GetVector("MoveLeftGP", "MoveRightGP", "MoveUpGP", "MoveDownGP");
        if(moveInput.LengthSquared() > 0 || newMoveInput.LengthSquared() > 0){
            moveInput = newMoveInput;
            OnMovementInput?.Invoke(moveInput);
        }
    }

    private void AimInputKeyboard(){
        Vector2 newAimInput = GetGlobalMousePosition();
        if(newAimInput != aimInput){
            aimInput = newAimInput;
            OnAimInput?.Invoke(aimInput);
        }
    }

    private void AimInputGamepad(){
        Vector2 newAimInput = Input.GetVector("AimLeftGP", "AimRightGP", "AimUpGP", "AimDownGP");
        if(newAimInput.LengthSquared() > 0){
            aimInput = newAimInput;
            OnAimInput?.Invoke(aimInput); 
        }
    }

    private void AttackInputKeyboard(){
        if(Input.IsActionJustPressed("AttackKB")){
            OnAttackInput?.Invoke();
        }
    }

    private void AttackInputGamepad(){
        if(Input.IsActionJustPressed("AttackGP")){
            OnAttackInput?.Invoke();
        }
    }

    private void HealInputKeyboard(){
        if(Input.IsActionJustPressed("HealKB")){
            OnHealInput?.Invoke();
        }
    }

    private void HealInputGamepad(){
        if(Input.IsActionJustPressed("HealGP")){
            OnHealInput?.Invoke();
        }
    }

    private void InteractInputKeyboard(){
        if(Input.IsActionJustPressed("InteractKB")){
            OnInteractInput?.Invoke();
        }
    }

    private void InteractInputGamepad(){
        if(Input.IsActionJustPressed("InteractGP")){
            OnInteractInput?.Invoke();
        }
    }

    private void DashInputKeyboard(){
        if(Input.IsActionJustPressed("DashKB")){
            OnDashInput?.Invoke();
        }
    }

    private void DashInputGamepad(){
        if(Input.IsActionJustPressed("DashGP")){
            OnDashInput?.Invoke();
        }
    }

    private void ShootInputKeyboard(){
        if(Input.IsActionJustPressed("ShootKB")){
            OnShootInput?.Invoke();
        }
    }

    private void ShootInputGamepad(){
        if(Input.IsActionJustPressed("ShootGP")){
            OnShootInput?.Invoke();
        }
    }


    /// 
    /// Linkage.
    /// 


    private void LinkEvents(){
        Input.Singleton.JoyConnectionChanged += OnJoyConnectionChanged;
    }

    private void UnlinkEvents(){
        Input.Singleton.JoyConnectionChanged -= OnJoyConnectionChanged;
    }


    /// 
    /// Linkage Functions.
    /// 


    private void OnJoyConnectionChanged(long deviceId, bool connected){
        if(connected){
            GamepadState();
        }
        else{
            KeyboardState();
        }
    }
}
