using System;

[Flags]
public enum PlayerActions : byte{
    None        = 0,
    Attack 		= 1,
    Heal        = 2,
    Dash   		= 4,
    FireFeather = 8
}
