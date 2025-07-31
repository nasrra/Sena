using Godot;
using System;

public partial class VolumeSlider : Control{
    [Export] private HSlider slider;
    [Export] private string audioBus = "";


    /// 
    /// Base.
    /// 


    public override void _EnterTree(){
        base._EnterTree();
        LinkEvents();
        slider.Value = AudioManager.Singleton.GetBusVolume(audioBus);
    }

    public override void _ExitTree(){
        base._ExitTree();
        UnlinkEvents();
    }


    /// 
    /// Functions.
    /// 


    private void HandleValueChanged(double value){
        AudioManager.Singleton.SetBusVolume(audioBus, (float)value);
    }


    /// 
    /// Linkge.
    /// 


    private void LinkEvents(){
        slider.ValueChanged += HandleValueChanged;
    }

    private void UnlinkEvents(){
        slider.ValueChanged -= HandleValueChanged;
    }
}
