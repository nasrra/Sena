using Godot;
using System;

public partial class AudioListener3D : Node3D{
	private static AudioListener3D Singleton;

	public override void _EnterTree(){
		base._EnterTree();
		if(Singleton != null){
			QueueFree();
			throw new Exception("There can only be one audio listener per scene.");
		}
		else{
			Singleton = this;
		}
	}

	public override void _ExitTree(){
		base._ExitTree();
		Singleton = null;
	}


	public override void _PhysicsProcess(double delta){
		base._PhysicsProcess(delta);
		AudioManager.Singleton.SetListenerPosition(GlobalPosition);
	}

}
