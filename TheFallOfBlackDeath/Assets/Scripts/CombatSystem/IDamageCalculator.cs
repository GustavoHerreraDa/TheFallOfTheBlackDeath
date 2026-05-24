/// <summary>
/// Input data required by damage calculators. It keeps HealthModSkill from
/// leaking into calculator implementations.
/// </summary>
public struct DamageCalculationContext
{
    public Fighter emitter;
    public Fighter receiver;
    public Skill sourceSkill;
    public BodyPart targetPart;
    public float amount;
    public HealthModType modType;
    public DamageType damageType;
    public PartStatus statusToApply;
    public float baseCritChance;
    public float spiritSoftCap;
    public float maxSpiritCritBonus;
    public float missChance;
    public bool rollRandomness;

    public DamageCalculationContext(
        Fighter emitter,
        Fighter receiver,
        Skill sourceSkill,
        BodyPart targetPart,
        float amount,
        HealthModType modType,
        DamageType damageType,
        PartStatus statusToApply,
        float baseCritChance,
        float spiritSoftCap,
        float maxSpiritCritBonus,
        float missChance,
        bool rollRandomness)
    {
        this.emitter = emitter;
        this.receiver = receiver;
        this.sourceSkill = sourceSkill;
        this.targetPart = targetPart;
        this.amount = amount;
        this.modType = modType;
        this.damageType = damageType;
        this.statusToApply = statusToApply;
        this.baseCritChance = baseCritChance;
        this.spiritSoftCap = spiritSoftCap;
        this.maxSpiritCritBonus = maxSpiritCritBonus;
        this.missChance = missChance;
        this.rollRandomness = rollRandomness;
    }
}

public interface IDamageCalculator
{
    DamageResult Calculate(DamageCalculationContext context);
    float EstimateDamage(DamageCalculationContext context);
    float CalculateMissChance(DamageCalculationContext context);
    bool CanTriggerSynergy(DamageCalculationContext context);
}
