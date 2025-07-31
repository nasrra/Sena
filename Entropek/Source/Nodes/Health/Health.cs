using Godot;
using System;

public partial class Health : Node{
    public const string NodeName = "Health";
    [Export] private Timer invincibleTimer; 
    [Export] public int Max {get; private set;}
    [Export] public int Value {get; private set;}
    public event Action OnDeath;
    public event Action OnDamage;
    public event Action OnHeal;
    public event Action OnInvincible;
    public event Action OnVincible;
    public bool Invincible {get;private set;} = false;


    /// 
    /// Base.
    /// 


    public override void _EnterTree(){
        base._EnterTree();
        LinkEvents();
    }

    public override void _ExitTree(){
        base._ExitTree();
        UnlinkEvents();
    }


    /// 
    /// Functions.
    /// 


    public void Heal(int amount){
        Value += amount;
        if(Value > Max){
            Value = Max;
        }
        OnHeal?.Invoke();
    } 

    public void HealToMax(){
        Value = Max;
        OnHeal?.Invoke();
    }

    public void Damage(int amount){
        if(Invincible==true){
            return;
        }
        Value -= amount;
        OnDamage?.Invoke();
        if(Value <= 0){
            OnDeath?.Invoke();
        }
    }

    public void SetInvincible(float time){
        invincibleTimer.WaitTime = time;
        invincibleTimer.Start();
        SetInvincible();
    }

    public void SetInvincible(){
        Invincible = true;
        OnInvincible?.Invoke();
    }

    public void SetVincible(){
        Invincible = false;
        OnVincible?.Invoke();
    }

    public void Initialise(int maxValue,int currentValue){
        Max = maxValue;
        Value = currentValue;
    }

    public void PauseState(){
        invincibleTimer.Paused = true;
    }

    public void ResumeState(){
        invincibleTimer.Paused = false;    
    }

    /// 
    /// Linkage.
    /// 


    private void LinkEvents(){
        invincibleTimer.Timeout += SetVincible;
    }

    private void UnlinkEvents(){
        invincibleTimer.Timeout -= SetVincible;
    }
}
