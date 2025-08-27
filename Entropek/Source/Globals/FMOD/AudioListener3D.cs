using Godot;
using System;

public partial class AudioListener3D : Node3D{
	private static AudioListener3D Singleton;

	public override void _EnterTree(){
		base._EnterTree();
		GD.Print("audio listener entered");
		if(Singleton != null){
			QueueFree();
			throw new Exception("There can only be one audio listener per scene.");
		}
		else{
			Singleton = this;
		}
	}

	public override void _ExitTree(){
		Singleton = null;
		GD.Print("audio listener exited");
		base._ExitTree();
	}


	public override void _PhysicsProcess(double delta){
		base._PhysicsProcess(delta);
		AudioManager.Singleton.SetListenerPosition(GlobalPosition);
	}

}
