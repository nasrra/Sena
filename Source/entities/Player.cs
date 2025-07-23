using Godot;
using System;

public partial class Player : CharacterBody2D{
    public static Player Instance {get;private set;}

    [ExportGroup("Nodes")]
    [Export] private Timer moveInputBlockTimer;
    [Export] private Timer emberDecayTimer;
    [Export] private CameraController camera;
    [Export] public CharacterMovement movement {get; private set;}
    [Export] public PlayerAimCursour aimCursour {get; private set;}
    [Export] public HitBoxHandler hitBoxes {get; private set;}
    [Export] public HitFlashShaderController hitFlash {get;private set;} 
    [Export] public Health health {get; private set;}
    [Export] public EmberStorage EmberStorage {get; private set;}
    [Export] public Interactor Interactor {get; private set;}
    
    [ExportGroup("Variables")]
    [Export] private AnimatedSprite2D animator;
    private Vector2 moveInput = Vector2.Zero;
    private float attackLungeForce = 100f;
    private float attackEnemyKnockback = 100f;
    private float attackPlayerKnockback = 80f;
    private bool blockMoveInput = false;


    /// 
    /// Base.
    /// 


    public override void _Ready(){
        base._Ready();
        #if TOOLS
        Entropek.Util.Node.VerifyName(this, nameof(Player));
        #endif
        animator.Play("IdleForward");
    }

    public override void _EnterTree(){
        base._EnterTree();
        LinkEvents();
        Instance = this;
    }

    public override void _ExitTree(){
        base._ExitTree();
        UnlinkEvents();
    }

    public void PhysicsProcess(double delta){
        HandleMovementInput();
    }

    private void Process(double delta){
        HandleAttackInput();
        HandleHealInput();
        HandleInteractInput();
        movement.Move(moveInput);
        UpdateAnimation();
    }


    /// <summary>
    /// Miscellaneous
    /// </summary>

    private void UpdateAnimation(){
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

    private void DecayEmberStorage(){
        EmberStorage.Remove(1, out int remainder);
        if(remainder == 0){
            StartEmberDecayTimer();
        }
    }

    private void StartEmberDecayTimer(){
        emberDecayTimer.Start();
    }


    /// 
    /// Input
    /// 


    private void HandleAttackInput(){

        if(Input.IsActionJustPressed("SwordSlash") == false){
            return;
        }

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
        BlockMoveInput();
        hitBoxes.EnableHitBox(hitBoxId, 0.167f);
        movement.Impulse(aimCursour.aimDirection * attackLungeForce);
        movement.ZeroDirection();
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

    private void HandleHealInput(){
        if(Input.IsActionJustPressed("Heal")){
            if(EmberStorage.Value >= 20){
                EmberStorage.Remove(20, out int remainder);
                health.Heal(1);
            }
        }
    }

    private void HandleInteractInput(){
        if(Input.IsActionJustPressed("Interact")){
            Interactor.Interact();
        }
    }

    public void BlockMoveInput(){
        blockMoveInput = true;
        moveInputBlockTimer.WaitTime = 0.167f;
        moveInputBlockTimer.Start();
    }

    public void UnblockMoveInput(){
        blockMoveInput = false;
    }

    private void HandleOnHitEnemy(Enemy enemy){
        Vector2 directionToHit = (enemy.GlobalPosition - GlobalPosition).Normalized();
        
        float stunTime = 0.33f;
        enemy.StunState(stunTime);
        enemy.IgnoreEnemyCollisionMask(stunTime);
        
        enemy.GetNode<Health>(Health.NodeName).Damage(1);
        
        CharacterMovement enemyMovement = enemy.GetNode<CharacterMovement>(CharacterMovement.NodeName); 
        enemyMovement.ZeroVelocity();
        enemyMovement.Impulse(directionToHit * attackEnemyKnockback);
        
        movement.Impulse(-directionToHit * attackPlayerKnockback);

        EmberStorage.Add(10, out int remainder);
    }


    /// 
    /// Linkage
    /// 


    private void LinkEvents(){
        
        hitBoxes.OnHit += OnHitBoxHit;
        
        moveInputBlockTimer.Timeout += UnblockMoveInput;
        emberDecayTimer.Timeout += DecayEmberStorage;
        EmberStorage.OnAdd += StartEmberDecayTimer;

        health.OnDamage += OnDamaged; 

        Node ui = GetNode("/root/Main/GUI/GameplayUI");
        ui.GetNode<HealthHud>(HealthHud.NodeName).LinkEvents(health);
        ui.GetNode<EmberBarHud>(EmberBarHud.NodeName).LinkToEmberStorage(EmberStorage);

        EntityManager.Instance.LinkToPause(OnPaused);
        EntityManager.Instance.LinkToResume(OnResume);
        EntityManager.Instance.LinkToProcess(Process);
        EntityManager.Instance.LinkToPhysicsProcess(PhysicsProcess);
    }

    private void UnlinkEvents(){
        
        hitBoxes.OnHit -= OnHitBoxHit;
        
        moveInputBlockTimer.Timeout -= UnblockMoveInput;
        emberDecayTimer.Timeout -= DecayEmberStorage;
        EmberStorage.OnAdd -= StartEmberDecayTimer;

        health.OnDamage -= OnDamaged; 

        Node ui = GetNode("/root/Main/GUI/GameplayUI");
        ui.GetNode<HealthHud>(HealthHud.NodeName).UnlinkEvents();
        ui.GetNode<EmberBarHud>(EmberBarHud.NodeName).UnlinkFromEmberStorage();
        
        EntityManager.Instance.UnlinkFromProcess(Process);
        EntityManager.Instance.UnlinkFromPhysicsProcess(PhysicsProcess);
    }


    ///
    /// Linkage functions.
    /// 


    private void OnHitBoxHit(Node2D node, int id){
        if(PhysicsManager.Instance.GetPhysics2DLayerName((node as CollisionObject2D).CollisionLayer, out string hitLayer)==false){
            return;
        }
        
        switch(hitLayer){
            case "Enemy":
                HandleOnHitEnemy((Enemy)node);
            break;
            case "HitInteractable":
                Interactable interactable = (Interactable)node;
                interactable.Interact();
            break;
            default:
            throw new Exception($"{hitLayer} not implemented.");
        }
    }

    private void OnDamaged(){
        hitFlash.Flash();
        camera.StartShake(20.0f, 0.33f);
        camera.Vignette.Update(0.33f,0.5f,0.005f);
        camera.Vignette.QueueUpdate(0,0,0.005f,1f);
        EntityManager.Instance.PauseEntityProcesses(time:0.25f);
    }

    private void OnPaused(){
        hitBoxes.PauseState();
        movement.PauseState();
    }

    private void OnResume(){
        hitBoxes.ResumeState();
        movement.ResumeState();
    }
}
