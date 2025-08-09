using Godot;
using System;

public partial class Boss : Node{
    [Export] protected Health health;
    [Export] protected WayfindingAgent2D wayfinding;
    [Export] protected CharacterMovement characterMovement;
    [Export] protected AiAttackHandler attackHandler;
    [Export] protected HitFlashShaderController hitFlash;
    [Export] protected AnimatedSprite2D animator;
    [Export] protected Node2D Target;

	protected const string ChaseMoveAnimationName 	= "Chase";
	protected const string IdleAnimationName 		= "Idle";
	protected const string WanderAnimationName 		= "Wander";
    [Export(PropertyHint.Layers2DPhysics)] uint avoidanceChaseStateLineOfSightObstructions;
}
