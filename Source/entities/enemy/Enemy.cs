using Entropek.Ai;
using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization.Metadata;

public abstract partial class Enemy : CharacterBody3D{ // <-- make sure to inherit from CollisionObect2D for hitbox handler and Player.
	
	[ExportGroup("Nodes")]
	[Export] protected Timer stunTimer;
	[Export] protected Timer avoidanceIntentionChaseStateTimer;
	[Export] protected Health health;
	[Export] protected WayfindingAgent3D navAgent;
	[Export] protected CharacterMovement characterMovement;
	[Export] protected AiAttackHandler attackHandler;
	[Export] protected HitBoxHandler hitBoxHandler;
	[Export] protected HitFlashShaderController hitFlash;
	[Export] protected AnimatedSprite3D animator;
	[Export] protected AudioPlayer audioPlayer;
	[Export] protected AgressionZone agressionZone;
	[Export] protected AvoidanceAgent avoidanceAgent;
	[Export] protected SpriteSorter3D spriteSorter;
	[Export] public Node3D Target;
	
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
	protected Vector3 directionToTarget 			= Vector3.Zero;
	protected Vector3 normalDirectionToTarget 		= Vector3.Zero;
	protected float distanceToTarget 				= float.MaxValue;
	[Export(PropertyHint.Layers2DPhysics)] uint avoidanceChaseStateLineOfSightObstructions;
	protected float damagedKnockback;
	public float stunStateAttackHandlerStandbyAdditiveTime;
	protected EnemyState state = EnemyState.None;
	protected FacingDirection facingDirection = FacingDirection.Backward;
	protected ChaseStateIntention chaseStateIntention = ChaseStateIntention.ApproachTarget;
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
		spriteSorter.UpdateSortingOffset();
		PhysicsProcess?.Invoke(delta);	
	}


	/// 
	/// State Machine
	/// 


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


	/// 
	/// Idle State.
	/// 


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
		Vector3 Velocity = characterMovement.Velocity; 
		if(Velocity == Vector3.Zero){
			PlayAnimation(IdleAnimationName, facingDirection);
		}
		else{
			CalculateFacingDirection(Velocity, out facingDirection);
			PlayAnimation(WanderAnimationName, facingDirection);			
		}
	}


	/// 
	/// Chase State.
	/// 


	protected void ChaseState(Node3D target){
		SetTarget(target);
		wanderer?.PauseState();
		ChaseState();
	}

	public void ChaseState(){

		state = EnemyState.Chase;
		
		Process        	= null;
		PhysicsProcess  = ChaseStatePhysicsProcess;
		
		// update data relevant to this frames.

		if(chaseStateIntention == ChaseStateIntention.ApproachTarget){
			ApproachIntentionChaseState();
		}

		CalculateRelationshipToTarget();
		attackHandler.ResumeState();
	}
	protected void ChaseStatePhysicsProcess(double delta){
		// throw new NotImplementedException("method is not implemented");
		if(IsInstanceValid(Target) == false || Target == null){
			return;
		}
		
		CalculateRelationshipToTarget();

		if(chaseStateIntention == ChaseStateIntention.ApproachTarget){
			navAgent.SetTargetPosition(Target.Position);
		}

		MoveAlongPath();
	
		Vector3 velocity = characterMovement.Velocity;
		if(velocity == Vector3.Zero){
			PlayAnimation(IdleAnimationName, facingDirection);
		}
		else{
			CalculateFacingDirection(velocity, out facingDirection);
			PlayAnimation(ChaseMoveAnimationName, facingDirection);
		}
	}

	protected void ApproachIntentionChaseState(){
		avoidanceIntentionChaseStateTimer.Start();
		chaseStateIntention = ChaseStateIntention.ApproachTarget;
	}

	protected void AvoidanceIntentionChaseState(){
		throw new NotImplementedException("method is not implemented");
		// if(GD.RandRange(0,2)==0){
		// 	return;
		// }

		// avoidanceIntentionChaseStateTimer.Stop();

		// if(navAgent.SetTargetPosition(Target.Position, new Vector2I(-6,-6), new Vector2I(6,6), avoidanceChaseStateLineOfSightObstructions)){
		// 	chaseStateIntention = ChaseStateIntention.AvoidTarget;
		// }
		// else{
		// 	chaseStateIntention = ChaseStateIntention.ApproachTarget;
		// }
	}


	/// 
	/// Stun State.
	/// 


	protected virtual void StunState(float time){
		
		state 			= EnemyState.Stunned;
		Process        	= null;
		PhysicsProcess  = null;
		attackHandler.HaltState(time+stunStateAttackHandlerStandbyAdditiveTime);
		hitBoxHandler.DisableAllHitBoxes();
		CalculateFacingDirection(-characterMovement.Velocity, out FacingDirection facing);
		PlayAnimation(StunAnimationName, facing);
		stunTimer.WaitTime = time;
		stunTimer.Start();
	}


	/// 
	/// Attacking State.
	/// 


	protected void AttackingState(){
		state = EnemyState.Attacking;
		Process = null;
		PhysicsProcess = null;
	}


	/// 
	/// Flow States.
	/// 


	protected void PauseState(){
		Process = null;
		PhysicsProcess = null;
		attackHandler.PauseState();
		characterMovement?.PauseState();
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

	protected void MoveAlongPath(){
		if(navAgent.CalculateNewPath()==true){
			navAgent.UpdateCurrentPathToTarget();
			// avoidanceAgent.CalculatAvoidanceDirection();
			// characterMovement.Move((navAgent.NextPathPoint - GlobalPosition).Lerp(avoidanceAgent.AvoidanceDirection, avoidanceAgent.ProximityStrength));
			characterMovement.Move(navAgent.NextPathPoint - GlobalPosition);
		}
		else{
			IdleState();
		}
	}

	protected void CalculateRelationshipToTarget(){		
		if(IsInstanceValid(Target)==false || Target == null){
			return;
		}

		directionToTarget = Target.GlobalPosition- GlobalPosition;
		normalDirectionToTarget = directionToTarget.Normalized();
		distanceToTarget = directionToTarget.Length();
		attackHandler.SetDirectionToTarget(directionToTarget);
		attackHandler.SetDistanceToTarget(distanceToTarget);
	}

	protected bool CalculateFacingDirection(Vector3 direction, out FacingDirection facing){
		
		facing = FacingDirection.Backward;
		
		if(direction == Vector3.Zero){
			return false;
		}
		
		float angle = Mathf.Atan2(direction.Z, direction.X);
		angle = Mathf.RadToDeg(angle);
		// Ceil to nearest multiple of 2
		angle = Mathf.Ceil(angle * 0.5f) * 2f;
		
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

	public void SetTarget(Node3D target){
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
		LinkWayfindingAgent();
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
		UnlinkWayfindingAgent();
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


	/// 
	/// Animator.
	/// 


	protected virtual void LinkAnimator(){
		animator.FrameChanged 		+= OnFrameChanged;
		animator.AnimationChanged 	+= OnAnimationChanged;
	}

	protected virtual void UnlinkAnimator(){
		animator.FrameChanged 		-= OnFrameChanged;
		animator.AnimationChanged 	-= OnAnimationChanged;
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


	/// 
	/// Health.
	/// 


	protected virtual void LinkHealth(){
		health.OnDeath  += Kill;
		health.OnDamage += OnDamagedCallback;
	}

	protected virtual void UnlinkHealth(){
		health.OnDeath  -= Kill;
		health.OnDamage -= OnDamagedCallback;
	}

	protected virtual void OnDamagedCallback(){
		hitFlash.Flash();
	}
	
	public virtual void Kill(){
		GD.Print("kill");
		EnemyManager.Instance.RemoveEnemy(this);
		QueueFree();
	}



	/// 
	/// AtackHandler.
	/// 


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

	private void HandleAttackChosen(byte attackId){
		if(agressionZone.IsInSight(Target)==true){
			attackHandler.StartAttacking();
		}
	}

	protected abstract void HandleStartAttack(byte attackId, AttackDirection attackDirection);

	protected abstract void HandleAttack(byte attackId, AttackDirection attackDirection);

	private void HandleAttackEnded(){
		EvaluateState();
	}


	/// 
	/// HitBoxHandler
	/// 


	protected virtual void LinkHitBoxHandler(){
		hitBoxHandler.OnHit += HandleAttackHit;
	}

	protected virtual void UnlinkHitBoxHandler(){
		hitBoxHandler.OnHit -= HandleAttackHit;
	}

	private void HandleAttackHit(Node other, int hitboxId){
		string hitLayer = PhysicsManager.Singleton.GetPhysics3DLayerName((other as CollisionObject3D).CollisionLayer);
		switch (hitLayer){
			case "Player":
				Health playerHealth = other.GetNode<Health>(Health.NodeName);
				playerHealth.Damage(1);
			break;
			default:
			throw new Exception($"{hitLayer} not implemented.");
		}
	}


	/// 
	/// Timers
	/// 


	protected virtual void LinkTimers(){
		stunTimer.Timeout 							+= EvaluateState;
		// avoidanceIntentionChaseStateTimer.Timeout 	+= AvoidanceIntentionChaseState;
	}

	protected virtual void UnlinkTimers(){
		stunTimer.Timeout 							-= EvaluateState;
		// avoidanceIntentionChaseStateTimer.Timeout 	-= AvoidanceIntentionChaseState;
	}


	/// 
	/// Ai Agression Zone.
	/// 


	protected virtual void LinkAgressionZone(){
		agressionZone.OnInSight 	+= ChaseState;
		agressionZone.OnExitedZone 	+= TargetLeft;
	}

	protected virtual void UnlinkAgressionZone(){
		agressionZone.OnInSight 	-= ChaseState;
		agressionZone.OnExitedZone 	-= TargetLeft;
	}

	protected virtual void TargetLeft(Node3D node){
		Target = null;
		characterMovement.ZeroDirection();
		attackHandler.HaltState();
		hitBoxHandler.DisableAllHitBoxes();
		IdleState();
	}


	// Ai Wander.


	protected virtual void LinkAiWander(){
		wanderer.OnDirectionChosen += characterMovement.Move;
	}

	protected virtual void UnlinkAiWander(){
		wanderer.OnDirectionChosen -= characterMovement.Move;
	}


	/// 
	/// Wayfinding Agent.
	/// 


	protected virtual void LinkWayfindingAgent(){
		navAgent.OnReachedTarget 		+= OnReachedTargetCallback;
	}

	protected virtual void UnlinkWayfindingAgent(){
		navAgent.OnReachedTarget 		-= OnReachedTargetCallback;
	}

	protected void OnReachedTargetCallback(){
		if(state != EnemyState.Chase || chaseStateIntention != ChaseStateIntention.AvoidTarget){
			return;
		}
		// return to approaching.
		ApproachIntentionChaseState();
	}

	protected void OnNextPathPointSetCallback(Vector3 nextPathPoint){
		CalculateFacingDirection(nextPathPoint - GlobalPosition, out facingDirection);
	}
}
