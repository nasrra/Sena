using Godot;
using System;

public struct AudioInstance{
    public FMOD.Studio.EventInstance EventInstance {get;private set;}
    public string Name {get;private set;}
    public bool Managed {get;private set;}

    public AudioInstance(FMOD.Studio.EventInstance eventInstance, string name, bool managed){
        EventInstance   = eventInstance;
        Name            = name;
        Managed         = managed;
    }
}
