using Entropek.Ai;
using Godot;
using System;
using System.Collections.Generic;

public partial class Player : CharacterBody2D{
	public static Player Instance {get;private set;}

	[ExportGroup("Nodes")]
		
		// note: decay timer should be left on autostart in the editor.
		// do not change this.

	[Export] private Timer emberDecayRate;
	[Export] private Timer evaluateStateTimer;
	
	[Export] private CameraController camera;
	[Export] private Area2D hurtBox;
	[Export] private AnimatedSprite2D animator;
	[Export] public CharacterMovement movement {get; private set;}
	[Export] public PlayerAimCursour aimCursour {get; private set;}
	[Export] public ProjectileSpawner projectileSpawner {get; private set;}
	[Export] public HitBoxHandler hitBoxes {get; private set;}
	[Export] public HitFlashShaderController hitFlash {get;private set;} 
	[Export] public Health Health {get; private set;}
	[Export] public EmberStorage EmberStorage {get; private set;}
	[Export] public Interactor Interactor {get; private set;}

	private event Action<double> statePhysicsProcess = null;

	[ExportGroup("Variables")]
	private Vector2I lastSafeTile = Vector2I.Zero;
	private const float AttackLungeForce = 100f;
	private const float AttackEnemyKnockback = 100f;
	private const float AttackPlayerKnockback = 80f;
	private const float DashForce = 450f;
	private PlayerState state = PlayerState.Standby;

	///
	/// Definitions.
	/// 


	private enum PlayerState : byte{
		Standby,
		Attack,
		Dashing,
		Evaluating
	}


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

	private void PhysicsProcess(double delta){
		statePhysicsProcess?.Invoke(delta);
	}


	///
	/// States.
	/// 


	private async void EvaluateState(){
		if (state == PlayerState.Dashing){
			movement.Deceleration = 800f;
			hurtBox.SetCollisionMaskValue(PhysicsManager.Singleton.GetPhysics2DLayerId("Pitfall"), true);
		}

		state = PlayerState.Evaluating;

		GD.Print("start eval");

		await ToSignal(GetTree(), "physics_frame"); // Wait one full physics frame
		CheckHurtCollider();
		await ToSignal(GetTree(), "physics_frame"); // Wait one full physics frame
		StandbyState(); // You can call this right after if needed
		GD.Print("end eval");
	}

	private void StandbyState(){
		
		state = PlayerState.Standby;
		statePhysicsProcess = StandbyStatePhysicsProcess;

	}

	private void StandbyStatePhysicsProcess(double delta){
		CheckPositionSafety();
	}

	private void DashState(){
		if(movement.MoveDirection.LengthSquared() <= 0){
			return;
		}
		
		state = PlayerState.Dashing;
		statePhysicsProcess = DashStatePhysicsProcess;

		hurtBox.SetCollisionMaskValue(PhysicsManager.Singleton.GetPhysics2DLayerId("Pitfall"), false);
		movement.ZeroVelocity();
		movement.Impulse(movement.MoveDirection * DashForce);
		movement.ZeroDirection(); // <-- here so Deceleration is applied.
		movement.Deceleration = 1150f;
		Health.SetInvincible(time:0.4f);
		evaluateStateTimer.Start(timeSec:0.4f); 
		InputManager.Singleton.BlockMovementInput(time: 0.4f);
		InputManager.Singleton.BlockAttackInput(time:0.4f);
		InputManager.Singleton.BlockDashInput(time: 1);
	}

	private void DashStatePhysicsProcess(double delta){
		CheckPositionSafety();
	}


	/// 
	/// Functions.
	/// 


	private void UpdateAnimation(){
		float angle = movement.GetMoveAngleDegrees();
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
		StandbyState();
		
		switch(GameManager.Instance.State){
			case GameState.Gameplay:
				// spawn at door.
				if(LevelSwapDoorManager.Instance.GetExitDoor(out LevelSwapDoor door)==true){
					EntityManager.Singleton.PauseEntityProcesses(0.33f);
					GlobalPosition = door.ExitPoint.GlobalPosition;
				}     
				InputManager.Singleton.ResumeGameplayInput();           
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

	private void CheckHurtCollider(){
		// Force update
		var shape = hurtBox.GetNode<CollisionShape2D>("CollisionShape2D");
		shape.Disabled = true;
		shape.Disabled = false;
	}

	private void CheckPositionSafety(){
		Vector2I currentCell = WayfindingGrid2D.Singleton.GlobalToIdPosition(GlobalPosition);
		if(WayfindingGrid2D.Singleton.IsCellNavigationType(currentCell, NavigationType.Open)){
			lastSafeTile = currentCell;
		}
	}

	private void ReturnToLastSafePosition(){
		GlobalPosition = WayfindingGrid2D.Singleton.IdToGlobalPosition(lastSafeTile);
	}

	/// 
	/// Linkage
	/// 


	private void LinkEvents(){
		evaluateStateTimer.Timeout += EvaluateState;
		LinkHitbox();        
		LinkInput();
		LinkHurtBox();
		LinkEmberStorage();
		LinkHealth();
		LinkGui();
		LinkEntityManager();
	}

	private void UnlinkEvents(){
		evaluateStateTimer.Timeout -= EvaluateState;
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
		InputManager.Singleton.OnAttackInput     += HandleAttackInput;
		InputManager.Singleton.OnMovementInput   += HandleMovementInput;
		InputManager.Singleton.OnHealInput       += HandleHealInput;
		InputManager.Singleton.OnDashInput       += DashState;
		InputManager.Singleton.OnShootInput      += HandleShootInput;
		InputManager.Singleton.OnInteractInput   += Interactor.Interact;
	}

	private void UnlinkInput(){
		InputManager.Singleton.OnAttackInput     -= HandleAttackInput;
		InputManager.Singleton.OnMovementInput   -= HandleMovementInput;
		InputManager.Singleton.OnHealInput       -= HandleHealInput;
		InputManager.Singleton.OnDashInput       -= DashState;
		InputManager.Singleton.OnShootInput      -= HandleShootInput;
		InputManager.Singleton.OnInteractInput   -= Interactor.Interact;
	}

	private void HandleAttackInput(){
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

		AudioManager.Singleton.PlayEvent("PlayerAttack");

		InputManager.Singleton.BlockMovementInput(time: 0.2f);
		InputManager.Singleton.BlockAttackInput(time: 0.2f);
	}

	private void HandleMovementInput(Vector2 input){
		movement.Move(input);
	}

	private void HandleHealInput(){
		if(EmberStorage.NotchAmount >= 1){
			EmberStorage.Remove(EmberStorage.NotchMaxEmberValue);
			Health.Heal(1);
		}
	}

	private void HandleShootInput(){
		if(EmberStorage.NotchAmount >= 1){
			Vector2 shootDirection = (aimCursour.Cursour.GlobalPosition - GlobalPosition).Normalized();
			projectileSpawner.Fire(shootDirection, 10);
			EmberStorage.Remove(EmberStorage.NotchMaxEmberValue);
		}
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
		switch(PhysicsManager.Singleton.GetPhysics2DLayerName((node as CollisionObject2D).CollisionLayer)){
			case "Enemy":
				Health.Damage(1);
			break;
			case "Pitfall":
				Health.Damage(1);
				ReturnToLastSafePosition();
				GD.Print("pitfall");
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
		Health.SetInvincible(time:1f);
		EntityManager.Singleton.PauseEntityProcesses(time:0.25f);
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
		EntityManager.Singleton.OnPause += HandlePause;
		EntityManager.Singleton.OnResume += HandleResume;
		EntityManager.Singleton.OnProcess += Process;
		EntityManager.Singleton.OnPhysicsProcess += PhysicsProcess;
	}

	private void UnlinkEntityManager(){
		EntityManager.Singleton.OnPause -= HandlePause;
		EntityManager.Singleton.OnResume -= HandleResume;
		EntityManager.Singleton.OnProcess -= Process;
		EntityManager.Singleton.OnPhysicsProcess -= PhysicsProcess;
	}

	private void HandlePause(){
		hitBoxes.PauseState();
		movement.PauseState();
		Health.PauseState();
		animator.SpeedScale = 0; // pause animator.
		InputManager.Singleton.PauseGameplayInput();
	}

	private void HandleResume(){
		hitBoxes.ResumeState();
		movement.ResumeState();
		Health.ResumeState();
		animator.SpeedScale = 1; // resume animator.
		InputManager.Singleton.ResumeGameplayInput();
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
		
		string layer = PhysicsManager.Singleton.GetPhysics2DLayerName((node as CollisionObject2D).CollisionLayer);
		switch(layer){
			case "Enemy":
				HandleOnHitEnemy((Enemy)node);
			break;
			case "HitInteractable":
				Interactable interactable = (Interactable)node;
				interactable.Interact(Interactor);
			break;
			default:
			throw new Exception($"{layer} not implemented.");
		}
	}
	
	private void HandleOnHitEnemy(Enemy enemy){
		Vector2 directionToHit = (enemy.GlobalPosition - GlobalPosition).Normalized();
		enemy.GetNode<CharacterMovement>(CharacterMovement.NodeName).Knockback(directionToHit * AttackEnemyKnockback); 
		enemy.GetNode<Health>(Health.NodeName).Damage(1);
		EmberStorage.Add(50);
	}
}
