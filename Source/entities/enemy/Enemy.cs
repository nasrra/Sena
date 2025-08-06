using Entropek.Ai;
using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization.Metadata;

public abstract partial class Enemy : CharacterBody2D{ // <-- make sure to inherit from CollisionObect2D for hitbox handler and Player.
	
	[ExportGroup("Nodes")]
	[Export] protected Timer stunTimer;
	[Export] protected Timer ignoreEnemyTimer;
	[Export] protected Timer chaseStateSwapTimer;
	[Export] protected Health health;
	[Export] protected WayfindingAgent2D navAgent;
	[Export] protected CharacterMovement characterMovement;
	[Export] protected AiAttackHandler attackHandler;
	[Export] protected HitBoxHandler hitBoxHandler;
	[Export] protected HitFlashShaderController hitFlash;
	[Export] protected AnimatedSprite2D animator;
	[Export] protected AudioPlayer audioPlayer;
	[Export] protected AgressionZone agressionZone;
	[Export] public Node2D Target;
	
	[ExportGroup("Wanderer")]
	[Export] protected AiWander wanderer;
    [Export] protected double minPathTime;
    [Export] protected double maxPathTime;
    [Export] protected double minIdleTime;
    [Export] protected double maxIdleTime;
    [Export] protected Vector2 maxDirection = new Vector2(1,1);
    [Export] protected Vector2 minDirection = new Vector2(-1,-1);

	protected event Action<double> Process = null;
	protected event Action<double> PhysicsProcess = null;

	[ExportGroup("Variabales")]
	protected const string ChaseMoveAnimationName 	= "Chase";
	protected const string IdleAnimationName 		= "Idle";
	protected const string WanderAnimationName 		= "Wander";
	protected const string StunAnimationName 		= "Stun";
	protected Vector2 directionToTarget 			= Vector2.Zero;
	protected Vector2 normalDirectionToTarget 		= Vector2.Zero;
	protected float distanceToTarget 				= float.MaxValue;
	protected float damagedKnockback;
	public float stunStateAttackHandlerStandbyAdditiveTime;
	protected EnemyState state = EnemyState.None;
	protected FacingDirection facingDirection = FacingDirection.Backward;
	public bool stunOnHit = true;

	/// 
	/// Definitions.
	/// 


	protected enum EnemyState : byte{
		None,
		Idle,
		Chase,
		Stunned,
		Attacking,
	}

	protected enum FacingDirection : byte{
		Backward,
		Forward,
		Left,
		Right
	}

	protected enum ChaseStateIntention : byte{
		AvoidTarget,
		ApproachTarget
	} 


	///
	/// Base
	///


	public override void _Ready(){
		base._Ready();
		EnemyManager.Instance.AddEnemy(this);
		wanderer?.Initialise(
			minPathTime,
			maxPathTime,
			minIdleTime,
			maxIdleTime,
			maxDirection,
			minDirection
		);
		IdleState();
	}

	public override void _EnterTree(){
		base._EnterTree();
		LinkEvents();
	}

	public override void _ExitTree(){
		base._ExitTree();
		EnemyManager.Instance.RemoveEnemy(this);
		UnlinkEvents();
	}

	private void InvokeProcess(double delta){
		Process?.Invoke(delta);
	}
	private void InvokePhysicsProcess(double delta){
		Vector2I[] cellsAroundTarget = WayfindingAgent2D.Singleton
		
		// for(int i = 0; i < 6; i++){
		// 	WayfindingGrid2D.Singleton.groundClearance
		// }
		// GodotObject debugDraw = GetNode<GodotObject>("/root/DebugDraw2D");
		// debugDraw.Call("rect",);
		PhysicsProcess?.Invoke(delta);	
	}


	/// 
	/// State Machine
	/// 


	protected virtual void TargetLeft(Node2D node){
		Target = null;
		characterMovement.ZeroDirection();
		attackHandler.HaltState();
		hitBoxHandler.DisableAllHitBoxes();
		IdleState();
	}

	protected void EvaluateState(){

		// TODO: do some recovery state code when needed.
		if(Target == null){
			IdleState();
		}
		else if(attackHandler.IsAttacking == false){
			ChaseState();
		}
		attackHandler.EvaluateState();
	}

	protected void IdleState(){
		Process 		= null;
		PhysicsProcess 	= IdleStatePhysicsProcess;

		if(state == EnemyState.Idle){
			return;
		}
		
		state 			= EnemyState.Idle;
		animator.Play("IdleBackward");
		wanderer?.SetOrigin(GlobalPosition);
		wanderer?.EvaluateState();
	}

	protected void IdleStatePhysicsProcess(double delta){
		Vector2 moveDirection = characterMovement.MoveDirection; 
		if(moveDirection == Vector2.Zero){
			PlayAnimation(IdleAnimationName, facingDirection);
		}
		else{
			CalculateFacingDirection(moveDirection, out facingDirection);
			PlayAnimation(WanderAnimationName, facingDirection);			
		}
	}

	protected void ChaseState(Node2D target){
		SetTarget(target);
		wanderer?.PauseState();
		ChaseState();
	}

	public void ChaseState(){
		Process        	= null;
		PhysicsProcess  = ChaseStatePhysicsProcess;
		
		// update data relevant to this frames.

		CalculateRelationshipToTarget();
		attackHandler.SetDirectionToTarget(directionToTarget);
		attackHandler.SetDistanceToTarget(distanceToTarget);

		/// sample a square range aroung target postion.
		/// have a while loop with 6 (max) iterations to find a possible avoidance path.
		/// head toward that avoidance path if found.
		/// return to approaching target if path is not found.

		attackHandler.ResumeState();
	}

	protected void ChaseStatePhysicsProcess(double delta){
		if(IsInstanceValid(Target) && Target.IsInsideTree()==true){
			CalculateRelationshipToTarget();
			UpdateAttackHandler();
			MoveAlongPathToTarget();
			Vector2 moveDirection = characterMovement.MoveDirection;
			if(moveDirection == Vector2.Zero){
				PlayAnimation(IdleAnimationName, facingDirection);
			}
			else{
				CalculateFacingDirection(moveDirection, out facingDirection);
				PlayAnimation(ChaseMoveAnimationName, facingDirection);
			}
		}
	}

	protected virtual void StunState(float time){
		
		state 			= EnemyState.Stunned;
		Process        	= null;
		PhysicsProcess  = null;
		attackHandler.HaltState(time+stunStateAttackHandlerStandbyAdditiveTime);
		hitBoxHandler.DisableAllHitBoxes();
		CalculateFacingDirection(-characterMovement.Velocity, out FacingDirection facing);
		PlayAnimation(StunAnimationName, facing);
		IgnoreEnemyCollisionMask(time);
		stunTimer.WaitTime = time;
		stunTimer.Start();
	}

	protected void AttackingState(){
		state = EnemyState.Attacking;
		Process = null;
		PhysicsProcess = null;
	}

	protected void PauseState(){
		Process = null;
		PhysicsProcess = null;
		attackHandler.PauseState();
		characterMovement.PauseState();
		audioPlayer.PauseState();
		hitBoxHandler.PauseState();
		agressionZone.PauseState();
		wanderer?.PauseState();
		animator.SpeedScale = 0; // pause animator.
	}

	protected void ResumeState(){
		attackHandler.ResumeState();
		characterMovement.ResumeState();
		audioPlayer.ResumeState();
		animator.SpeedScale = 1; // resume animator.
		hitBoxHandler.ResumeState();
		agressionZone.ResumeState();
		wanderer?.ResumeState();
		EvaluateState();
	}


	/// 
	/// Shared Functions
	/// 


	protected void MoveAlongPathToTarget(){
		if(navAgent.CalculateNewPath(Target.GlobalPosition)==true){
			navAgent.UpdateCurrentPathToTarget();
			characterMovement.Move(navAgent.CurrentPathPoint - GlobalPosition);
		}
		else{
			IdleState();
		}
	}

	protected void CalculateRelationshipToTarget(){
		directionToTarget = Target.GlobalPosition- GlobalPosition;
		normalDirectionToTarget = directionToTarget.Normalized();
		distanceToTarget = directionToTarget.Length();
	}

	protected void UpdateAttackHandler(){
		attackHandler.SetDirectionToTarget(directionToTarget);
		attackHandler.SetDistanceToTarget(distanceToTarget);
	}

	protected void IgnoreEnemyCollisionMask(float time){
		
		ignoreEnemyTimer.WaitTime = time;
		SetCollisionMaskValue(PhysicsManager.Singleton.GetPhysics2DLayerId("Enemy"), false);
		ignoreEnemyTimer.Start();
	}

	protected void RespondToEnemyCollisionMask(){
		SetCollisionMaskValue(PhysicsManager.Singleton.GetPhysics2DLayerId("Enemy"), true);
	}

	protected bool CalculateFacingDirection(Vector2 direction, out FacingDirection facing){
		
		facing = FacingDirection.Backward;
		
		if(direction == Vector2.Zero){
			return false;
		}
		
		float angle = Mathf.Atan2(direction.Y, direction.X);
        angle = Mathf.RadToDeg(angle);
		
		if(angle >= -155 && angle <= -25){
			facing = FacingDirection.Forward;
			return true;
		}
		else if(angle >= 25 && angle <= 155){
			facing = FacingDirection.Backward;
			return true;
		}
		else if(angle > -45 && angle < 45){
			facing = FacingDirection.Right;
			return true;
		}
		else{
			facing = FacingDirection.Left;
			return true;
		}
	}

	protected void PlayAnimation(string animationName, FacingDirection facingDirection){
		switch(facingDirection){
			case FacingDirection.Backward:
				animator.Play(animationName+"Backward");
				animator.FlipH = false;
			break;
			case FacingDirection.Forward:
				animator.Play(animationName+"Forward");
				animator.FlipH = false;
			break;
			case FacingDirection.Left:
				animator.Play(animationName+"Side");
				animator.FlipH = false;
			break;
			case FacingDirection.Right:
				animator.Play(animationName+"Side");
				animator.FlipH = true;
			break;
		}
	}

	protected FacingDirection AttackDirectionToFacingDirection(AttackDirection attackDirection){
		switch(attackDirection){
			case AttackDirection.Down:
				return FacingDirection.Backward;
			case AttackDirection.Left:
				return FacingDirection.Left;
			case AttackDirection.Right:
				return FacingDirection.Right;
			case AttackDirection.Up:
				return FacingDirection.Forward;
			default:
				return facingDirection;
		}
	}

	public void SetTarget(Node2D target){
		Target = target;
	}

	/// 
	/// Linkage
	/// 


	protected virtual void LinkEvents(){
		LinkEntityManager();
		LinkAnimator();
		LinkHealth();
		LinkAttackHandler();
		LinkHitBoxHandler();
		LinkTimers();
		LinkAgressionZone();
		// LinkAiWander();
	}

	protected virtual void UnlinkEvents(){
		UnlinkEntityManager();
		UnlinkAnimator();
		UnlinkHealth();
		UnlinkAttackHandler();
		UnlinkHitBoxHandler();
		UnlinkTimers();
		UnlinkAgressionZone();
		// UnlinkAiWander();
	}

	protected virtual void LinkEntityManager(){
		EntityManager.Singleton.OnProcess 			+= InvokeProcess;
		EntityManager.Singleton.OnPhysicsProcess 	+= InvokePhysicsProcess;
		EntityManager.Singleton.OnPause 			+= PauseState;
		EntityManager.Singleton.OnResume 			+= ResumeState;
	}

	protected virtual void UnlinkEntityManager(){
		EntityManager.Singleton.OnProcess 			-= InvokeProcess;
		EntityManager.Singleton.OnPhysicsProcess 	-= InvokePhysicsProcess;
		EntityManager.Singleton.OnPause 			-= PauseState;
		EntityManager.Singleton.OnResume 			-= ResumeState;
	}

	protected virtual void LinkAnimator(){
		animator.FrameChanged 		+= OnFrameChanged;
		animator.AnimationChanged 	+= OnAnimationChanged;
	}

	protected virtual void UnlinkAnimator(){
		animator.FrameChanged 		-= OnFrameChanged;
		animator.AnimationChanged 	-= OnAnimationChanged;
	}

	protected virtual void LinkHealth(){
		health.OnDeath  += Kill;
		health.OnDamage += OnDamagedCallback;
	}

	protected virtual void UnlinkHealth(){
		health.OnDeath  -= Kill;
		health.OnDamage -= OnDamagedCallback;
	}

	protected virtual void LinkAttackHandler(){
		attackHandler.OnAttackChosen	+= HandleAttackChosen;
		attackHandler.OnAttack          += HandleAttack;
		attackHandler.OnAttackStarted   += HandleStartAttack;
		attackHandler.OnAttackEnded     += HandleAttackEnded;
	}

	protected virtual void UnlinkAttackHandler(){
		attackHandler.OnAttackChosen	-= HandleAttackChosen;
		attackHandler.OnAttack          -= HandleAttack;
		attackHandler.OnAttackStarted   -= HandleStartAttack;
		attackHandler.OnAttackEnded     -= HandleAttackEnded;
	}

	protected virtual void LinkHitBoxHandler(){
		hitBoxHandler.OnHit += HandleAttackHit;
	}

	protected virtual void UnlinkHitBoxHandler(){
		hitBoxHandler.OnHit -= HandleAttackHit;
	}

	protected virtual void LinkTimers(){
		stunTimer.Timeout 			+= EvaluateState;
		ignoreEnemyTimer.Timeout 	+= RespondToEnemyCollisionMask;
	}

	protected virtual void UnlinkTimers(){
		stunTimer.Timeout 			-= EvaluateState;
		ignoreEnemyTimer.Timeout 	-= RespondToEnemyCollisionMask;	
	}

	protected virtual void LinkAgressionZone(){
		agressionZone.OnInSight 	+= ChaseState;
		agressionZone.OnExitedZone 	+= TargetLeft;
	}

	protected virtual void UnlinkAgressionZone(){
		agressionZone.OnInSight 	-= ChaseState;
		agressionZone.OnExitedZone 	-= TargetLeft;
	}

	protected virtual void LinkAiWander(){
		if(wanderer != null){
			wanderer.OnDirectionChosen += characterMovement.Move;
		}
	}

	protected virtual void UnlinkAiWander(){
		if(wanderer != null){
			wanderer.OnDirectionChosen -= characterMovement.Move;
		}
	}


	///
	/// Linkage Functions.
	/// 

	private void HandleAttackChosen(byte attackId){
		if(agressionZone.IsInSight(Target)==true){
			attackHandler.StartAttacking();
		}
	}

	protected abstract void HandleStartAttack(byte attackId, AttackDirection attackDirection);

	protected abstract void HandleAttack(byte attackId, AttackDirection attackDirection);

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

	protected virtual void OnDamagedCallback(){
		hitFlash.Flash();
	}



	public void Kill(){
		EnemyManager.Instance.RemoveEnemy(this);
		QueueFree();
	}

	private void OnAnimationChanged(){
		// work around to always ensure frame 0 of animation event is fired.
		OnAnimatorFrameChange(animator.Animation);		
	}

	private void OnFrameChanged(){
		// work around to always ensure frame 0 of animation event is fired.
		int frame = animator.Frame;
		if(frame==0){
			return;
		}
		OnAnimatorFrameChange(animator.Animation);
	}

	protected abstract void OnAnimatorFrameChange(string animation);
}
