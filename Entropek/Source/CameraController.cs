using Godot;
using System;

public partial class CameraController : Camera2D{
    
    private RandomNumberGenerator rng;
    [Export] public VignetteShaderController Vignette {get;private set;}
    [Export] private Timer shakeTimer;
    [Export] public Node2D Target;
    
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
    }

    public override void _EnterTree(){
        base._EnterTree();
        LinkEvents();
    }

    public override void _ExitTree(){
        base._ExitTree();
        UnlinkEvents();
    }


    public override void _Process(double delta){
        base._Process(delta);

        // call on late.

        CallDeferred("UpdateCamera", (float)delta);
    }

    public override void _PhysicsProcess(double delta){
        base._PhysicsProcess(delta);
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
        GlobalPosition = GlobalPosition.Lerp(Target.GlobalPosition + Offset + FollowOffset + shakeOffset, followSpeed * delta);
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


    ///
    /// Linkage.
    /// 


    private void LinkEvents(){
        shakeTimer.Timeout += StopShake;
    }

    private void UnlinkEvents(){
        shakeTimer.Timeout -= StopShake;
    }
}
