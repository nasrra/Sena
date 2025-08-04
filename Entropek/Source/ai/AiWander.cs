using Godot;
using System;

public partial class AiWander : Node2D{
    public const string NodeName = nameof(AiWander);

    [Export] private Timer timer;
    public event Action<Vector2> OnDirectionChosen;
    private Vector2 origin;
    private double previousPathTime;
    [Export] private double minPathTime;
    [Export] private double maxPathTime;
    [Export] private double minIdleTime;
    [Export] private double maxIdleTime;
    [Export] private Vector2 maxDirection = new Vector2(1,1);
    [Export] private Vector2 minDirection = new Vector2(-1,-1);
    private State previousState = State.Idle;
    private State currentState = State.Return;

    private enum State : byte{
        Return,
        Wander,
        Idle,
    }

    public void Initialise(
        double minPathTime,
        double maxPathTime,
        double minIdleTime,
        double maxIdleTime,
        Vector2 maxDirection,
        Vector2 minDirection
    ){

        this.minPathTime = minPathTime;
        this.maxPathTime = maxPathTime;
        this.minIdleTime = minIdleTime;
        this.maxIdleTime = maxIdleTime;
        this.maxDirection = maxDirection;
        this.minDirection = minDirection;
        EvaluateState();
    }

    public override void _EnterTree(){
        base._EnterTree();
        #if TOOLS
        Entropek.Util.Node.VerifyName(this, NodeName);
        #endif
        LinkEvents();
    }

    public override void _Ready(){
        base._Ready();
    }

    public override void _ExitTree(){
        base._ExitTree();
        UnlinkEvents();
    }


    public void SetOrigin(Vector2 Position){
        origin = Position;
    }

    public void EvaluateState(){
        if(currentState == State.Idle){
            if(previousState == State.Wander){
                ReturnState();
            }
            else{
                WanderState();
            }
        }
        else{
            IdleState();
        }
    }

    private void ReturnState(){
        previousState = currentState;
        currentState = State.Return;
        timer.Start(previousPathTime);
        OnDirectionChosen?.Invoke((origin - GlobalPosition).Normalized());
    }

    private void WanderState(){
        previousState = currentState;
        currentState = State.Wander;
        timer.Start(GD.RandRange(minPathTime, maxPathTime));
        previousPathTime = timer.TimeLeft;
        OnDirectionChosen?.Invoke(new Vector2(
            (float)GD.RandRange((double)minDirection.X, (double)maxDirection.X), 
            (float)GD.RandRange((double)minDirection.X, (double)maxDirection.X)));
    }

    private void IdleState(){
        previousState = currentState;
        currentState = State.Idle;
        timer.Start(GD.RandRange(minIdleTime, maxIdleTime));
        OnDirectionChosen?.Invoke(Vector2.Zero);
    }

    public void PauseState(){
        timer.Paused = true;
    }

    public void ResumeState(){
        timer.Paused = false;
    }

    private void LinkEvents(){
        timer.Timeout += EvaluateState;
    }

    private void UnlinkEvents(){
        timer.Timeout -= EvaluateState;
    }
}
