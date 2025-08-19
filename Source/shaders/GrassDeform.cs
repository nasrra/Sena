using Godot;
using System;

public partial class GrassDeform : Node{
    [Export] Godot.Collections.Array<ShaderMaterial> shaderMaterials;
    [Export] Node3D deformer;
    public override void _Ready(){
        base._Ready();
    }


    public override void _PhysicsProcess(double delta){
        base._PhysicsProcess(delta);
        for(int i = 0; i < shaderMaterials.Count; i++){
            shaderMaterials[i].SetShaderParameter("character_position", deformer.GlobalPosition);
        }
    }

}
