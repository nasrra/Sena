using Godot;
using System;

public partial class CameraController : Camera2D{
    
    public static CameraController Instance {get;private set;}

    private RandomNumberGenerator rng;
    [Export] public VignetteShaderController Vignette {get;private set;}
    [Export] private ColorRect fadeTransition;
    [Export] private Timer fadeTimer;
    [Export] private Timer shakeTimer;
    [Export] public Node2D Target;
    
    private event Action fadeStatePhysicsProcess;

    private Vector2 shakeOffset = Vector2.Zero;
    [Export] public Vector2 FollowOffset = Vector2.Zero;
    
    private float shakeStrength = 0.0f;
    [Export] private float followSpeed = 0.88f;
    
    private bool shake = false;


    /// 
    /// Base.
    /// 


    public override void _Ready(){
        base._Ready();
        rng = new RandomNumberGenerator();

        // set alpha to full for one frame rule in SceneManager when loading new scene with transition.
        
    }

    public override void _EnterTree(){
        base._EnterTree();
        LinkEvents();
        Instance = this;
    }

    public override void _ExitTree(){
        base._ExitTree();
        UnlinkEvents();
        Instance = null;
    }

    public override void _Process(double delta){
        base._Process(delta);


        // call on late.

        CallDeferred("UpdateCamera", (float)delta);
    }

    public override void _PhysicsProcess(double delta){
        base._PhysicsProcess(delta);
        fadeStatePhysicsProcess?.Invoke();
        if(shake == true){
            shakeOffset = new Vector2(
                rng.RandfRange(-shakeStrength, shakeStrength),
                rng.RandfRange(-shakeStrength, shakeStrength)
            );
        }
    }


    /// 
    /// Functions.
    /// 


    private void UpdateCamera(float delta){
        if(IsInstanceValid(Target) && Target.IsInsideTree()){
            GlobalPosition = GlobalPosition.Lerp(Target.GlobalPosition + Offset + FollowOffset + shakeOffset, followSpeed * delta);
        }
    }

    public void StartShake(float strength){
        shakeStrength = strength;
        shake = true;
    }

    public void StartShake(float strength, float time){
        shakeTimer.WaitTime = time;
        shakeTimer.Start();
        StartShake(strength);
    }

    public void StopShake(){
        shake = false;
        shakeOffset = Vector2.Zero;
    }

    public void FadeToBlack(float time){
        fadeTimer.WaitTime = time;
        fadeTimer.Start();
        fadeStatePhysicsProcess = FadeToBlackPhysicsProcess; 
    }

    private void FadeToBlackPhysicsProcess(){
        fadeTransition.Color = new Color(fadeTransition.Color.R, fadeTransition.Color.G, fadeTransition.Color.B, (float)(1 - (fadeTimer.TimeLeft / fadeTimer.WaitTime)));
    }

    public void FadeFromBlack(float time){
        fadeTimer.WaitTime = time;
        fadeTimer.Start();
        fadeStatePhysicsProcess = FadeFromBlackPhysicsProcess; 
    }

    private void FadeFromBlackPhysicsProcess(){
        fadeTransition.Color = new Color(fadeTransition.Color.R, fadeTransition.Color.G, fadeTransition.Color.B, (float)(fadeTimer.TimeLeft / fadeTimer.WaitTime));
    }

    private void StopFadeTransition(){
        fadeStatePhysicsProcess = null;
        if(fadeTransition.Color.A > 0.5f){
            fadeTransition.Color = new Color(fadeTransition.Color.R, fadeTransition.Color.G, fadeTransition.Color.B, 1);
        }
        else{
            fadeTransition.Color = new Color(fadeTransition.Color.R, fadeTransition.Color.G, fadeTransition.Color.B, 0);
        }
    }

    public void SnapToTarget(){
        GlobalPosition = Target.GlobalPosition;
    }

    private void LevelEnterTransition(){
        FadeFromBlack(0.33f);
        SnapToTarget();
    }

    private void LevelExitTransition(){
        FadeToBlack(0.33f);
    }


    ///
    /// Linkage.
    /// 


    private void LinkEvents(){
        shakeTimer.Timeout += StopShake;
        fadeTimer.Timeout += StopFadeTransition;
        SceneManager.Instance.OnScene2DLoaded += LevelEnterTransition;
        SceneManager.Instance.OnScene2DDelayedLoadSet += LevelExitTransition;
    }

    private void UnlinkEvents(){
        shakeTimer.Timeout -= StopShake;
        fadeTimer.Timeout -= StopFadeTransition;
        SceneManager.Instance.OnScene2DLoaded -= LevelEnterTransition;
        SceneManager.Instance.OnScene2DDelayedLoadSet -= LevelExitTransition;
    }
}
