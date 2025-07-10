using Godot;
using System;

public partial class Health : Node{
    public const string NodeName = "Health";
    [Export] public float Max {get; private set;}
    [Export] public float Current {get; private set;}
    public event Action OnDeath;
    public event Action OnDamage;
    public event Action OnHeal;

    public void Heal(float amount){
        Current += amount;
        if(Current > Max){
            Current = Max;
        }
        OnHeal?.Invoke();
    } 

    public void Damage(float amount){
        Current -= amount;
        OnDamage?.Invoke();
        if(Current <= 0){
            OnDeath?.Invoke();
        }
    }
}
