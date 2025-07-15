using Godot;
using System;

public partial class PhysicsManager : Node{
    private static string[] LayerNames2D;
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
                LayerNames2D[i] = (string)ProjectSettings.GetSetting(key);
            }
        }
    }

    public static string GetPhysics2DLayerName(uint collisionObjectLayer){
        
        // convert the nodes layer bitmask to layer index. 
        
        int layerIndex = System.Numerics.BitOperations.TrailingZeroCount(collisionObjectLayer) + 1;
        
        // GD.Print($"bitmask[{collisionObjectLayer}] index[{layerIndex}]");
        
        return LayerNames2D[layerIndex];
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
