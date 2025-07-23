using Godot;
using System;

// Note: add entities to an entities node so above the camera node.
// so that vignette is always above.

public partial class VignetteShaderController : Node{
    [Export] private CanvasItem canvasItem;
    [Export] private Timer queuedTimer;
    private ShaderMaterial shaderMaterial;
    private event Action intensityStateProcess = null;
    private event Action opacityStateProcess = null;
    private float targetIntensity = 0;
    private float targetOpacity = 0;
    private float intensityStep = 0;
    private float opacityStep = 0;
    private float queuedIntensity = 0;
    private float queuedOpacity = 0;
    private float queuedStep = 0;


    /// 
    /// Base.
    /// 


    public override void _Ready(){
        base._Ready();
        shaderMaterial = (ShaderMaterial)canvasItem.Material; 
    }

    public override void _EnterTree(){
        base._EnterTree();
        LinkEvents();
    }

    public override void _ExitTree(){
        base._ExitTree();
        UnlinkEvents();
    }

    public override void _PhysicsProcess(double delta){
        base._PhysicsProcess(delta);
        intensityStateProcess?.Invoke();
        opacityStateProcess?.Invoke();
    }


    /// 
    /// Functions.
    /// 


    public void Update(float targetIntensity, float targetOpacity, float step){
        intensityStep = (targetIntensity > this.targetIntensity)? step : -step;
        opacityStep = (targetOpacity > this.targetOpacity)? step : -step;
        this.targetIntensity = targetIntensity;
        this.targetOpacity = targetOpacity; 
        intensityStateProcess = IntensityStateProcessSingle;
        opacityStateProcess = OpacityStateProcessSingle;
    }

    private void IntensityStateProcessSingle(){
        float value = (float)shaderMaterial.GetShaderParameter("vignette_intensity") + intensityStep;
        if(Mathf.Abs(value-targetIntensity) <= intensityStep){
            value = targetIntensity;
            intensityStateProcess = null;
        }
        shaderMaterial.SetShaderParameter("vignette_intensity", value);
    }

    private void OpacityStateProcessSingle(){
        float value = (float)shaderMaterial.GetShaderParameter("vignette_opacity") + opacityStep;
        if(Mathf.Abs(value-targetOpacity) <= opacityStep){
            value = targetOpacity;
            opacityStateProcess = null;
        }
        shaderMaterial.SetShaderParameter("vignette_opacity", value);
    }


    public void QueueUpdate(float targetIntensity, float targetOpacity, float speed, float timeDelay){
        queuedIntensity = targetIntensity;
        queuedOpacity = targetOpacity;
        queuedStep = speed;
        
        queuedTimer.WaitTime = timeDelay;
        queuedTimer.Start();
    }

    private void DequeueUpdate(){
        Update(queuedIntensity, queuedOpacity, queuedStep);
    }


    /// 
    /// Linkage.
    /// 


    private void LinkEvents(){
        queuedTimer.Timeout += DequeueUpdate;
    }

    private void UnlinkEvents(){
        queuedTimer.Timeout -= DequeueUpdate;
    }
}
