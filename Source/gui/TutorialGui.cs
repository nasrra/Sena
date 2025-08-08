using Godot;
using System;

public partial class TutorialGui : Control{
    [Export] AnimationPlayer textAnimator;
    [Export] AnimationPlayer backgroundAnimator;
    [Export] Timer evaluateStateTimer;
    private const float swapStateTimeBuffer = 0.1f;
    private string currentTutorial = "";
    private State previousState = State.Disabled;
    private State currentState = State.Disabled;

    private enum State : byte{
        Enabled,
        Transition,
        Disabled,
    }


    ///
    /// Base.
    /// 


    public override void _EnterTree(){
        base._EnterTree();
        LinkEvents();
    }

    public override void _ExitTree(){
        base._ExitTree();
        UnlinkEvents();
    }

    public override void _Input(InputEvent @event){
        base._Input(@event);
        if(@event.IsPressed() && currentState == State.Enabled){
            GD.Print(1);
            DisableCurrentState();
        }
    }


    /// 
    /// State Machine.
    /// 


    private void EnableState(string tutorialText){
        SwapStateTo(State.Transition);
        currentTutorial = tutorialText;
        textAnimator.Play("Enable"+tutorialText);
        backgroundAnimator.Play("Enable");
        GameManager.Singleton.TutorialState();
        evaluateStateTimer.Start(backgroundAnimator.CurrentAnimationLength+swapStateTimeBuffer);
    }

    private void DisableCurrentState(){
        DisableState(currentTutorial);
    }

    private void DisableState(string tutorialText){
        SwapStateTo(State.Transition);
        currentTutorial = tutorialText;
        textAnimator.Play("Disable"+tutorialText);
        backgroundAnimator.Play("Disable");
        evaluateStateTimer.Start(backgroundAnimator.CurrentAnimationLength+swapStateTimeBuffer);
    }

    private void SwapStateTo(State state){
        previousState = currentState;
        currentState = state;
    }


    ///
    /// Tutorial Text.  
    /// 


    public void EnableHealTutorial(){
        EnableState("Heal");
    }

    public void DisableHealTutorial(){
        DisableState("Heal");
    }

    public void EnableAttackTutorial(){
        EnableState("Attack");
    }

    public void DisableAttackTutorial(){
        DisableState("Attack");
    }


    /// 
    /// Linkage.
    /// 


    private void LinkEvents(){
        evaluateStateTimer.Timeout += EvaluateState;
    }

    private void UnlinkEvents(){
        evaluateStateTimer.Timeout -= EvaluateState;
    }

    private void EvaluateState(){
        switch(previousState){
            case State.Disabled:
                SwapStateTo(State.Enabled);
            break;
            case State.Enabled:
                SwapStateTo(State.Disabled);
                GameManager.Singleton.GameplayState();
            break;
        }
    }
}