using Godot;
using System;

public partial class EmberBarHud : ProgressBar{
    public const string NodeName = nameof(EmberBarHud);
    private EmberStorage emberStorage;
    [Export] private ProgressBar valueBar;
    [Export] private ProgressBar trailBar;
    [Export] private Timer trailBarCatchUpDelay;

    public override void _EnterTree(){
        base._EnterTree();
        LinkEvents();
        #if TOOLS
        Entropek.Util.Node.VerifyName(this, NodeName);
        #endif
    }

    public override void _ExitTree(){
        base._ExitTree();
        UnlinkEvents();
    }

    public void UpdateValueBar(){
        valueBar.Value = emberStorage.Value;
        trailBarCatchUpDelay.Start();
        GD.Print("update value");
    }

    public void UpdateTrailBar(){
        trailBar.Value = valueBar.Value;
    }

    public void LinkToEmberStorage(EmberStorage emberStorage){
        this.emberStorage = emberStorage;
        emberStorage.OnAdd      += UpdateValueBar;
        emberStorage.OnAdd      += UpdateTrailBar;
        emberStorage.OnRemove   += UpdateValueBar;
        UpdateValueBar();
    }

    public void UnlinkFromEmberStorage(){
        if(emberStorage!=null){
            emberStorage.OnAdd      -= UpdateValueBar;
            emberStorage.OnAdd      -= UpdateTrailBar;
            emberStorage.OnRemove   -= UpdateValueBar;
        }
        emberStorage = null;
    }

    private void LinkEvents(){
        trailBarCatchUpDelay.Timeout += UpdateTrailBar;
    }

    private void UnlinkEvents(){
        trailBarCatchUpDelay.Timeout -= UpdateTrailBar;        
    }
}
