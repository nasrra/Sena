using Godot;
using System;

// use fire feather to launch into a boss suction attack so it clogs the device and stuns the boss.

public partial class EmberNotchGui : Control{
    [Export] private TextureRect container;
    [Export] private TextureRect fill;
    
    public void UpdateFill(int emberNotchValue){
        float difference = (float)emberNotchValue / (float)EmberStorage.NotchMaxEmberValue;
        fill.Scale = new Vector2(difference, difference);
    }
}
