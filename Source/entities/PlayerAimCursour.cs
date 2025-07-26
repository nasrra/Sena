using Godot;
using System;

public partial class PlayerAimCursour : Node2D{

    [Export] public Node2D cursour {get;private set;}
    [Export] public Vector2 aimDirection {get;private set;} = Vector2.Zero;
    [Export] public float cursourDistance {get; private set;} = 2;
    [Export] public float aimAngle {get; private set;} = 0f;


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
        aimAngle = Mathf.Atan2(aimDirection.Y, aimDirection.X);
        aimAngle = Mathf.RadToDeg(aimAngle);
        cursour.GlobalPosition = GlobalPosition+aimDirection * cursourDistance;
    }

    private void HandleAimDirection(Vector2 direction){
        if(InputManager.Instance.IsGamepad == true){
            aimDirection = direction.Normalized();
        }else{
            aimDirection = (direction-GlobalPosition).Normalized();
        }
    }


    /// 
    /// Linkage.
    /// 


    private void LinkEvents(){
        InputManager.Instance.OnAimInput += HandleAimDirection;
    }

    private void UnlinkEvents(){
        InputManager.Instance.OnAimInput -= HandleAimDirection;
    }
}

