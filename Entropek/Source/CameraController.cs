using Godot;
using System;

public partial class CameraController : Camera3D{
	
	public static CameraController Instance {get;private set;}

	private RandomNumberGenerator rng;
	[Export] public VignetteShaderController Vignette {get;private set;}
	[Export] private ColorRect fadeTransition;
	[Export] private Timer fadeTimer;
	[Export] private Timer shakeTimer;
	[Export] public Node3D Target;
	
	private event Action fadeStatePhysicsProcess;

	private Vector3 shakeOffset = Vector3.Zero;
	[Export] public Vector3 FollowOffset = Vector3.Zero;
	
	private float shakeStrength = 0.0f;
	[Export] private float followSpeed = 0.88f;
	
	private bool shake = false;


	/// 
	/// Base.
	/// 


	public override void _Ready(){
		base._Ready();
		rng = new RandomNumberGenerator();


		Scale = new Vector3(0.7071f, 1, 1); // horizontal squish

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
			shakeOffset = new Vector3(
				rng.RandfRange(-shakeStrength, shakeStrength),
				rng.RandfRange(-shakeStrength, shakeStrength),
				0
			);
		}
	}


	/// 
	/// Functions.
	/// 


	private void UpdateCamera(float delta){
		if(IsInstanceValid(Target) && Target.IsInsideTree()){
			GlobalPosition = GlobalPosition.Lerp(Target.GlobalPosition + FollowOffset + shakeOffset, followSpeed * delta);
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
		shakeOffset = Vector3.Zero;
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

	private void OnZoomInCallback(){
		Size *= 0.95f;
	}

	private void OnZoomOutCallback(){
		Size *= 1.05f;
	}

	///
	/// Linkage.
	/// 


	private void LinkEvents(){
		shakeTimer.Timeout += StopShake;
		fadeTimer.Timeout += StopFadeTransition;
		SceneManager.Instance.OnScene3DLoaded += LevelEnterTransition;
		SceneManager.Instance.OnScene3DDelayedLoadSet += LevelExitTransition;
		InputManager.Singleton.OnZoomInInput 	+= OnZoomInCallback;
		InputManager.Singleton.OnZoomOutInput 	+= OnZoomOutCallback;
	}

	private void UnlinkEvents(){
		shakeTimer.Timeout -= StopShake;
		fadeTimer.Timeout -= StopFadeTransition;
		SceneManager.Instance.OnScene3DLoaded -= LevelEnterTransition;
		SceneManager.Instance.OnScene3DDelayedLoadSet -= LevelExitTransition;
		InputManager.Singleton.OnZoomInInput 	-= OnZoomInCallback;
		InputManager.Singleton.OnZoomOutInput 	-= OnZoomOutCallback;
	}
}
