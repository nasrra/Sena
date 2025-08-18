using Godot;
using System;
using System.ComponentModel;

public partial class Worker : Enemy{
	
	/// <summary>
	/// Definitions.
	/// </summary>


    private enum AttackId : byte{
		Punch    	= 0,
	}

	private enum AttackHitBoxId{
		PunchDown   = 0,
		PunchLeft   = 1,
		PunchRight  = 2,
		PunchUp     = 3,
	}

	private static readonly AiAttack PunchAttack = new AiAttack(
		cooldown: 			2,
		handlerCooldown:	0.2f,
		leadInTime: 		0.5f,
		attackTime:			0.05f,
		followThroughTime:  0.75f,
		minTargetDistance:  2,
		damage: 			1,
		id:					(byte)AttackId.Punch
	);


    /// 
    /// Base.
    /// 


    public override void _Ready(){
        base._Ready();
		attackHandler.Initialise(
			downAttacks: 	[PunchAttack],
			leftAttacks: 	[PunchAttack],	
			omniAttacks: 	[],
			rightAttacks: 	[PunchAttack],
			upAttacks: 		[PunchAttack]
		);

		navAgent.target = Target;

		// GameplayGui.Singleton.BossHealthBarHud.LinkToHealth(health);
		// GameplayGui.Singleton.BossHealthBarHud.EnableBar();
		// GameplayGui.Singleton.BossHealthBarHud.SetNameTag("Worker");
	}


	/// 
	/// Overrides.
	/// 

    // protected override void TargetLeft(Node2D node){
    //     base.TargetLeft(node);
    //     audioPlayer.StopSound("WorkerAttack", immediate: true);
    // }

    protected override void StunState(float time){
        base.StunState(time);
        audioPlayer.StopSound("WorkerAttack", immediate: true);
    }

    protected override void HandleAttack(byte attackId, AttackDirection attackDirection){
		FacingDirection attackFacingDirection = AttackDirectionToFacingDirection(attackDirection);
		switch(attackId){
			case (byte)AttackId.Punch:
				switch(attackDirection){
					case AttackDirection.Down:
						hitBoxHandler.EnableHitBox((int)AttackHitBoxId.PunchDown, 0.33f);
					break;
					case AttackDirection.Left:
						hitBoxHandler.EnableHitBox((int)AttackHitBoxId.PunchLeft, 0.33f);
					break;
					case AttackDirection.Right:
						hitBoxHandler.EnableHitBox((int)AttackHitBoxId.PunchRight, 0.33f);
					break;
					case AttackDirection.Up:
						hitBoxHandler.EnableHitBox((int)AttackHitBoxId.PunchUp, 0.33f);
					break;
				}
                PlayAnimation("Attack", attackFacingDirection);
				characterMovement.Impulse(normalDirectionToTarget * 12f);
			break;
			default:
			throw new Exception($"Attack id[{attackId}] has not been implemented!");
		}
    }

	protected override void OnAnimatorFrameChange(string animation){
		switch(animation){
			case "AttackBackward":
			case "AttackForward":
			case "AttackSide":
				AttackAnimationFrameEvent(animator.Frame);
			break;
			case "ChaseBackward":
			case "ChaseForward":
			case "ChaseSide":
			case "WanderBackward":
			case "WanderForward":
			case "WanderSide":
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

    protected override void HandleStartAttack(byte attackId, AttackDirection attackDirection){
		AttackingState();
		switch(attackId){
			case (byte)AttackId.Punch:
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

    protected override void OnDamagedCallback(){
        base.OnDamagedCallback();
		float stunTime = 0.33f;
		StunState(stunTime);
		audioPlayer.PlaySound("MeleeHit", GlobalPosition);
    }

    public override void Kill(){
		AudioManager.Singleton.PlayEvent("EnemyDeath", GlobalPosition, true);
		// GameplayGui.Singleton.BossHealthBarHud.DisableBar();
        base.Kill();
    }



}
