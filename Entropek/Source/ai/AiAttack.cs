
public struct AiAttack{
    public float Cooldown              {get; private set;} // <-- cooldown for this specific attack.
    public float HandlerCooldown       {get; private set;} // <-- cooldown before a new attack can be chosen.
    public float LeadInTime            {get; private set;}
    public float AttackTime            {get; private set;}
    public float FollowThroughTime     {get; private set;}
    public float MinTargetDistance     {get; private set;}
    public int Damage                  {get; private set;}
    public byte Id                     {get; private set;}
    public bool IsValid                {get; private set;} = false;

    public AiAttack(
        float cooldown,               
        float handlerCooldown,
        float leadInTime,      
        float attackTime,            
        float followThroughTime,     
        float minTargetDistance,     
        int damage,                  
        byte id
    ){
        Cooldown            = cooldown;
        HandlerCooldown     = handlerCooldown;
        LeadInTime          = leadInTime;
        AttackTime          = attackTime;
        FollowThroughTime   = followThroughTime;
        MinTargetDistance   = minTargetDistance;
        Damage              = damage;
        Id                  = id; 
        IsValid             = true;
    }
}

public enum AttackDirection : byte{
    Down = 0,
    Left = 1,
    Omni = 2,
    Right = 3,
    Up = 4
}
