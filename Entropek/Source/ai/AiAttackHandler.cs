using Godot;
using System;
using Godot.Collections;

public partial class AiAttackHandler : Node{

    public const string NodeName = nameof(AiAttackHandler);

     
    [Export] private Array<AiAttack> upAttacks;
    [Export] private Array<AiAttack> rightAttacks;
    [Export] private Array<AiAttack> downAttacks;
    [Export] private Array<AiAttack> leftAttacks;
    [Export] private Array<AiAttack> omniDirectionalAttacks;
    private Array<AiAttack> availableAttacks = new Array<AiAttack>(); // available attacks to use in a given frame.

    private RandomNumberGenerator rng = new RandomNumberGenerator();
    
    [Export] 
    private Timer cooldown;

    public event Action<byte> OnAttackChosen;

    private float angleToTarget     = 0;
    private float distanceToTarget  = float.MaxValue;
    private float maxRangeAttack    = float.MinValue;

    private TargetDirection targetDirection = TargetDirection.Right;
    private enum TargetDirection : byte{
        Up,
        Right,
        Down,
        Left
    } 

    public override void _Ready(){
        base._Ready();

        #if TOOLS
        Entropek.Util.Node.VerifyName(this, NodeName);
        #endif

        rng.Randomize();

        // set max range attack for perf reasons.

        CalcMaxRange();

        // GD.Print(attacks.Count);
        // GD.Print(maxRangeAttack);
    }

    public override void _PhysicsProcess(double delta){
        base._PhysicsProcess(delta);

        if(cooldown.TimeLeft <= 0){
            DetermineAvailableAttacks();
            ExecuteRandomAvailableAttack();
        }
    }

    private void CalcMaxRange(){
        
        // up attacks.

        for(int i = 0; i < upAttacks.Count; i++){
            AiAttack attack = upAttacks[i];
            if(attack.Range > maxRangeAttack){
                maxRangeAttack = attack.Range;
            }
        }

        // right attacks.

        for(int i = 0; i < rightAttacks.Count; i++){
            AiAttack attack = rightAttacks[i];
            if(attack.Range > maxRangeAttack){
                maxRangeAttack = attack.Range;
            }
        }

        // down attacks.

        for(int i = 0; i < downAttacks.Count; i++){
            AiAttack attack = downAttacks[i];
            if(attack.Range > maxRangeAttack){
                maxRangeAttack = attack.Range;
            }
        }

        // left attacks.

        for(int i = 0; i < leftAttacks.Count; i++){
            AiAttack attack = leftAttacks[i];
            if(attack.Range > maxRangeAttack){
                maxRangeAttack = attack.Range;
            }
        }

        // omni directional attacks.

        for(int i = 0; i < omniDirectionalAttacks.Count; i++){
            AiAttack attack = omniDirectionalAttacks[i];
            if(attack.Range > maxRangeAttack){
                maxRangeAttack = attack.Range;
            }
        }
    }

    private void DetermineAvailableAttacks(){
        availableAttacks.Clear();
        
        // return if we are not in the max range attack.

        if(distanceToTarget > maxRangeAttack){
            // GD.Print(maxRangeAttack);
            return;
        }

        // directional attacks.

        switch(targetDirection){
            case TargetDirection.Up:
                for(int i = 0; i < upAttacks.Count; i++){
                    AiAttack attack = upAttacks[i];
                    if(attack.Range < distanceToTarget){
                        continue;
                    }
                    availableAttacks.Add(attack);
                }
            break;
            case TargetDirection.Right:
                for(int i = 0; i < rightAttacks.Count; i++){
                    AiAttack attack = rightAttacks[i];
                    if(attack.Range < distanceToTarget){
                        continue;
                    }
                    availableAttacks.Add(attack);
                }
            break;
            case TargetDirection.Down:
                for(int i = 0; i < downAttacks.Count; i++){
                    AiAttack attack = downAttacks[i];
                    if(attack.Range < distanceToTarget){
                        continue;
                    }
                    availableAttacks.Add(attack);
                }
            break;
            case TargetDirection.Left:
                for(int i = 0; i < leftAttacks.Count; i++){
                    AiAttack attack = leftAttacks[i];
                    if(attack.Range < distanceToTarget){
                        continue;
                    }
                    availableAttacks.Add(attack);
                }
            break;
        }

        // omni directional attacks

        for(int i = 0; i < omniDirectionalAttacks.Count; i++){
            AiAttack attack = omniDirectionalAttacks[i];
            if(attack.Range < distanceToTarget){
                continue;
            }
            availableAttacks.Add(attack);
        }
    }

    private void ExecuteRandomAvailableAttack(){
        
        // return if there are no available attacks.
        
        if(availableAttacks.Count <= 0){
            return;
        }

        // choose a random attack.

        int r = rng.RandiRange(0,availableAttacks.Count-1);
        AiAttack chosen = availableAttacks[r];
        
        // set the cooldown time for the next attack.

        cooldown.WaitTime = chosen.Cooldown;
        cooldown.Start();
        
        // signal that the attack has been chosen.

        OnAttackChosen?.Invoke(chosen.Id);

    }

    public void SetDistanceToTarget(float distanceToTarget){
        this.distanceToTarget = distanceToTarget;
        // GD.Print(distanceToTarget);
    }

    public void SetDirectionToTarget(Vector2 directionToTarget){
        angleToTarget = Mathf.Atan2(directionToTarget.Y, directionToTarget.X);
        angleToTarget = Mathf.RadToDeg(angleToTarget);
    
        if(angleToTarget >= -135 && angleToTarget <= -45){
            targetDirection = TargetDirection.Up;
        }
        else if(angleToTarget >= -45 && angleToTarget <= 45){
            targetDirection = TargetDirection.Right;
        }
        else if(angleToTarget >= 45 && angleToTarget <= 135){
            targetDirection = TargetDirection.Down;
        }
        else{
            targetDirection = TargetDirection.Left;
        }
    }
}
