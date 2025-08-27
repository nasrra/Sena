using System.Collections.Generic;
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

    [Export] private Godot.Collections.Array<string> _busesToLoad = new Godot.Collections.Array<string>();

    public static AudioManager Singleton {get;private set;}


    /// 
    /// base.
    /// 


    public override void _EnterTree(){
        base._EnterTree();
        CreateFmodSystems();
        LoadMasters();

        // load other buses.

        for(int i = 0; i < _busesToLoad.Count; i++){
            LoadBusHandle(_busesToLoad[i]);
            SetBusVolume(_busesToLoad[i], 1.0f);
        }

        Singleton = this;
    }

    public override void _Process(double delta){
        base._Process(delta);
        StudioSystem.update();
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
    /// Initialisation.
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
    }

    private void LoadMasters(){
        LoadBank("Master");
        LoadBank("Master.strings");
        LoadBusHandle("Master");
        SetBusVolume("Master", 1.0f);
    }


    /// 
    /// Audio Bank Hanlding.
    /// 


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
            _loadedEvents.Add(GetEventName(e), e);
        }
    }

    public void UnloadBank(string bankName){

        // Get loaded bank.
        FMOD.Studio.Bank bank = _loadedBanks[bankName];
        
        // Remove all loaded event descriptions.

        bank.getEventList(out FMOD.Studio.EventDescription[] events);
        foreach(FMOD.Studio.EventDescription e in events){
            _loadedEvents.Remove(GetEventName(e));
        }

        // Unload the bank from FMOD Studio.
        
        _loadedBanks[bankName].unload();

        // Remove the unloaded bank.

        _loadedBanks.Remove(bankName);
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
        return absolutePath;
    }



    /// 
    /// Audio Bus Handling.
    /// 


    public void LoadBusHandle(string busName){
        StudioSystem.getBus("bus:/"+(busName == "Master"? "" : busName), out FMOD.Studio.Bus bus);
        ref FMOD.Studio.Bus b = ref bus;
        _busHandles.Add(busName == ""? "Master" : busName, bus);
    }

    public void SetBusVolume(string busHandleName, float volume){
        _busHandles[busHandleName].setVolume(volume);
    }

    public float GetBusVolume(string busHandleName){
        _busHandles[busHandleName].getVolume(out float volume);
        return volume;
    }


    /// 
    /// Event Handling.
    /// 

    public string GetEventName(FMOD.Studio.EventDescription eventDescription){
        eventDescription.getPath(out string path);
        return System.IO.Path.GetFileNameWithoutExtension(path);
    }

    public AudioInstance PlayManagedEvent(string eventName){
        AudioInstance instance = CreateAudioInstance(eventName, managed: true);
        instance.EventInstance.start();
        instance.EventInstance.release();
        return instance;
    }

    public AudioInstance PlayManagedEvent(string eventName, Vector2 globalPosition){
        AudioInstance instance = CreateAudioInstance(eventName, globalPosition, managed: true);
        instance.EventInstance.start();
        instance.EventInstance.release();
        return instance;
    }

    public AudioInstance PlayManagedEvent(string eventName, Vector3 globalPosition){
        AudioInstance instance = CreateAudioInstance(eventName, globalPosition, managed: true);
        instance.EventInstance.start();
        instance.EventInstance.release();
        return instance;
    }

    public AudioInstance PlayUnmanagedEvent(string eventName, Vector2 globalPosition){
        AudioInstance instance = CreateAudioInstance(eventName, globalPosition, managed: false);
        instance.EventInstance.start();
        return instance;
    }

    public AudioInstance PlayUnmanagedEvent(string eventName, Vector3 globalPosition){
        AudioInstance instance = CreateAudioInstance(eventName, globalPosition, managed: false);
        instance.EventInstance.start();
        return instance;
    }

    public AudioInstance PlayUnmanagedEvent(string eventName){
        AudioInstance instance = CreateAudioInstance(eventName, managed: false);
        instance.EventInstance.start();
        return instance;
    }


    /// 
    /// Audio Instance Creation.
    /// 

    private AudioInstance CreateAudioInstance(string eventName, bool managed){
        FMOD.Studio.EventDescription desc = _loadedEvents[eventName];
        desc.createInstance(out FMOD.Studio.EventInstance inst);
        return new AudioInstance(inst, eventName, managed);
    }

    private AudioInstance CreateAudioInstance(string eventName, Vector2 globalPosition, bool managed){
        return CreateAudioInstance(eventName, GodotToFmodVector(globalPosition), managed);
    }

    private AudioInstance CreateAudioInstance(string eventName, Vector3 globalPosition, bool managed){
        return CreateAudioInstance(eventName, GodotToFmodVector(globalPosition), managed);
    }

    private AudioInstance CreateAudioInstance(string eventName, FMOD.VECTOR globalPosition, bool managed){
        AudioInstance inst = CreateAudioInstance(eventName, managed);
        FMOD.ATTRIBUTES_3D attributes = new FMOD.ATTRIBUTES_3D{
            position = globalPosition,
            velocity = new FMOD.VECTOR { x = 0, y = 0, z = 0 },
            forward = new FMOD.VECTOR { x = 0, y = 0, z = 1 },
            up = new FMOD.VECTOR { x = 0, y = 1, z = 0 }
        };
        inst.EventInstance.set3DAttributes(attributes);
        return inst;
    }


    /// 
    /// Listener Handling.
    /// 


    public void SetListenerPosition(FMOD.VECTOR globalPosition){
        FMOD.ATTRIBUTES_3D listenerAttributes = new FMOD.ATTRIBUTES_3D{
            position = globalPosition,
            velocity = new FMOD.VECTOR { x = 0, y = 0, z = 0 },
            forward  = new FMOD.VECTOR { x = 0, y = 0, z = 1 },
            up       = new FMOD.VECTOR { x = 0, y = 1, z = 0 }
        };

        // Assuming you have a valid FMOD.Studio.System instance called studioSystem
        FMOD.RESULT result = StudioSystem.setListenerAttributes(0, listenerAttributes);
        if (result != FMOD.RESULT.OK){
            GD.PrintErr($"Failed to set listener attributes: {result}");
        }
        StudioSystem.update();
    }

    public void SetListenerPosition(Vector2 globalPosition){
        SetListenerPosition(GodotToFmodVector(globalPosition));
    }

    public void SetListenerPosition(Vector3 globalPosition){
        SetListenerPosition(GodotToFmodVector(globalPosition));
    }


    ///
    /// Godot Conversions. 
    /// 

    private FMOD.VECTOR GodotToFmodVector(Vector2 globalPosition){
        return new FMOD.VECTOR{
            x = globalPosition.X,
            y = 0,
            z = -globalPosition.Y
        };
    }

    private FMOD.VECTOR GodotToFmodVector(Vector3 globalPosition){
        return new FMOD.VECTOR{
            x = globalPosition.X,
            y = globalPosition.Y,
            z = globalPosition.Z
        };
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

}