using System.Collections.Generic;
using FMOD;
using Godot;

public partial class AudioManager : Node{

    /// <summary>
    /// Gets the FMOD Studio System created by this AudioManager.
    /// </summary>
    public FMOD.Studio.System StudioSystem {get; private set;}
    
    /// <summary>
    /// Gets the FMOD Core System created by this AudioManager.
    /// </summary>
    public FMOD.System CoreSystem {get; private set;}
    
    /// <summary>
    /// Handles to loaded banks are stored via their name without the ".bank" extension.
    /// </summary>
    private Dictionary<string, FMOD.Studio.Bank> _loadedBanks = new Dictionary<string, FMOD.Studio.Bank>();
    
    /// <summary>
    /// Handles to loaded events are stored via their path without the full relative path (just their event name).
    /// </summary>
    private Dictionary<string, FMOD.Studio.EventDescription> _loadedEvents  = new Dictionary<string, FMOD.Studio.EventDescription>(); 
    
    /// <summary>
    /// Handles to buses are stored via their name without the "bus:/" extension.
    /// </summary>
    private Dictionary<string, FMOD.Studio.Bus> _busHandles = new Dictionary<string, FMOD.Studio.Bus>(); 

    public static AudioManager Singleton {get;private set;}

    /// <summary>
    /// Creates a new AudioManager instance.
    /// </summary>
    /// <param name="fileDirectory">The file directory path, relative to the executable, where all audio files are located.</param>
    public override void _EnterTree(){
        base._EnterTree();

        // load Masters.

        CreateFmodSystems();

        // Use Stereo Audio.
        
        HandleResult(CoreSystem.setSoftwareFormat(
            48000,
            FMOD.SPEAKERMODE.STEREO,
            0 // <-- should never change.
        ));

        // initialise with all defined settings as it cannot be changed afterwards.

        HandleResult(StudioSystem.initialize(
            128, // how many simultaneous audio channels (voices) FMOD can mix and play at the same time.
            FMOD.Studio.INITFLAGS.NORMAL,
            FMOD.INITFLAGS.NORMAL,
            0
        ));

        // load master banks.
        LoadBank("Master");
        LoadBank("Master.strings");
        LoadBusHandle("Master");
        SetBusVolume("Master", 1.0f);

        Singleton = this;
    }


    /// <summary>
    /// Prints the result of an FMOD function call.
    /// </summary>
    /// <param name="result"></param>
    private void PrintResult(FMOD.RESULT result){
        GD.Print($"[AUDIOMANAGR]: {result}");
    }

    /// <summary>
    /// Prints the result of an FMOD function call, if it is not RESULT.OK.
    /// </summary>
    /// <param name="result"></param>
    private void HandleResult(FMOD.RESULT result){
        if(result != FMOD.RESULT.OK){
            PrintResult(result);
        }
    }

    /// <summary>
    /// Creates instances for FMOD Studio and Core System.
    /// </summary>
    private void CreateFmodSystems(){
        HandleResult(FMOD.Studio.System.create(
            out FMOD.Studio.System system
        ));

        HandleResult(system.getCoreSystem(
            out FMOD.System coreSystem)
        );

        StudioSystem = system;
        CoreSystem = coreSystem; 

    }

    /// <summary>
    /// Disposes of the FMOD Studio and Core System.
    /// </summary>
    public override void _ExitTree(){
        base._ExitTree();
        GD.Print($"[AudioManager] Dispose");
        
        // unload all banks before disposal.
        
        foreach(FMOD.Studio.Bank bank in _loadedBanks.Values){
            bank.unload();
        }

        // release all systems.

        HandleResult(CoreSystem.release());
        HandleResult(StudioSystem.release());
    }

    /// <summary>
    /// Loads a bank instance into the FMOD Studio System to play sounds from.
    /// </summary>
    /// <param name="bankName">The name of a bank to load, without the ".bank" extension.</param>
    public void LoadBank(string bankName){
        
        // Load the bank from FMOD studio.
        
        HandleResult(StudioSystem.loadBankFile(
            GetBankPath(bankName),
            FMOD.Studio.LOAD_BANK_FLAGS.NORMAL,
            out FMOD.Studio.Bank bank)
        );
        _loadedBanks.Add(bankName, bank);
        
        // Get all event descriptions from the bank. 
        
        bank.getEventList(out FMOD.Studio.EventDescription[] events);
        foreach(FMOD.Studio.EventDescription e in events){
            e.getPath(out string path);
            path = System.IO.Path.GetFileNameWithoutExtension(path);
            GD.Print(path);
            _loadedEvents.Add(path, e);
        }
    }

    /// <summary>
    /// Unloads a bank instance from the FMOD Studio System.
    /// </summary>
    /// <param name="bankName">The name of a bank to load, without the ".bank" extension.</param>
    public void UnloadBank(string bankName){

        // Get loaded bank.
        FMOD.Studio.Bank bank = _loadedBanks[bankName];
        
        // Remove all loaded event descriptions.

        bank.getEventList(out FMOD.Studio.EventDescription[] events);
        foreach(FMOD.Studio.EventDescription e in events){
            e.getPath(out string path);
            path = System.IO.Path.GetFileNameWithoutExtension(path);
            _loadedEvents.Remove(path);
        }

        // Unload the bank from FMOD Studio.
        
        _loadedBanks[bankName].unload();

        // Remove the unloaded bank.

        _loadedBanks.Remove(bankName);
    }

    /// <summary>
    /// Loads the handle to access a bus within the FMOD Studio System.
    /// </summary>
    /// <param name="busName">The name of a bus handle to load, without the "bus:/" prefix.</param>
    public void LoadBusHandle(string busName){
        
        // Get the bus.

        StudioSystem.getBus("bus:/"+(busName == "Master"? "" : busName), out FMOD.Studio.Bus bus);
        ref FMOD.Studio.Bus b = ref bus;
        _busHandles.Add(busName == ""? "Master" : busName, bus);
    }

    /// <summary>
    /// Sets the volume of a bus, via a bus handle that has been loaded.
    /// </summary>
    /// <param name="busHandleName">The name of the loaded bus handle to use when accessing a bus in the FMOD Studio System.</param>
    /// <param name="volume">The specified volume to change to.</param>
    public void SetBusVolume(string busHandleName, float volume){
        _busHandles[busHandleName].setVolume(volume);
    }

    /// <summary>
    /// Gets the volume of a bus, via a bus handle that has been loaded.
    /// </summary>
    /// <param name="busHandleName">The name of the loaded bus handle to use when accessing a bus in the FMOD Studio System.</param>
    /// <returns></returns>
    public float GetBusVolume(string busHandleName){
        _busHandles[busHandleName].getVolume(out float volume);
        return volume;
    }

    /// <summary>
    /// Plays a one-shot instance of a sound.
    /// </summary>
    /// <param name="eventName">The name of the event to play, excluding its relative path.</param>
    public void PlayOneShot(string eventName){
        
        // Get the loaded event description.

        FMOD.Studio.EventDescription desc = _loadedEvents[eventName];
        
        // Create and play an instance of the description.

        desc.createInstance(out FMOD.Studio.EventInstance inst);
        inst.start();
        
        // Immediately release it, so when the sound has finished, FMOD Studio can garbage collect it.
        inst.release();

        // update the audio system to play the sound.
        StudioSystem.update();
    }

    /// <summary>
    /// Plays a one-shot instance of a sound.
    /// </summary>
    /// <param name="eventName"></param>
    /// <param name="globalPosition"></param>
    public void PlayOneShot(string eventName, Vector2 globalPosition){
        
        FMOD.Studio.EventDescription desc = _loadedEvents[eventName];
        desc.createInstance(out FMOD.Studio.EventInstance inst);

        ATTRIBUTES_3D attributes = new ATTRIBUTES_3D{
            position = GodotToFmodPosition(globalPosition),
            velocity = new FMOD.VECTOR { x = 0, y = 0, z = 0 },
            forward = new FMOD.VECTOR { x = 0, y = 0, z = 1 },
            up = new FMOD.VECTOR { x = 0, y = 1, z = 0 }
        };
        inst.set3DAttributes(attributes);

        inst.start();
        inst.release(); // Immediately release it, so when the sound has finished, FMOD Studio can garbage collect it.

        StudioSystem.update();
    }

    public bool IsBankLoaded(string bankName){
        return _loadedBanks.ContainsKey(bankName);
    }

    /// <summary>
    /// Gets the full absolute path to a bank file within the Godot project.
    /// </summary>
    /// <param name="bankName">The specified name of the bank to get the path to.</param>
    /// <returns>Absolute path to the bank file on the filesystem.</returns>
    private string GetBankPath(string bankName){
        // Use Godot's project settings to convert from "res://" to actual file path
        string godotPath = $"res://Exports/Audio/Desktop/{bankName}.bank";
        string absolutePath = ProjectSettings.GlobalizePath(godotPath);

        GD.Print($"Bank path: {absolutePath}");
        return absolutePath;
    }

    private VECTOR GodotToFmodPosition(Vector2 globalPosition){
        return new VECTOR{
            x = globalPosition.X,
            y = 0,
            z = -globalPosition.Y
        };
    }


public void SetListenerPosition(Vector2 globalPosition){
    ATTRIBUTES_3D listenerAttributes = new ATTRIBUTES_3D{
        position = GodotToFmodPosition(globalPosition),
        velocity = new VECTOR { x = 0, y = 0, z = 0 },
        forward  = new VECTOR { x = 0, y = 0, z = 1 },
        up       = new VECTOR { x = 0, y = 1, z = 0 }
    };

    // Assuming you have a valid FMOD.Studio.System instance called studioSystem
    RESULT result = StudioSystem.setListenerAttributes(0, listenerAttributes);
    if (result != RESULT.OK){
        GD.PrintErr($"Failed to set listener attributes: {result}");
    }
    StudioSystem.update();
}
}