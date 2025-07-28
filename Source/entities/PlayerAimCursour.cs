using Godot;
using System;

public partial class PlayerAimCursour : Node2D{

    [Export] public Node2D Cursour {get;private set;}
    [Export] public Vector2 AimDirection {get;private set;} = Vector2.Zero;
    [Export] public float CursourDistance {get; private set;} = 2;
    [Export] public float AimAngle {get; private set;} = 0f;


    /// 
    /// Base.
    /// 


    public override void _EnterTree(){
        base._EnterTree();
        LinkEvents();
    }

    public override void _ExitTree(){
        base._ExitTree();
        UnlinkEvents();
    }


    /// 
    /// Functions.
    /// 


    public override void _Process(double delta){
        base._PhysicsProcess(delta);
        AimAngle = Mathf.Atan2(AimDirection.Y, AimDirection.X);
        AimAngle = Mathf.RadToDeg(AimAngle);
        Cursour.GlobalPosition = GlobalPosition+AimDirection * CursourDistance;
    }

    private void HandleAimDirection(Vector2 direction){
        if(InputManager.Singleton.IsGamepadConnected == true){
            AimDirection = direction.Normalized();
        }else{
            AimDirection = (direction-GlobalPosition).Normalized();
        }
    }


    /// 
    /// Linkage.
    /// 


    private void LinkEvents(){
        InputManager.Singleton.OnAimInput += HandleAimDirection;
    }

    private void UnlinkEvents(){
        InputManager.Singleton.OnAimInput -= HandleAimDirection;
    }
}

