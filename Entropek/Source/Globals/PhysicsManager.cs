using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class PhysicsManager : Node{
    private string[] LayerNames2D;
    private Dictionary<string, int> NameToLayerIDMap = new Dictionary<string, int>();  
    public Vector3 GravityDirection {get;private set;}
    public float Gravity {get;private set;}
    public static PhysicsManager Instance {get;private set;}


    public override void _EnterTree(){
        base._EnterTree();
        Instance = this;
        Gravity = (float)ProjectSettings.GetSetting("physics/2d/default_gravity");
        GravityDirection = (Vector3)ProjectSettings.GetSetting("physics/2d/default_gravity_vector");
        SetLayerNames();
    }

    private void SetLayerNames(){
        LayerNames2D = new string[32];
        for(int i = 0; i < 32; ++i){
            string key = $"layer_names/2d_physics/layer_{i}";
            if(ProjectSettings.HasSetting(key)){
                string layerName = (string)ProjectSettings.GetSetting(key);
                if(layerName!=""){
                    LayerNames2D[i] = layerName;
                    NameToLayerIDMap.Add(layerName, i);
                }
            }
        }
    }

    public bool GetPhysics2DLayerName(uint collisionObjectLayerBit, out string layerName){
        
        layerName = "";
        
        // convert the nodes layer bitmask to layer index. 
        
        int layerIndex = System.Numerics.BitOperations.TrailingZeroCount(collisionObjectLayerBit) + 1;
        
        // GD.Print($"bitmask[{collisionObjectLayer}] index[{layerIndex}]");
        
        if(LayerNames2D.Length < layerIndex){
            return false;
        }

        layerName = LayerNames2D[layerIndex];

        return true;
    }

    public bool GetPhysics2DLayerId(string layerName, out int layerId){
        layerId = -1;

        if(NameToLayerIDMap.ContainsKey(layerName)){
            layerId = NameToLayerIDMap[layerName];
            return true;
        }

        return false;
    }

    public void SetGravity(float gravity){
        ProjectSettings.SetSetting("physics/2d/default_gravity",gravity);
        Gravity = gravity;
    }

    public void SetGravityDirection(Vector3 direction){
        ProjectSettings.SetSetting("physics/2d/default_gravity_vector", direction);
        GravityDirection = direction;
    }

}
