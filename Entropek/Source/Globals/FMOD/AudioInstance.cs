using Godot;
using System;

public struct AudioInstance{
    public FMOD.Studio.EventInstance EventInstance {get;private set;}
    public string Name {get;private set;}

    public AudioInstance(FMOD.Studio.EventInstance eventInstance, string name){
        EventInstance = eventInstance;
        Name = name;
    }
}
