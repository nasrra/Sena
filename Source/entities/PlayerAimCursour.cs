using Godot;
using System;

public partial class PlayerAimCursour : Node3D{

	[Export] public Node3D Cursour {get;private set;}
	[Export] public Vector3 AimDirection {get;private set;} = Vector3.Zero;
	[Export] public float CursourDistance {get; private set;} = 1;
	[Export] public float AimAngle {get; private set;} = 0f;


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


	/// 
	/// Functions.
	/// 


	public override void _Process(double delta){
		base._PhysicsProcess(delta);
		AimAngle = Mathf.Atan2(AimDirection.Y, AimDirection.X);
		AimAngle = Mathf.RadToDeg(AimAngle);
		Cursour.GlobalPosition = GlobalPosition+AimDirection * CursourDistance;
	}

	private void HandleAimDirection(Vector2 direction){
		Vector3 trueDirection = new Vector3(direction.X, 0, direction.Y);
		if(InputManager.Singleton.IsGamepadConnected == true){
			AimDirection = trueDirection.Normalized();
		}else{
			AimDirection = (trueDirection-GlobalPosition).Normalized();
		}
	}

	private bool ConvertToWorldPosition(Vector2 aimPosition, out Vector3 worldPos){
		worldPos = Vector3.Zero;

		Camera3D camera = GetViewport().GetCamera3D();

		// Generate ray origin and direction from camera through mouse
		Vector3 rayOrigin = camera.ProjectRayOrigin(aimPosition);
		Vector3 rayDirection = camera.ProjectRayNormal(aimPosition);

		// Define the ground plane (Y = 0)
		Plane groundPlane = new Plane(Vector3.Up, 0f);

		Vector3? intersect = groundPlane.IntersectsRay(rayOrigin, rayDirection);
		worldPos = intersect.GetValueOrDefault();
		return intersect!=null;
	}

	/// 
	/// Linkage.
	/// 


	private void LinkEvents(){
		InputManager.Singleton.OnAimInput += HandleAimDirection;
	}

	private void UnlinkEvents(){
		InputManager.Singleton.OnAimInput -= HandleAimDirection;
	}
}
