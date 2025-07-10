using Godot;
using System;

public partial class Player : CharacterBody2D{
    [Export] private CameraController camera;
    [Export] private CharacterMovement movement;
    [Export] private PlayerAimCursour aimCursour;
    [Export] private HitBoxHandler hitBoxes;
    [Export] private Timer moveInputBlockTimer;
    private Vector2 moveInput = Vector2.Zero;
    [Export] private float swordSlashLungForce = 5f;
    private bool blockMoveInput = false;

    public override void _Ready(){
        base._Ready();
        #if DEBUG
        Entropek.Util.Node.VerifyName(this, nameof(Player));
        #endif
    }

    public override void _EnterTree(){
        base._EnterTree();
        LinkEvents();
    }

    public override void _ExitTree(){
        base._ExitTree();
        UnlinkEvents();
    }

    public override void _PhysicsProcess(double delta){
        base._PhysicsProcess(delta);
        HandleMovementInput();
        // if(movementInput.LengthSquared() >= 1){
            // camera.FollowOffset = movementInput*50;
        // }
    }

    public override void _Process(double delta){
        base._Process(delta);
        if(Input.IsActionJustPressed("SwordSlash")){
            movement.Impulse(aimCursour.aimDirection * swordSlashLungForce);
            int hitBoxId;
            float angle = aimCursour.aimAngle;
            if(angle >= -135 && angle <= -45){
                hitBoxId = 0;
            }
            else if(angle >= -45 && angle <= 45){
                hitBoxId = 1;
            }
            else if(angle >= 45 && angle <= 135){
                hitBoxId = 2;
            }
            else{
                hitBoxId = 3;
            }
            moveInputBlockTimer.WaitTime = 0.167f;
            moveInputBlockTimer.Start();
            BlockMoveInput();
            hitBoxes.EnableHitBox(hitBoxId, 0.167f);
        }
        movement.Move(moveInput);
    }

    private void HandleMovementInput(){
        moveInput = Vector2.Zero;
        
        if(blockMoveInput == true){
            return;
        }

        if(Input.IsActionPressed("MoveUp")){
            moveInput.Y -= 1;
        }
        if(Input.IsActionPressed("MoveDown")){
            moveInput.Y += 1;
        }
        if(Input.IsActionPressed("MoveLeft")){
            moveInput.X -= 1;
        }
        if(Input.IsActionPressed("MoveRight")){
            moveInput.X += 1;
        }
    }

    public void BlockMoveInput(){
        blockMoveInput = true;
    }

    public void UnblockMoveInput(){
        blockMoveInput = false;
    }

    private void HitEnemy(Node node, int id){
        GD.Print($"Hit! {id}");
        Health health = (Health)node.GetNode(Health.NodeName);
        if(health != null){
            health.Damage(1);
        }
    }

    private void LinkEvents(){
        hitBoxes.OnHit += HitEnemy;
        moveInputBlockTimer.Timeout += UnblockMoveInput;
    }

    private void UnlinkEvents(){
        hitBoxes.OnHit -= HitEnemy;
        moveInputBlockTimer.Timeout -= UnblockMoveInput;
    }
}
