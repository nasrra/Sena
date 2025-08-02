using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization.Metadata;

public partial class Enemy : CharacterBody2D{ // <-- make sure to inherit from CollisionObect2D for hitbox handler and Player.
	
	[ExportGroup("Nodes")]
	[Export] private Health health;
	[Export] private WayfindingAgent2D navAgent;
	[Export] private CharacterMovement characterMovement;
	[Export] private AiAttackHandler attackHandler;
	[Export] private HitBoxHandler hitBoxHandler;
	[Export] private HitFlashShaderController hitFlash;
	[Export] private Timer stunTimer;
	[Export] private Timer ignoreEnemyTimer;
	[Export] public Node2D Target;
	[Export] private AnimatedSprite2D animator;
	[Export] private AudioPlayer audioPlayer;
	[Export] private AgressionZone agressionZone;

	private event Action<double> Process = null;
	private event Action<double> PhysicsProcess = null;

	[ExportGroup("Variabales")]
	private Vector2 directionToTarget = Vector2.Zero;
	private Vector2 normalDirectionToTarget = Vector2.Zero;
	private float distanceToTarget = float.MaxValue;
	[Export] private float damagedKnockback = 100f;
	[Export] public float stunStateAttackHandlerStandbyAdditiveTime = 1.0f;
	private EnemyState state = EnemyState.Chase;
	[Export] public bool stunOnHit = true;


	/// 
	/// Definitions.
	/// 


	private enum EnemyState : byte{
		Idle,
		Chase,
		Stunned,
		Attacking,
	}

	private enum AttackId : byte{
		Slash    = 0,
	}

	private enum AttackHitBoxId{
		SlashDown   = 0,
		SlashLeft   = 1,
		SlashRight  = 2,
		SlashUp     = 3,
	}


	///
	/// Base
	///


	public override void _Ready(){
		base._Ready();
		EnemyManager.Instance.AddEnemy(this);
		IdleState();
	}

	public override void _EnterTree(){
		base._EnterTree();
		LinkEvents();
	}

	public override void _ExitTree(){
		base._ExitTree();
		UnlinkEvents();
	}

	private void InvokeProcess(double delta) => Process?.Invoke(delta);
	private void InvokePhysicsProcess(double delta) => PhysicsProcess?.Invoke(delta);


	/// 
	/// State Machine
	/// 


	private void TargetLeft(Node2D node){
		Target = null;
		characterMovement.ZeroDirection();
		audioPlayer.StopSound("WorkerAttack");
		attackHandler.HaltState();
		hitBoxHandler.DisableAllHitBoxes();
		IdleState();
	}

	private void EvaluateState(){

		// TODO: do some recovery state code when needed.
		if(Target == null){
			IdleState();
		}
		else if(attackHandler.IsAttacking == false){
			ChaseState();
		}
		attackHandler.EvaluateState();
	}

	private void IdleState(){
		state 			= EnemyState.Idle;
		Process 		= null;
		PhysicsProcess 	= null;
		animator.Play("IdleBackward");
	}

	private void ChaseState(Node2D target){
		SetTarget(target);
		ChaseState();
	}

	public void ChaseState(){
		Process        	= null;
		PhysicsProcess  = ChaseStatePhysicsProcess;
		
		// update data relevant to this frames.

		CalculateRelationshipToTarget();
		attackHandler.SetDirectionToTarget(directionToTarget);
		attackHandler.SetDistanceToTarget(distanceToTarget);

		attackHandler.ResumeState();
	}

	private void ChaseStatePhysicsProcess(double delta){
		if(IsInstanceValid(Target) && Target.IsInsideTree()==true){
			CalculateRelationshipToTarget();
			UpdateAttackHandler();
			MoveAlongPathToTarget();
			RunAnimation();
		}
	}

	public void StunState(float time){
		
		state 			= EnemyState.Stunned;
		Process        	= null;
		PhysicsProcess  = null;
		attackHandler.HaltState(time+stunStateAttackHandlerStandbyAdditiveTime);
		hitBoxHandler.DisableAllHitBoxes();

		audioPlayer.StopSound("WorkerAttack", immediate: true);

		float angle = characterMovement.GetVelocityAngleDegrees();
		if(angle > -135 && angle < -45){
			animator.Play("HitBackward");
			animator.FlipH = false;
		}
		else if(angle > 45 && angle < 135){
			animator.Play("HitForward");
			animator.FlipH = false;
		}
		else if(angle > -45 && angle < 45){
			animator.Play("HitSide");
			animator.FlipH = false;
		}
		else{
			animator.Play("HitSide");
			animator.FlipH = true;
		}

		stunTimer.WaitTime = time;
		stunTimer.Start();
	}

	private void AttackingState(){
		
		state = EnemyState.Attacking;

		Process = null;
		PhysicsProcess = null;
	}

	private void PauseState(){
		Process = null;
		PhysicsProcess = null;
		attackHandler.PauseState();
		characterMovement.PauseState();
		audioPlayer.PauseState();
		hitBoxHandler.PauseState();
		agressionZone.PauseState();
		animator.SpeedScale = 0; // pause animator.
	}

	private void ResumeState(){
		attackHandler.ResumeState();
		characterMovement.ResumeState();
		audioPlayer.ResumeState();
		animator.SpeedScale = 1; // resume animator.
		hitBoxHandler.ResumeState();
		agressionZone.ResumeState();
		EvaluateState();
	}


	/// 
	/// Shared Functions
	/// 


	private void MoveAlongPathToTarget(){
		navAgent.CalculateNewPath(Target.GlobalPosition);
		navAgent.UpdateCurrentPathToTarget();
		characterMovement.Move(navAgent.CurrentPathPoint - GlobalPosition);
	}

	private void CalculateRelationshipToTarget(){
		directionToTarget = Target.GlobalPosition- GlobalPosition;
		normalDirectionToTarget = directionToTarget.Normalized();
		distanceToTarget = directionToTarget.Length();
	}

	private void UpdateAttackHandler(){
		attackHandler.SetDirectionToTarget(directionToTarget);
		attackHandler.SetDistanceToTarget(distanceToTarget);
	}

	public void IgnoreEnemyCollisionMask(float time){
		
		ignoreEnemyTimer.WaitTime = time;
		SetCollisionMaskValue(PhysicsManager.Singleton.GetPhysics2DLayerId("Enemy"), false);
		ignoreEnemyTimer.Start();
	}

	private void RespondToEnemyCollisionMask(){
		SetCollisionMaskValue(PhysicsManager.Singleton.GetPhysics2DLayerId("Enemy"), true);
	}

	private void RunAnimation(){
		float angle = characterMovement.GetMoveAngleDegrees();
		if(characterMovement.MoveDirection == Vector2.Zero){
			return;
		}
		if(angle >= -155 && angle <= -25){
			animator.Play("RunForward");
			animator.FlipH = false;
			return;
		}
		else if(angle >= 25 && angle <= 155){
			animator.Play("RunBackward");
			animator.FlipH = false;
			return;
		}
		else if(angle > -45 && angle < 45){
			animator.Play("RunSide");
			animator.FlipH = true;
			return;
		}
		else{
			animator.Play("RunSide");
			animator.FlipH = false;
			return;
		}
	}

	public void SetTarget(Node2D target){
		Target = target;
	}

	/// 
	/// Linkage
	/// 


	private void LinkEvents(){
		LinkEntityManager();
		LinkAnimator();
		LinkHealth();
		LinkAttackHandler();
		LinkHitBoxHandler();
		LinkTimers();
		LinkAgressionZone();
	}

	private void UnlinkEvents(){
		UnlinkEntityManager();
		UnlinkAnimator();
		UnlinkHealth();
		UnlinkAttackHandler();
		UnlinkHitBoxHandler();
		UnlinkTimers();
		UnlinkAgressionZone();
	}

	private void LinkEntityManager(){
		EntityManager.Singleton.OnProcess 			+= InvokeProcess;
		EntityManager.Singleton.OnPhysicsProcess 	+= InvokePhysicsProcess;
		EntityManager.Singleton.OnPause 			+= PauseState;
		EntityManager.Singleton.OnResume 			+= ResumeState;
	}

	private void UnlinkEntityManager(){
		EntityManager.Singleton.OnProcess 			-= InvokeProcess;
		EntityManager.Singleton.OnPhysicsProcess 	-= InvokePhysicsProcess;
		EntityManager.Singleton.OnPause 			-= PauseState;
		EntityManager.Singleton.OnResume 			-= ResumeState;
	}

	private void LinkAnimator(){
		animator.FrameChanged 		+= OnFrameChanged;
		animator.AnimationChanged 	+= OnAnimationChanged;
	}

	private void UnlinkAnimator(){
		animator.FrameChanged 		-= OnFrameChanged;
		animator.AnimationChanged 	-= OnAnimationChanged;
	}

	private void LinkHealth(){
		health.OnDeath  += Kill;
		health.OnDamage += OnDamaged;
	}

	private void UnlinkHealth(){
		health.OnDeath  -= Kill;
		health.OnDamage -= OnDamaged;
	}

	private void LinkAttackHandler(){
		attackHandler.OnAttackChosen	+= HandleAttackChosen;
		attackHandler.OnAttack          += HandleAttack;
		attackHandler.OnAttackStarted   += HandleStartAttack;
		attackHandler.OnAttackEnded     += HandleAttackEnded;
	}

	private void UnlinkAttackHandler(){
		attackHandler.OnAttackChosen	-= HandleAttackChosen;
		attackHandler.OnAttack          -= HandleAttack;
		attackHandler.OnAttackStarted   -= HandleStartAttack;
		attackHandler.OnAttackEnded     -= HandleAttackEnded;
	}

	private void LinkHitBoxHandler(){
		hitBoxHandler.OnHit += HandleAttackHit;
	}

	private void UnlinkHitBoxHandler(){
		hitBoxHandler.OnHit -= HandleAttackHit;
	}

	private void LinkTimers(){
		stunTimer.Timeout 			+= EvaluateState;
		ignoreEnemyTimer.Timeout 	+= RespondToEnemyCollisionMask;
	}

	private void UnlinkTimers(){
		stunTimer.Timeout 			-= EvaluateState;
		ignoreEnemyTimer.Timeout 	-= RespondToEnemyCollisionMask;	
	}

	private void LinkAgressionZone(){
		agressionZone.OnInSight 	+= SetTarget;
		agressionZone.OnEnteredZone += ChaseState;
		agressionZone.OnExitedZone 	+= TargetLeft;
	}

	private void UnlinkAgressionZone(){
		agressionZone.OnInSight 	-= SetTarget;
		agressionZone.OnEnteredZone -= ChaseState;
		agressionZone.OnExitedZone 	-= TargetLeft;
	}


	///
	/// Linkage Functions.
	/// 

	private void HandleAttackChosen(byte attackId){
		if(agressionZone.IsInSight(Target)==true){
			attackHandler.StartAttacking();
		}
	}

	private void HandleStartAttack(byte attackId, AttackDirection attackDirection){
		AttackingState();
		switch(attackId){
			case (byte)AttackId.Slash:
				switch(attackDirection){
					case AttackDirection.Down:
						animator.Play("AttackBackward");
						animator.FlipH = false;
					break;
					case AttackDirection.Left:
						animator.Play("AttackSide");
						animator.FlipH = false;
					break;
					case AttackDirection.Right:
						animator.Play("AttackSide");
						animator.FlipH = true;
					break;
					case AttackDirection.Up:
						animator.Play("AttackForward");                 
						animator.FlipH = false;
					break;
				}
			break;
			default:
				throw new Exception($"Attack id[{attackId}] has not been implemented!");
		}
		characterMovement.ZeroDirection();
	}

	private void HandleAttack(byte attackId, AttackDirection attackDirection){
		switch(attackId){
			case (byte)AttackId.Slash:
				switch(attackDirection){
					case AttackDirection.Down:
						hitBoxHandler.EnableHitBox((int)AttackHitBoxId.SlashDown, 0.33f);
						animator.Play("AttackBackward");
					break;
					case AttackDirection.Left:
						hitBoxHandler.EnableHitBox((int)AttackHitBoxId.SlashLeft, 0.33f);
						animator.Play("AttackSide");
					break;
					case AttackDirection.Right:
						hitBoxHandler.EnableHitBox((int)AttackHitBoxId.SlashRight, 0.33f);
						animator.Play("AttackSide");
					break;
					case AttackDirection.Up:
						hitBoxHandler.EnableHitBox((int)AttackHitBoxId.SlashUp, 0.33f);
						animator.Play("AttackForward");                 
					break;
				}
				characterMovement.Impulse(normalDirectionToTarget * 100f);
			break;
			default:
			throw new Exception($"Attack id[{attackId}] has not been implemented!");
		}
	}

	private void HandleAttackHit(Node other, int hitboxId){
		string hitLayer = PhysicsManager.Singleton.GetPhysics2DLayerName((other as CollisionObject2D).CollisionLayer);
		switch (hitLayer){
			case "Player":
				Health playerHealth = other.GetNode<Health>(Health.NodeName);
				playerHealth.Damage(1);
			break;
			default:
			throw new Exception($"{hitLayer} not implemented.");
		}
	}

	private void HandleAttackEnded(){
		EvaluateState();
	}

	private void OnDamaged(){
		float stunTime = 0.66f;
		hitFlash.Flash();
		StunState(stunTime);
		IgnoreEnemyCollisionMask(stunTime);
	}



	public void Kill(){
		EnemyManager.Instance.RemoveEnemy(this);
		QueueFree();
	}

	private void OnAnimationChanged(){
		// work around to always ensure frame 0 of animation event is fired.
		OnFrameChanged(animator.Animation);		
	}

	private void OnFrameChanged(){
		// work around to always ensure frame 0 of animation event is fired.
		int frame = animator.Frame;
		if(frame==0){
			return;
		}
		OnFrameChanged(animator.Animation);
	}

	private void OnFrameChanged(string animation){
		switch(animation){
			case "AttackBackward":
			case "AttackForward":
			case "AttackSide":
				AttackAnimationFrameEvent(animator.Frame);
			break;
			case "RunBackward":
			case "RunForward":
			case "RunSide":
				RunAnimationFrameEvent(animator.Frame);
			break;
		}
	}

	private void AttackAnimationFrameEvent(int frame){
		switch(frame){
			case 0:
				audioPlayer.PlaySound("WorkerAttack", GlobalPosition);
			break;
		}
	}

	private void RunAnimationFrameEvent(int frame){
		switch(frame){
			case 2:
			case 6:
				audioPlayer.PlaySound("StoneFootstep", GlobalPosition);
			break;
		}
	}

}
