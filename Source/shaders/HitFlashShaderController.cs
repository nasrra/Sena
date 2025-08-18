using Godot;
using System;

public partial class HitFlashShaderController : Node{
    [Export] private CanvasItem sprite;
    private ShaderMaterial shaderMaterial;
    [Export] private Timer timer;
    private event Action stateProccess;

    public override void _Ready(){
        base._Ready();
        // shaderMaterial = (ShaderMaterial)sprite.Material;
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
        stateProccess?.Invoke();
    }


    public void Flash(){
        stateProccess = ActiveStateProcess;
        timer.Start();
    }

    private void ActiveStateProcess(){
        shaderMaterial.SetShaderParameter("flash_value", timer.TimeLeft / timer.WaitTime);
    }

    private void Stop(){
        stateProccess = null;
    }

    private void LinkEvents(){
        timer.Timeout += Stop;
    }

    private void UnlinkEvents(){
        timer.Timeout -= Stop;
    }
}
