using System;
using Entropek.Ai;
using Godot;

public partial class EnvironmentDoor : Door{
    [ExportGroup(nameof(EnvironmentDoor))]
    [Export] private Godot.Collections.Array<SegmentedDoorPiece> segments;
    [Export] private float segmentLiftSpeed = 1.0f;
    [Export] private float segmentLowerSpeed = 1.0f;
    [Export] private float segmentAsyncTime = 0.165f;

    public override void _Ready(){
        base._Ready();
        LinkEvents();
        Open();
    }

    public override void _ExitTree(){
        base._ExitTree();
        UnlinkEvents();
    }

    protected override void Opened(){
        base.Opened();
        for(int i = 0; i < segments.Count - 1; i++){
            segments[i].Opened();
        }
    }

    protected override void Closed(){
        base.Closed();
        for(int i = 0; i < segments.Count - 1; i++){
            segments[i].Closed();
        }
    }

    public override void Open(){
        for(int i = 0; i < segments.Count; i++){
            segments[i].Open((segments.Count-i)*segmentAsyncTime);
        }
    }

    public override void Close(){
        // wayfindingObstacle.Enable();
        EnableCollider();
        segments[0].Close();
        for(int i = 1; i < segments.Count; i++){
            segments[i].Close(i*segmentAsyncTime);
        }
    }

    public override void Lock(){
        Locked();
    }

    public override void Unlock(){
        Unlocked();
    }

    private void LinkEvents(){
        segments[0].OnOpenCompleted      += Opened;
        segments[segments.Count-1].OnCloseCompleted                    += Closed;
    }

    private void UnlinkEvents(){
        segments[0].OnOpenCompleted      -= Opened;
        segments[segments.Count-1].OnCloseCompleted                    -= Closed;
    }
}
