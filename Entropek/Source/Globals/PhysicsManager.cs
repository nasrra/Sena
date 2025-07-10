using Godot;
using System;

public partial class PhysicsManager : Node{
    public Vector3 GravityDirection {get;private set;}
    public float Gravity {get;private set;}
    public static PhysicsManager Instance {get;private set;}

    public override void _EnterTree(){
        base._EnterTree();
        Instance = this;
        Gravity = (float)ProjectSettings.GetSetting("physics/3d/default_gravity");
        GravityDirection = (Vector3)ProjectSettings.GetSetting("physics/3d/default_gravity_vector");
        GD.Print(Gravity);
    }

    public void SetGravity(float gravity){
        ProjectSettings.SetSetting("physics/3d/default_gravity",gravity);
        Gravity = gravity;
    }

    public void SetGravityDirection(Vector3 direction){
        ProjectSettings.SetSetting("physics/3d/default_gravity_vector", direction);
        GravityDirection = direction;
    }
}
