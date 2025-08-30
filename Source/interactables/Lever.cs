using Godot;
using System;

public partial class Lever : Node{
    
    public event Action OnToggledOn;
    public event Action OnToggledOff;
    public event Action OnLocked;
    public event Action OnUnlocked;
    [Export] public bool IsOn {get;private set;}
    [Export] public bool IsLocked {get;private set;}
    [Export] private EmberSignalEmitter emberSignals;
    [Export] private Interactable interactable;
    [Export] private Sprite3D sprite;
    [Export] private Texture2D unlockedOnSprite;
    [Export] private Texture2D unlockedOffSprite;
    [Export] private Texture2D lockedOnSprite;
    [Export] private Texture2D lockedOffSprite;


    ///
    /// Base.
    /// 


    public override void _Ready(){
        LinkEvents();
        base._Ready();
    }

    public override void _ExitTree(){
        UnlinkEvents();
        base._ExitTree();
    }


    /// 
    /// States.
    /// 

    public void SetOn(){
        IsOn = true;
        sprite.Texture = IsLocked==false? unlockedOnSprite : lockedOnSprite;
    }

    public void SetOff(){
        IsOn = false;
        sprite.Texture = IsLocked==false? unlockedOffSprite : lockedOffSprite;
    }

    public void ToggleOn(){
        SetOn();
        OnToggledOn?.Invoke();
    }

    public void ToggleOff(){
        SetOff();
        OnToggledOff?.Invoke();
    }

    public void ToggleFailed(){
        GD.Print("toggle failed");
    }

    public void Toggle(){
        if(IsLocked==true){
            ToggleFailed();
            return;
        }

        if(IsOn==true){
            ToggleOff();
        }
        else{
            ToggleOn();
        }
    }

    public void Unlock(){
        IsLocked = false;
        sprite.Texture = IsOn==true? unlockedOnSprite : unlockedOffSprite;
        OnUnlocked?.Invoke();
    }

    public void Lock(){
        IsLocked = true;
        sprite.Texture = IsOn==true? lockedOnSprite : lockedOffSprite;
        OnLocked?.Invoke();
    }


    /// 
    /// Linkage.
    /// 


    private void LinkEvents(){
        emberSignals.OnActivate      += Unlock;
        emberSignals.OnDeactivate    += Lock;
        interactable.OnInteract      += OnInteractCallback;
    }

    private void UnlinkEvents(){
        emberSignals.OnActivate      -= Unlock;
        emberSignals.OnDeactivate    -= Lock;
        interactable.OnInteract      -= OnInteractCallback;
    }

    private void OnInteractCallback(Interactor interactor){
        Toggle();
    }
}
