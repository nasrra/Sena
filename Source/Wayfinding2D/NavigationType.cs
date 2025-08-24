using Godot;
using System;

namespace Entropek.Ai;

[Flags]
public enum NavigationType : byte{
    None        = 1 << 0,  // 1
    Blocked     = 1 << 1, // 2
    PassThrough = 1 << 2, // 4
    Open        = 1 << 3, // 8

    All = 
        Blocked     | 
        PassThrough | 
        Open
}