using Godot;
using System;

public partial class AvoidancePoint : Area2D{
    
    public const string NodeName = nameof(AvoidancePoint);
    
    [Export] public float Strength = 0.0f;
    
    public override void _EnterTree(){
        base._EnterTree();
        #if TOOLS
            Entropek.Util.Node.VerifyName(this, NodeName);
        #endif
    }
}
