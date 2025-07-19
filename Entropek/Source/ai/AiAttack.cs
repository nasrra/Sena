using Godot;
using System;

public partial class AiAttack : Resource{
    [Export] public float Cooldown              {get; private set;}
    [Export] public float LeadInTime            {get; private set;}
    [Export] public float AttackTime            {get; private set;}
    [Export] public float FollowThroughTime     {get; private set;}
    [Export] public float MinTargetDistance     {get; private set;}
    [Export] public int Damage                  {get; private set;}
    [Export] public byte Id                     {get; private set;}
}

public enum AttackDirection : byte{
    Down = 0,
    Left = 1,
    Omni = 2,
    Right = 3,
    Up = 4
}
