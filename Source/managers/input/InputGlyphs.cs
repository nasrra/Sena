using Godot;
using System;
using System.Collections.Generic;

public class InputGlyphs{
    public readonly  Dictionary<JoyButton, string> JoyButtonUnicode = new Dictionary<JoyButton, string>(){
        { JoyButton.X,              "\u21A4"},
        { JoyButton.Y,              "\u21A5"},
        { JoyButton.B,              "\u21A6"},
        { JoyButton.A,              "\u21A7"},
        { JoyButton.LeftShoulder,   "\u2198"},
        { JoyButton.RightShoulder,  "\u2199"},
        { JoyButton.DpadLeft,       "\u219E"},
        { JoyButton.DpadUp,         "\u219F"},
        { JoyButton.DpadRight,      "\u21A0"},
        { JoyButton.DpadDown,       "\u21A1"},
        { JoyButton.Start,          "\u21FB"} // options.
    };

    public readonly Dictionary<JoyAxis, string> JoyAxisUnicode = new Dictionary<JoyAxis, string>(){
        { JoyAxis.TriggerLeft,      "\u2196"},
        { JoyAxis.TriggerRight,     "\u2197"},
        { JoyAxis.LeftX,            "\u21CB"}, // all left joystick.
        { JoyAxis.LeftY,            "\u21CB"}, // all left joystick.
        { JoyAxis.RightX,           "\u21CC"}, // all right joystick.
        { JoyAxis.RightY,           "\u21CC"}, // all right joystick.
    };

    public readonly Dictionary<Key, string> KeyboardUnicode = new Dictionary<Key, string>(){
        {Key.E,     "\uFF25"},
        {Key.F,     "\uFF26"},
        {Key.Space, "\u243A"},
        {Key.Q,     "\uFF31"},
        {Key.W,     "\uFF37"},
        {Key.A,     "\uFF21"},
        {Key.S,     "\uFF33"},
        {Key.D,     "\uFF24"},
        {Key.Escape,"\u242F"}
    };

    public readonly Dictionary<MouseButton, string> MouseButtonUnicode = new Dictionary<MouseButton, string>(){
        {MouseButton.Left, "\u278A"},
        {MouseButton.Right,"\u278B"}
    };

    public readonly Dictionary<string, string> SpecialGlyphs = new Dictionary<string, string>(){
        { "MouseAny",       "\u27FC" },
        { "LeftJoyStick",   "\u21CB"}, 
        { "RightJoyStick",  "\u21CC"}, 
    };
}
