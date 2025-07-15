using Godot;
using System;

public partial class EmberStorage : Node{
    public const string NodeName = nameof(EmberStorage);
    [Export] public int Max {get; private set;}
    [Export] public int Value {get; private set;}
    public event Action OnAdd;
    public event Action OnRemove;


    public void Add(int amount, out int remainder){
        remainder = amount;
        Value += amount;
        if(Value > Max){
            remainder = Value - Max; // send back.
            Value = Max;
        }
        OnAdd?.Invoke();
    } 

    public void Remove(int amount, out int remainder){
        remainder = amount;
        if(Value > 0){
            Value -= amount;
            if(Value <= 0){
                remainder = -Value; // send back.
                Value = 0;
            }
        }
        OnRemove?.Invoke();
    }

}
