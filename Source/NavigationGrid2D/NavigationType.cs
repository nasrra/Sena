using Godot;
using System;

namespace Entropek.Ai;

public enum NavigationType : byte{
    Block   = 0, // the cell is blocked and cant be travelled accross.
    Open    = 1, // The cell is open to anything coming through it.
    Pass    = 2, // The cell is passable to any that are flagged to do so.
}
