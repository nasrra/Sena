using Godot;
using System;

namespace Entropek.Ai;

[Flags]
public enum NavigationType : byte{
    Blocked     = 1 << 0, // 1
    PassThrough = 1 << 1, // 2
    Open        = 1 << 2, // 4
}