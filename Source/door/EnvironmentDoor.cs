using System;
using Entropek.Ai;
using Godot;

public partial class EnvironmentDoor : Door{
    [ExportGroup(nameof(EnvironmentDoor))]
    [Export] private AudioPlayer audioPlayer;
    [Export] private Godot.Collections.Array<SegmentedDoorPiece> segments = new Godot.Collections.Array<SegmentedDoorPiece>();
    [Export] private string transitionSound;
    [Export] private float segmentLiftSpeed     = 1.0f;
    [Export] private float segmentLowerSpeed    = 1.0f;
    [Export] private float segmentAsyncTime     = 0.165f;
    [Export] private bool transitionSoundOneShot;

    public override void _Ready(){
        LinkEvents();
        base._Ready();
        // Open();
    }

    public override void _ExitTree(){
        UnlinkEvents();
        base._ExitTree();
    }

    protected override void Opened(){
        for(int i = 0; i < segments.Count; i++){
            segments[i].Opened();
        }
        audioPlayer.StopSound(transitionSound, false);
        base.Opened();
    }

    protected override void Closed(){
        for(int i = 0; i < segments.Count; i++){
            segments[i].Closed();
        }
        audioPlayer.StopSound(transitionSound, false);
        base.Closed();
    }

    public override void Open(){
        for(int i = 0; i < segments.Count; i++){
            segments[i].Open((segments.Count-i)*segmentAsyncTime);
        }
        audioPlayer.StopSound(transitionSound, false);
        audioPlayer.PlaySound(transitionSound, GlobalPosition, transitionSoundOneShot);
    }

    public override void Close(){
        // wayfindingObstacle.Enable();
        EnableCollider();
        segments[0].Close();
        for(int i = 1; i < segments.Count; i++){
            segments[i].Close(i*segmentAsyncTime);
        }
        audioPlayer.StopSound(transitionSound, false);
        audioPlayer.PlaySound(transitionSound, GlobalPosition, transitionSoundOneShot);
    }

    public override void Lock(){
        Locked();
    }

    public override void Unlock(){
        Unlocked();
    }

    private void LinkEvents(){
        segments[0].OnOpenCompleted                 += Opened;
        segments[segments.Count-1].OnCloseCompleted += Closed;
    }

    private void UnlinkEvents(){
        segments[0].OnOpenCompleted                 -= Opened;
        segments[segments.Count-1].OnCloseCompleted -= Closed;
    }
}
