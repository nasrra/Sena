using Godot;
using System;

public partial class AvoidancePoint : Area3D{
    
    public const string NodeName = nameof(AvoidancePoint);
    
    [Export] public float Strength {get; private set;} = 0.0f;
    [Export] private CollisionShape3D collisionShape;
    public float Radius {get; private set;} = 0.0f;

    public override void _EnterTree(){
        base._EnterTree();
        #if TOOLS
            Entropek.Util.Node.VerifyName(this, NodeName);
        #endif
        if(collisionShape.Shape is SphereShape3D sphere){
            Radius = 1.732f * Scale.X * sphere.Radius;
        }
        else{
            throw new Exception();
        }
    }
}
