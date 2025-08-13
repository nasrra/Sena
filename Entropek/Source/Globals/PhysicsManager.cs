using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class PhysicsManager : Node{
	
	private string[] LayerNames2D;
	private string[] LayerNames3D;
	private Dictionary<string, int> NameToLayer2DIDMap = new Dictionary<string, int>();  
	private Dictionary<string, int> NameToLayer3DIDMap = new Dictionary<string, int>();
	public Vector3 GravityDirection2D {get;private set;}
	public Vector3 GravityDirection3D {get;private set;}
	public float Gravity2D {get;private set;}
	public float Gravity3D {get;private set;}
	public static PhysicsManager Singleton {get;private set;}


	public override void _EnterTree(){
		base._EnterTree();
		Singleton = this;
		Initialise2D();
		Initialise3D();
	}


	/// <summary>
	/// 2D
	/// </summary>


	private void Initialise2D(){
		Gravity2D = (float)ProjectSettings.GetSetting("physics/2d/default_gravity");
		GravityDirection2D = (Vector3)ProjectSettings.GetSetting("physics/2d/default_gravity_vector");
		LayerNames2D = new string[32];
		for(int i = 0; i < 32; ++i){
			string key = $"layer_names/2d_physics/layer_{i}";
			if(ProjectSettings.HasSetting(key)){
				string layerName = (string)ProjectSettings.GetSetting(key);
				if(layerName!=""){
					LayerNames2D[i] = layerName;
					NameToLayer2DIDMap.Add(layerName, i);
				}
			}
		}
	}

	public string GetPhysics2DLayerName(uint collisionObjectLayerBit){        
		// convert the nodes layer bitmask to layer index. 
		
		int layerIndex = System.Numerics.BitOperations.TrailingZeroCount(collisionObjectLayerBit) + 1;
		
		// GD.Print($"bitmask[{collisionObjectLayer}] index[{layerIndex}]");
		
		if(LayerNames2D.Length < layerIndex){
			throw new Exception($"{collisionObjectLayerBit} is not a valid Physics2D bit layer.");
		}

		return LayerNames2D[layerIndex];
	}

	public int GetPhysics2DLayerId(string layerName){
		if(NameToLayer2DIDMap.ContainsKey(layerName)){
			return NameToLayer2DIDMap[layerName];;
		}

		throw new Exception($"{layerName} is not a Physics2D layer.");
	}

	public void SetGravity(float gravity){
		ProjectSettings.SetSetting("physics/2d/default_gravity",gravity);
		Gravity2D = gravity;
	}

	public void SetGravityDirection(Vector3 direction){
		ProjectSettings.SetSetting("physics/2d/default_gravity_vector", direction);
		GravityDirection2D = direction;
	}



	/// <summary>
	/// 3D
	/// </summary>


	private void Initialise3D(){
		Gravity3D = (float)ProjectSettings.GetSetting("physics/3d/default_gravity");
		GravityDirection3D = (Vector3)ProjectSettings.GetSetting("physics/3d/default_gravity_vector");
		LayerNames3D = new string[32];
		for(int i = 0; i < 32; ++i){
			string key = $"layer_names/3d_physics/layer_{i}";
			if(ProjectSettings.HasSetting(key)){
				string layerName = (string)ProjectSettings.GetSetting(key);
				if(layerName!=""){
					LayerNames3D[i] = layerName;
					NameToLayer3DIDMap.Add(layerName, i);
				}
			}
		}
	}

	public string GetPhysics3DLayerName(uint collisionObjectLayerBit){        
		// convert the nodes layer bitmask to layer index. 
		
		int layerIndex = System.Numerics.BitOperations.TrailingZeroCount(collisionObjectLayerBit) + 1;
		
		// GD.Print($"bitmask[{collisionObjectLayer}] index[{layerIndex}]");
		
		if(LayerNames3D.Length < layerIndex){
			throw new Exception($"{collisionObjectLayerBit} is not a valid Physics2D bit layer.");
		}

		return LayerNames3D[layerIndex];
	}

	public int GetPhysics3DLayerId(string layerName){
		if(NameToLayer3DIDMap.ContainsKey(layerName)){
			return NameToLayer3DIDMap[layerName];;
		}

		throw new Exception($"{layerName} is not a Physics3D layer.");
	}

	public void SetGravity3D(float gravity){
		ProjectSettings.SetSetting("physics/3d/default_gravity",gravity);
		Gravity3D = gravity;
	}

	public void SetGravityDirection3D(Vector3 direction){
		ProjectSettings.SetSetting("physics/3d/default_gravity_vector", direction);
		GravityDirection3D = direction;
	}
}
