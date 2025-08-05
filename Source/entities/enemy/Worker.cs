using Godot;
using System;
using System.ComponentModel;

public partial class Worker : Enemy{
    private enum AttackId : byte{
		Slash    = 0,
	}

	private enum AttackHitBoxId{
		AttackDown   = 0,
		AttackLeft   = 1,
		AttackRight  = 2,
		AttackUp     = 3,
	}

    protected override void TargetLeft(Node2D node){
        base.TargetLeft(node);
        audioPlayer.StopSound("WorkerAttack", immediate: true);
    }

    protected override void StunState(float time){
        base.StunState(time);
        audioPlayer.StopSound("WorkerAttack", immediate: true);
    }

    protected override void HandleAttack(byte attackId, AttackDirection attackDirection){
		FacingDirection attackFacingDirection = AttackDirectionToFacingDirection(attackDirection);
		switch(attackId){
			case (byte)AttackId.Slash:
				switch(attackDirection){
					case AttackDirection.Down:
						hitBoxHandler.EnableHitBox((int)AttackHitBoxId.AttackDown, 0.33f);
					break;
					case AttackDirection.Left:
						hitBoxHandler.EnableHitBox((int)AttackHitBoxId.AttackLeft, 0.33f);
					break;
					case AttackDirection.Right:
						hitBoxHandler.EnableHitBox((int)AttackHitBoxId.AttackRight, 0.33f);
					break;
					case AttackDirection.Up:
						hitBoxHandler.EnableHitBox((int)AttackHitBoxId.AttackUp, 0.33f);
					break;
				}
                PlayAnimation("Attack", attackFacingDirection);
				characterMovement.Impulse(normalDirectionToTarget * 100f);
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

    protected override void OnDamagedCallback(){
        base.OnDamagedCallback();
		float stunTime = 0.33f;
		StunState(stunTime);
    }


}
