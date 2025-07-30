using Godot;
using System;

public partial class EmberStorage : Node{

    /// <summary>
    /// A Notch is what stores embers.
    /// Embers are the resource expended for ability usage.
    /// </summary>

    public const string NodeName            = nameof(EmberStorage);
    public const int NotchMaxEmberValue     = 100;
    [Export] public int MaxNotchAmount      {get; private set;} = 0; 
    public int NotchAmount                  {get; private set;} = 0; 
    public int EmptyNotches                 => MaxNotchAmount - NotchAmount;
    public int MaxEmberValue                {get; private set;} = 0; 
    [Export] public int EmberValue          {get; private set;} = 0; 
    public int RemainderNotchEmberValue     {get; private set;} = 0; 
    public event Action OnAdd; 
    public event Action OnRemove;
    public event Action<int, int> OnNotchesUpdated;


    /// 
    /// Base.
    /// 


    public override void _Ready(){
        base._Ready();
        MaxEmberValue   = MaxNotchAmount * NotchMaxEmberValue;
        NotchAmount = EmberValue / NotchMaxEmberValue;
        RemainderNotchEmberValue = EmberValue % NotchMaxEmberValue;
    }



    /// 
    /// Functions.
    /// 

    public void Add(int amount){
        EmberValue += amount;
        if(EmberValue > MaxEmberValue){
            EmberValue = MaxEmberValue;
        }
        CalculateNotches();
        OnAdd?.Invoke();
    } 

    public void Remove(int amount){
        if(EmberValue > 0){
            EmberValue -= amount;
            if(EmberValue < 0){
                EmberValue = 0;
            }
        }
        CalculateNotches();
        OnRemove?.Invoke();
    }

    public bool RemoveRemainder(int amount){
        if(RemainderNotchEmberValue > 0){
            RemainderNotchEmberValue -= amount;
            EmberValue -= amount;
            CalculateNotches();
            return true;
        }
        CalculateNotches();
        return false;
    }

    public void Initialise(int maxNotchAmount, int emberValue){
        MaxNotchAmount  = maxNotchAmount;
        EmberValue      = emberValue;
        MaxEmberValue   = MaxNotchAmount * NotchMaxEmberValue;
        CalculateNotches();
    }

    // the total amount of ember stored, converted into how many notches (containers)
    // it can fill.

    private void CalculateNotches(){
        NotchAmount = EmberValue / NotchMaxEmberValue;
        RemainderNotchEmberValue = EmberValue % NotchMaxEmberValue;
        OnNotchesUpdated?.Invoke(NotchAmount, RemainderNotchEmberValue);
    }
}
