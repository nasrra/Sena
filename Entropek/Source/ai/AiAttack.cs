using Godot;
using System;

public partial class AiAttack : Resource{
    [Export] public byte Id {get; private set;}
    [Export] public double Cooldown {get; private set;}
    [Export] public float Range {get; private set;}
}