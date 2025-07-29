using Godot;
using System;
using System.Collections.Generic;

public partial class InputManager : Node2D{
    public static InputManager Singleton {get;private set;}

    private InputGlyphs inputGlyphs = new InputGlyphs();
    private Dictionary<string, string> inputActionGlyphUnicodes = new Dictionary<string, string>();

    /// 
    /// Variables.
    /// 

    private Vector2 moveInput = Vector2.Zero;
    private Vector2 aimInput = Vector2.Zero;

    ///
    /// Timers.
    /// 


    [Export] private Timer blockAimInputTimer;
    [Export] private Timer blockAttackInputTimer;
    [Export] private Timer blockDashInputTimer;
    [Export] private Timer blockHealInputTimer;
    [Export] private Timer blockInteractInputTimer;
    [Export] private Timer blockMovementInputTimer;
    [Export] private Timer blockShootInputTimer;


    /// 
    /// States.
    /// 

    private event Action AimInputProcess;
    private event Action AttackInputProcess;
    private event Action DashInputProcess;
    private event Action HealInputProcess;
    private event Action InteractInputProcess;
    private event Action MovementInputProcess;
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

    public bool IsGamepadConnected {get; private set;} = false;
    public bool Paused {get; private set;} = false;


    /// 
    /// Base.
    /// 


    public override void _EnterTree(){
        base._EnterTree();
        Singleton = this;
        CheckGamepad();
        InitialiseInputActionGlyphUnicodes();
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
        InteractInputProcess?.Invoke();
        MovementInputProcess?.Invoke();
        AttackInputProcess?.Invoke();
        AimInputProcess?.Invoke();
        HealInputProcess?.Invoke();
        DashInputProcess?.Invoke();
        ShootInputProcess?.Invoke();
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
        
        IsGamepadConnected = false;
        
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
        
        IsGamepadConnected = true;
        
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
        Paused                          = true;
        blockAimInputTimer.Paused       = true;
        blockAttackInputTimer.Paused    = true;
        blockDashInputTimer.Paused      = true;
        blockHealInputTimer.Paused      = true;
        blockInteractInputTimer.Paused  = true;
        blockMovementInputTimer.Paused  = true;
        blockShootInputTimer.Paused     = true;
    }

    public void ResumeState(){
        Paused                          = false;
        blockAimInputTimer.Paused       = false;
        blockAttackInputTimer.Paused    = false;
        blockDashInputTimer.Paused      = false;
        blockHealInputTimer.Paused      = false;
        blockInteractInputTimer.Paused  = false;
        blockMovementInputTimer.Paused  = false;
        blockShootInputTimer.Paused     = false;
    }


    /// 
    /// Input Checks.
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
    /// Blockers.
    /// 


    public void BlockMovementInput(){
        MovementInputProcess = null;
    }

    public void BlockMovementInput(float time){
        BlockMovementInput();
        blockMovementInputTimer.Start(time);
    }

    public void UnblockMovementInput(){
        MovementInputProcess = IsGamepadConnected == false? MovementInputKeyboard : MovementInputGamepad;
    }

    public void BlockAttackInput(){
        AttackInputProcess = null;
    }

    public void BlockAttackInput(float time){
        BlockAttackInput();
        blockAttackInputTimer.Start(time);
    }

    public void UnblockAttackInput(){
        AttackInputProcess = IsGamepadConnected == false ? AttackInputKeyboard : AttackInputGamepad;  
    }

    public void BlockInteractInput(){
        InteractInputProcess = null;
    }

    public void BlockInteractInput(float time){
        BlockInteractInput();
        blockInteractInputTimer.Start(time);
    }

    public void UnblockInteractInput(){
        InteractInputProcess = IsGamepadConnected == false ? InteractInputKeyboard : InteractInputGamepad;
    }

    public void BlockDashInput(){
        DashInputProcess = null;
    }

    public void BlockDashInput(float time){
        BlockDashInput();
        blockDashInputTimer.Start(time);
    }

    public void UnblockDashInput(){
        DashInputProcess = IsGamepadConnected == false ? DashInputKeyboard : DashInputGamepad;
    }

    public void BlockHealInput(){
        HealInputProcess = null;
    }

    public void BlockHealInput(float time){
        BlockHealInput();
        blockHealInputTimer.Start(time);
    }

    public void UnblockHealInput(){
        HealInputProcess = IsGamepadConnected == false ? HealInputKeyboard : HealInputGamepad;
    }

    public void BlockShootInput(){
        ShootInputProcess = null;
    }

    public void BlockShootInput(float time){
        BlockShootInput();
        blockShootInputTimer.Start(time);
    }

    public void UnblockShootInput(){
        ShootInputProcess = IsGamepadConnected == false ? ShootInputKeyboard : ShootInputGamepad;
    }

    public void BlockAimInput(){
        AimInputProcess = null;
    }

    public void BlockAimInput(float time){
        BlockAimInput();
        blockAimInputTimer.Start(time);
    }

    public void UnblockAimInput(){
        AimInputProcess = IsGamepadConnected == false ? AimInputKeyboard : AimInputGamepad;
    }


    /// 
    /// Input Glyphs.
    /// 

    private void InitialiseInputActionGlyphUnicodes(){
        inputActionGlyphUnicodes.Clear();

        // un-rebindable actions.

        inputActionGlyphUnicodes.Add("AimKB", inputGlyphs.SpecialGlyphs["MouseAny"]);
        inputActionGlyphUnicodes.Add("AimGP", inputGlyphs.SpecialGlyphs["RightJoyStick"]);
        inputActionGlyphUnicodes.Add("MoveGP", inputGlyphs.SpecialGlyphs["LeftJoyStick"]);

        Godot.Collections.Array<StringName> inputActions = InputMap.GetActions();
        for(int i =0; i < inputActions.Count; i++){
            if(RetrieveInputActionGlyphUnicode(inputActions[i], out string glyphUnicode) == true){
                inputActionGlyphUnicodes.Add(inputActions[i], glyphUnicode);
            }
        }
    }

    private bool RetrieveInputActionGlyphUnicode(string inputAction, out string glyphUnicode){
        glyphUnicode="";

        Godot.Collections.Array<InputEvent> events =  InputMap.ActionGetEvents(inputAction);
        if(events.Count<=0){
            // throw new Exception($"No input events for {inputAction}!");
            return false;
        }

        InputEvent primaryEvent = events[0];
        
        if(inputAction.EndsWith("KB")){
            if(primaryEvent is InputEventKey keyEvent){
                glyphUnicode = inputGlyphs.KeyboardUnicode[keyEvent.PhysicalKeycode];
                return true;
            }
            else if(primaryEvent is InputEventMouseButton mouseButtonEvent){
                glyphUnicode = inputGlyphs.MouseButtonUnicode[mouseButtonEvent.ButtonIndex];
                return true;
            }
        }
        
        else if(inputAction.EndsWith("GP")){
            if(primaryEvent is InputEventJoypadButton buttonEvent){
                glyphUnicode = inputGlyphs.JoyButtonUnicode[buttonEvent.ButtonIndex];
                return true;
            }
            else if(primaryEvent is InputEventJoypadMotion motionEvent){
                glyphUnicode = inputGlyphs.JoyAxisUnicode[motionEvent.Axis];
                return true;
            }
        }

        return false;
    }

    public string GetInputActionGlyphUnicode(string inputActionName){ // <-- without GP or KB suffix.
        string fullName = inputActionName + (IsGamepadConnected == true? "GP" : "KB");
        GD.Print(fullName);
        return inputActionGlyphUnicodes[fullName];
    }


    /// 
    /// Linkage.
    /// 


    private void LinkEvents(){
        Input.Singleton.JoyConnectionChanged    += OnJoyConnectionChanged;
        blockAimInputTimer.Timeout              += UnblockAimInput;
        blockAttackInputTimer.Timeout           += UnblockAttackInput;
        blockDashInputTimer.Timeout             += UnblockDashInput;
        blockHealInputTimer.Timeout             += UnblockHealInput;
        blockInteractInputTimer.Timeout         += UnblockInteractInput;
        blockMovementInputTimer.Timeout         += UnblockMovementInput;
        blockShootInputTimer.Timeout            += UnblockShootInput;
        
    }

    private void UnlinkEvents(){
        Input.Singleton.JoyConnectionChanged    -= OnJoyConnectionChanged;
        blockAimInputTimer.Timeout              -= UnblockAimInput;
        blockAttackInputTimer.Timeout           -= UnblockAttackInput;
        blockDashInputTimer.Timeout             -= UnblockDashInput;
        blockHealInputTimer.Timeout             -= UnblockHealInput;
        blockInteractInputTimer.Timeout         -= UnblockInteractInput;
        blockMovementInputTimer.Timeout         -= UnblockMovementInput;
        blockShootInputTimer.Timeout            -= UnblockShootInput;
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
