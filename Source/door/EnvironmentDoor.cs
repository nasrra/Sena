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
    }

    public override void _ExitTree(){
        base._ExitTree();
        UnlinkEvents();
    }


    public override void Open(){
        StartOpening();
    }

    private void StartOpening(){
        segments[0].Open();
        for(int i = 1; i < segments.Count; i++){
            segments[i].Open(i*segmentAsyncTime);
        }
    }

    private void FinishOpening(){
        base.Open();
        GD.Print("finished opening!");
    }

    public override void Close(){
        base.Close();
        // wayfindingObstacle.Enable();
        segments[0].Close();
        for(int i = 1; i < segments.Count; i++){
            segments[i].Close(i*segmentAsyncTime);
        }
    }

    private void LinkEvents(){
        segments[segments.Count-1].OnFinishedOpening    += FinishOpening;
    }

    private void UnlinkEvents(){
        segments[segments.Count-1].OnFinishedOpening    -= FinishOpening;
    }
}
