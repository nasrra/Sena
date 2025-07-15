using Godot;
using System;

public partial class Health : Node{
    public const string NodeName = "Health";
    [Export] public int Max {get; private set;}
    [Export] public int Value {get; private set;}
    public event Action OnDeath;
    public event Action OnDamage;
    public event Action OnHeal;

    public void Heal(int amount){
        Value += amount;
        if(Value > Max){
            Value = Max;
        }
        OnHeal?.Invoke();
    } 

    public void Damage(int amount){
        Value -= amount;
        OnDamage?.Invoke();
        if(Value <= 0){
            OnDeath?.Invoke();
        }
    }
}
