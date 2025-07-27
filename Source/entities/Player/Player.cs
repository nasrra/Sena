using Godot;
using System;
using System.Collections.Generic;

public partial class Player : CharacterBody2D{
    public static Player Instance {get;private set;}

    [ExportGroup("Nodes")]
        
        // note: decay timer should be left on autostart in the editor.
        // do not change this.

    [Export] private Timer emberDecayRate;
    [Export] private Timer moveInputCooldown;
    [Export] private Timer dashCooldown;
    [Export] private Timer attackCooldown;
    
    [Export] private CameraController camera;
    [Export] private Area2D hurtBox;
    [Export] public CharacterMovement movement {get; private set;}
    [Export] public PlayerAimCursour aimCursour {get; private set;}
    [Export] public HitBoxHandler hitBoxes {get; private set;}
    [Export] public HitFlashShaderController hitFlash {get;private set;} 
    [Export] public Health Health {get; private set;}
    [Export] public EmberStorage EmberStorage {get; private set;}
    [Export] public Interactor Interactor {get; private set;}

    [ExportGroup("Variables")]
    [Export] private AnimatedSprite2D animator;
    private const float AttackLungeForce = 100f;
    private const float AttackEnemyKnockback = 100f;
    private const float AttackPlayerKnockback = 80f;
    private const float DashForce = 200f;
    
    private bool blockAttackInput = false;
    private bool blockMoveInput = false;
    private bool blockDashInput = false;


    /// 
    /// Base.
    /// 


    public override void _EnterTree(){
        base._EnterTree();
        #if TOOLS
        Entropek.Util.Node.VerifyName(this, nameof(Player));
        #endif        
        LoadPersistentData();
        LinkEvents();
        Instance = this;
    }

    public override void _Ready(){
        base._Ready();
        animator.Play("IdleForward");
        HandleLevelEnter();
        StartEmberDecayTimer();
    }

    public override void _ExitTree(){
        base._ExitTree();
        UnlinkEvents();
        StorePersistentData();
    }

    private void Process(double delta){
        UpdateAnimation();
    }


    /// 
    /// Functions.
    /// 


    private void UpdateAnimation(){
        float angle = movement.GetMoveAngle();
        if(movement.MoveDirection == Vector2.Zero){
            return;
        }
        if(angle >= -135 && angle <= -45){
            animator.Play("IdleForward");
            animator.FlipH = false;
            return;
        }
        else if(angle >= 45 && angle <= 135){
            animator.Play("IdleBackward");
            animator.FlipH = false;
            return;
        }
        else if(angle >= -45 && angle <= 45){
            animator.Play("IdleSide");
            animator.FlipH = true;
            return;
        }
        else{
            animator.Play("IdleSide");
            animator.FlipH = false;
            return;
        }
    }

    private void HandleLevelEnter(){
        
        // spawn at the exit point from the last door we entered.
        switch(GameManager.Instance.State){
            case GameState.Gameplay:
                // spawn at door.
                if(LevelSwapDoorManager.Instance.GetExitDoor(out LevelSwapDoor door)==true){
                    EntityManager.Instance.PauseEntityProcesses(0.33f);
                    GlobalPosition = door.ExitPoint.GlobalPosition;
                }                
                break;
            case GameState.Death:
                // spawn at respawn point.
                if(RespawnPoint.Instance != null){
                    GlobalPosition = RespawnPoint.Instance.GlobalPosition;
                }
                GameManager.Instance.GameplayState();
                break;
        }
    }

    private void LoadPersistentData(){        
        if(PlayerPersistence.Initialised == false || GameManager.Instance.State == GameState.Death){
            return;
        }

        Health.Initialise(
            PlayerPersistence.MaxHealthValue, 
            PlayerPersistence.HealthValue
        );
        EmberStorage.Initialise(
            PlayerPersistence.MaxNotchAmount,
            PlayerPersistence.EmberValue
        );
    }

    private void StorePersistentData(){
        PlayerPersistence.Initialised       = true;
        PlayerPersistence.MaxHealthValue    = Health.Max;
        PlayerPersistence.HealthValue       = Health.Value;
        PlayerPersistence.EmberValue        = EmberStorage.EmberValue;
        PlayerPersistence.MaxNotchAmount    = EmberStorage.MaxNotchAmount;
    }


    /// 
    /// Linkage
    /// 


    private void LinkEvents(){
        LinkHitbox();        
        LinkInput();
        LinkHurtBox();
        LinkEmberStorage();
        LinkHealth();
        LinkGui();
        LinkEntityManager();
    }

    private void UnlinkEvents(){
        UnlinkHitBox();        
        UnlinkInput();
        UnlinkHurtBox();
        UnlinkEmberStorage();
        UnlinkHealth();
        UnlinkGui();
        UnlinkEntityManager();
    }


    /// 
    /// Input Linkage.
    /// 


    private void LinkInput(){
        InputManager.Instance.OnAttackInput     += HandleAttackInput;
        InputManager.Instance.OnMovementInput   += HandleMovementInput;
        InputManager.Instance.OnHealInput       += HandleHealInput;
        InputManager.Instance.OnDashInput       += HandleDashInput;
        InputManager.Instance.OnInteractInput   += Interactor.Interact;
        moveInputCooldown.Timeout               += UnblockMoveInput;
        dashCooldown.Timeout                    += UnblockDashInput;
        attackCooldown.Timeout                  += UnblockAttackInput;
    }

    private void UnlinkInput(){
        InputManager.Instance.OnAttackInput     -= HandleAttackInput;
        InputManager.Instance.OnMovementInput   -= HandleMovementInput;
        InputManager.Instance.OnHealInput       -= HandleHealInput;
        InputManager.Instance.OnDashInput       -= HandleDashInput;
        InputManager.Instance.OnInteractInput   -= Interactor.Interact;
        moveInputCooldown.Timeout               -= UnblockMoveInput;
        dashCooldown.Timeout                    -= UnblockDashInput;
        attackCooldown.Timeout                  -= UnblockAttackInput;
    }

    private void HandleAttackInput(){
        
        if(blockAttackInput == true){
            return;
        }

        int hitBoxId;
        float angle = aimCursour.AimAngle;
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

        hitBoxes.EnableHitBox(hitBoxId, time: 0.167f);

        movement.ZeroDirection();
        movement.ZeroVelocity();
        movement.Impulse(aimCursour.AimDirection * AttackLungeForce);

        BlockMoveInput(time: 0.2f);
        BlockAttackInput(time: 0.2f);
    }

    private void BlockAttackInput(float time){
        blockAttackInput = true;
        attackCooldown.WaitTime = time;
        attackCooldown.Start();
    }

    private void UnblockAttackInput(){
        blockAttackInput = false;
    }

    private void HandleDashInput(){
        if(blockDashInput==true){
            return;
        }
        if(movement.MoveDirection.LengthSquared() > 0){
            movement.Impulse(movement.MoveDirection * DashForce);
            BlockDashInput(time: 1);
            BlockMoveInput(time: 0.2f);
        }
    }

    private void HandleMovementInput(Vector2 input){
        if(blockMoveInput == false){
            movement.Move(input);
        }
    }

    private void HandleHealInput(){
        if(EmberStorage.NotchAmount >= 1){
            EmberStorage.Remove(EmberStorage.NotchMaxEmberValue);
            Health.Heal(1);
        }
    }

    private void BlockMoveInput(float time){
        blockMoveInput = true;
        moveInputCooldown.WaitTime = time;
        moveInputCooldown.Start();
        movement.ZeroDirection();
    }

    private void UnblockMoveInput(){
        blockMoveInput = false;
    }

    private void BlockDashInput(float time){
        blockDashInput = true;
        dashCooldown.WaitTime = time;
        dashCooldown.Start();
    }

    private void UnblockDashInput(){
        blockDashInput = false;
    }


    /// 
    /// Hurtbox Linkage.
    /// 


    private void LinkHurtBox(){
        hurtBox.BodyEntered += HandleHurtBoxCollision;
        hurtBox.AreaEntered += HandleHurtBoxCollision;
    }

    private void UnlinkHurtBox(){
        hurtBox.BodyEntered -= HandleHurtBoxCollision;
        hurtBox.AreaEntered -= HandleHurtBoxCollision;
    }

    private void HandleHurtBoxCollision(Node2D node){
        if(PhysicsManager.Instance.GetPhysics2DLayerName((node as CollisionObject2D).CollisionLayer, out string layerName) == false){
            return;
        }
        switch(layerName){
            case "Enemy":
                Health.Damage(1);
            break;
        }
    }


    /// 
    /// Ember Storage Linkage.
    /// 


    private void LinkEmberStorage(){
        emberDecayRate.Timeout += DecayEmberStorage;
        EmberStorage.OnAdd += StartEmberDecayTimer;
    }

    private void UnlinkEmberStorage(){
        emberDecayRate.Timeout -= DecayEmberStorage;
        EmberStorage.OnAdd -= StartEmberDecayTimer;
    }

    private void DecayEmberStorage(){
        if(EmberStorage.RemoveRemainder(2)==true){
            StartEmberDecayTimer();
        }
    }

    private void StartEmberDecayTimer(){
        emberDecayRate.Start();
    }


    /// 
    /// Health Linkage.
    /// 


    private void LinkHealth(){
        Health.OnDamage += HandleDamaged;
        Health.OnDeath += HandleDeath;
    }

    private void UnlinkHealth(){
        Health.OnDamage -= HandleDamaged; 
        Health.OnDeath -= HandleDeath;
    }

    private void HandleDamaged(){
        hitFlash.Flash();
        camera.StartShake(20.0f, 0.33f);
        camera.Vignette.Update(0.33f,1f,0.01f);
        camera.Vignette.QueueUpdate(0,0,0.005f,1f);
        EntityManager.Instance.PauseEntityProcesses(time:0.25f);
        Health.SetInvincible(time:1f);
    }

    private void HandleDeath(){
        GameManager.Instance.DeathState();
        QueueFree();
    }


    /// 
    /// Gui Linkage.
    /// 


    private void LinkGui(){
        GameplayGui ui = (GameplayGui)GetNode("/root/Main/GUI/GameplayGui");
        Control hudGui = ui.HudGui;
        hudGui.GetNode<HealthHud>(HealthHud.NodeName).LinkEvents(Health);
        hudGui.GetNode<EmberNotchChainHud>(EmberNotchChainHud.NodeName).LinkToEmberStorage(EmberStorage);
    }

    private void UnlinkGui(){
        GameplayGui ui = (GameplayGui)GetNode("/root/Main/GUI/GameplayGui");
        Control hudGui = ui.HudGui;
        hudGui.GetNode<HealthHud>(HealthHud.NodeName).UnlinkEvents();
        hudGui.GetNode<EmberNotchChainHud>(EmberNotchChainHud.NodeName).UnlinkFromEmberStorage(EmberStorage);
    }


    /// 
    /// Entity Manager Linkage.
    /// 


    private void LinkEntityManager(){
        EntityManager.Instance.OnPause += HandlePause;
        EntityManager.Instance.OnResume += HandleResume;
        EntityManager.Instance.OnProcess += Process;
    }

    private void UnlinkEntityManager(){
        EntityManager.Instance.OnPause -= HandlePause;
        EntityManager.Instance.OnResume -= HandleResume;
        EntityManager.Instance.OnProcess -= Process;
    }

    private void HandlePause(){
        hitBoxes.PauseState();
        movement.PauseState();
        InputManager.Instance.PauseState();
    }

    private void HandleResume(){
        hitBoxes.ResumeState();
        movement.ResumeState();
        InputManager.Instance.ResumeState();
    }


    ///
    /// Hitbox Linkage.
    /// 


    private void LinkHitbox(){
        hitBoxes.OnHit += OnHitBoxHit;
    }

    private void UnlinkHitBox(){
        hitBoxes.OnHit -= OnHitBoxHit;
    }

    private void OnHitBoxHit(Node2D node, int id){

        // validate physics layer name.

        if(PhysicsManager.Instance.GetPhysics2DLayerName((node as CollisionObject2D).CollisionLayer, out string hitLayer)==false){
            return;
        }
        
        switch(hitLayer){
            case "Enemy":
                HandleOnHitEnemy((Enemy)node);
            break;
            case "HitInteractable":
                Interactable interactable = (Interactable)node;
                interactable.Interact(Interactor);
            break;
            default:
            throw new Exception($"{hitLayer} not implemented.");
        }
    }
    
    private void HandleOnHitEnemy(Enemy enemy){
        Vector2 directionToHit = (enemy.GlobalPosition - GlobalPosition).Normalized();
        
        float stunTime = 0.33f;
        enemy.StunState(stunTime);
        enemy.IgnoreEnemyCollisionMask(stunTime);
        
        enemy.GetNode<Health>(Health.NodeName).Damage(1);
        
        CharacterMovement enemyMovement = enemy.GetNode<CharacterMovement>(CharacterMovement.NodeName); 
        enemyMovement.ZeroVelocity();
        enemyMovement.Impulse(directionToHit * AttackEnemyKnockback);
        
        movement.Impulse(-directionToHit * AttackPlayerKnockback);

        EmberStorage.Add(50);
    }
}
