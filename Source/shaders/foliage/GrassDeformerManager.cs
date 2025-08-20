using Entropek.Collections;
using Godot;
using System;
using System.Collections.Generic;

public partial class GrassDeformerManager : Node{
    [Export] Godot.Collections.Array<ShaderMaterial> grassShaders = new Godot.Collections.Array<ShaderMaterial>();

    private SwapbackList<GrassDeformer> grassDeformers = new SwapbackList<GrassDeformer>();
    private Vector3[] deformersPosition = Array.Empty<Vector3>();
    private float[] deformersRadius = Array.Empty<float>();
    private float[] deformersStrength = Array.Empty<float>();
    
    const string DeformerCount = "deformer_count";
    const string DeformersPosition = "deformers_position";
    const string DeformersRadius = "deformers_radius";
    const string DeformersStrength = "deformers_strength";

    public static GrassDeformerManager Singleton {get;private set;}


    /// 
    /// Base.
    /// 


    public override void _EnterTree(){
        base._EnterTree();
        Singleton = this;
    }

    public override void _PhysicsProcess(double delta){
        base._PhysicsProcess(delta);
        UpdateDeformerPositions();
    }   

    public override void _ExitTree(){
        base._ExitTree();
        Singleton = null;
    }


    /// 
    /// public deformer management.
    /// 


    public void AddDeformer(GrassDeformer grassDeformer){
        grassDeformers.Add(grassDeformer);
        VerifyArrayLengths();
        UpdateDeformers();
    }

    public void RemoveDeformer(GrassDeformer grassDeformer){
        grassDeformers.Remove(grassDeformer);
        VerifyArrayLengths();
        UpdateDeformers();
    }


    /// 
    /// private deformer management.
    /// 


    private void UpdateDeformerPositions(){

        // populate data.

        for(int i = 0; i < grassDeformers.Count; i++){
            deformersPosition[i] = grassDeformers[i].GlobalPosition;
        }

        // set shaders.

        for (int i = 0; i < grassShaders.Count; i++){
            grassShaders[i].SetShaderParameter(DeformersPosition, deformersPosition);
        }
    }

    private void UpdateDeformerRaddi(){
        
        // populate data.

        for(int i = 0; i < grassDeformers.Count; i++){
            deformersRadius[i] = grassDeformers[i].Radius;
        }

        // set shaders.

        for (int i = 0; i < grassShaders.Count; i++){
            grassShaders[i].SetShaderParameter(DeformersRadius, deformersRadius);
        }
    }

    private void UpdateDeformerStrengths(){

        // populate data.

        for(int i = 0; i < grassDeformers.Count; i++){
            deformersStrength[i] = grassDeformers[i].Strength;
        }

        // set shaders.

        for (int i = 0; i < grassShaders.Count; i++){
            grassShaders[i].SetShaderParameter(DeformersStrength, deformersStrength);
        }
    }

    private void UpdateDeformers(){

        // populate data.

        for(int i = 0; i < grassDeformers.Count; i++){
            deformersPosition[i] = grassDeformers[i].GlobalPosition;
            deformersRadius[i] = grassDeformers[i].Radius;
            deformersStrength[i] = grassDeformers[i].Strength;
        }

        // set shaders.
        
        for (int i = 0; i < grassShaders.Count; i++){
            grassShaders[i].SetShaderParameter(DeformersPosition, deformersPosition);
            grassShaders[i].SetShaderParameter(DeformersStrength, deformersStrength);
            grassShaders[i].SetShaderParameter(DeformersRadius, deformersRadius);
        }

    }

    private void VerifyArrayLengths(){
        if(deformersPosition.Length != grassDeformers.Count){
            deformersPosition   = new Vector3[grassDeformers.Count];
            deformersRadius     = new float[grassDeformers.Count];
            deformersStrength   = new float[grassDeformers.Count];
            for(int i = 0; i < grassShaders.Count; i++){
                grassShaders[i].SetShaderParameter(DeformerCount, grassDeformers.Count);
            }
        }
    }
}
