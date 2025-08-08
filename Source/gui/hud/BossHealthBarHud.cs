using Godot;
using System;

public partial class BossHealthBarHud : Control{

    /// 
    /// Variables
    /// 

    public const string NodeName = nameof(BossHealthBarHud);
    [Export] private ProgressBar valueBar;
    [Export] private ProgressBar trailBar;
    [Export] private Timer trailBarCatchUpDelay;
    [Export] private AnimationPlayer animator;
    [Export] private Label nameTag;
    private Health health = null;


    /// 
    /// Base.
    /// 


    public override void _EnterTree(){
        base._EnterTree();
        #if TOOLS
        Entropek.Util.Node.VerifyName(this, NodeName);
        #endif
        LinkTimers();
    }

    public override void _ExitTree(){
        base._ExitTree();
        UnlinkTimers();
    }


    /// 
    /// Functions.
    /// 


    public void UpdateValueBar(){
        trailBarCatchUpDelay.Start();
        valueBar.Value = health.Value;
    }

    public void DisableBar(){
        animator.Play("Disable");
    }

    public void EnableBar(){
        animator.Play("Enable");
    }

    public void UpdateTrailBar(){
        trailBar.Value = valueBar.Value;
    }

    public void SetNameTag(string name){
        nameTag.Text = name;
    }


    /// 
    /// Health Linkage.
    /// 


    public void LinkToHealth(Health health){
        if(this.health != null){
            throw new Exception("health node has already been linked!");
        }


        this.health = health;
        trailBar.MaxValue = health.Max;
        trailBar.Value = health.Value;
        valueBar.MaxValue = health.Max;
        valueBar.Value = health.Value;
        health.OnHeal           += OnHealCallback;
        health.OnDamage         += OnDamageCallback;
        health.OnDeath          += OnDeathCallback;
        UpdateValueBar();
    }

    public void UnlinkFromHealth(){
        health.OnHeal           -= OnHealCallback;
        health.OnDamage         -= OnDamageCallback;
        health.OnDeath          -= OnDeathCallback;
        health = null;
    }

    private void OnHealCallback(){
        UpdateValueBar();
    }

    private void OnDamageCallback(){
        UpdateValueBar();
    }

    private void OnDeathCallback(){
        UpdateValueBar();
        DisableBar();
    }


    /// 
    /// Timer linkage.
    /// 


    private void LinkTimers(){
        trailBarCatchUpDelay.Timeout += UpdateTrailBar;
    }

    private void UnlinkTimers(){
        trailBarCatchUpDelay.Timeout -= UpdateTrailBar;        
    }
}
