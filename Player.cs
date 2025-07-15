using Godot;
using System;

public partial class Player : CharacterBody2D{
    
    [ExportGroup("Nodes")]
    [Export] private CameraController camera;
    [Export] private CharacterMovement movement;
    [Export] private PlayerAimCursour aimCursour;
    [Export] private HitBoxHandler hitBoxes;
    [Export] private Health health;
    [Export] private EmberStorage emberStorage;
    [Export] private Timer moveInputBlockTimer;
    
    [ExportGroup("Variables")]
    private Vector2 moveInput = Vector2.Zero;
    [Export] private float swordSlashLungForce = 5f;
    [Export] private AnimatedSprite2D animator;
    private bool blockMoveInput = false;

    public override void _Ready(){
        base._Ready();
        #if DEBUG
        Entropek.Util.Node.VerifyName(this, nameof(Player));
        #endif
        animator.Play("IdleForward");
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
        if(Input.IsActionJustPressed("Debug1")){
            emberStorage.Add(2, out int remainder);
        }
        if(Input.IsActionJustPressed("Debug2")){
            emberStorage.Remove(2, out int remainder);
        }
        movement.Move(moveInput);
        UpdateAnimation();
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

    public void UpdateAnimation(){
        if(moveInput.Y < 0){
            animator.Play("IdleForward");
            animator.FlipH = false;
            return;
        }
        else if(moveInput.Y > 0){
            animator.Play("IdleBackward");
            animator.FlipH = false;
            return;
        }
        else{
            if(moveInput.X < 0){
                animator.Play("IdleSide");
                animator.FlipH = false;
            }
            else if(moveInput.X > 0){
                animator.Play("IdleSide");
                animator.FlipH = true;
            }
        }
    }

    public void BlockMoveInput(){
        blockMoveInput = true;
    }

    public void UnblockMoveInput(){
        blockMoveInput = false;
    }

    private void HitBoxHit(Node2D node, int id){
        string hitLayer = PhysicsManager.GetPhysics2DLayerName((node as CollisionObject2D).CollisionLayer);
        GD.Print(node.Name);
        switch(hitLayer){
            case "Enemy":
                Health health = (Health)node.GetNode(Health.NodeName);
                health.Damage(1);
            break;
            case "HitInteractable":
                Interactable interactable = (Interactable)node;
                interactable.Interact();
            break;
        }
    }

    private void LinkEvents(){
        hitBoxes.OnHit += HitBoxHit;
        moveInputBlockTimer.Timeout += UnblockMoveInput;
        Node ui = GetNode("/root/Main/GUI/GameplayUI");
        ui.GetNode<HealthHud>(HealthHud.NodeName).LinkEvents(health);
        ui.GetNode<EmberBarHud>(EmberBarHud.NodeName).LinkToEmberStorage(emberStorage);
    }

    private void UnlinkEvents(){
        hitBoxes.OnHit -= HitBoxHit;
        moveInputBlockTimer.Timeout -= UnblockMoveInput;
        Node ui = GetNode("/root/Main/GUI/GameplayUI");
        ui.GetNode<HealthHud>(HealthHud.NodeName).UnlinkEvents();
        ui.GetNode<EmberBarHud>(EmberBarHud.NodeName).UnlinkFromEmberStorage();
    }
}
