/// <summary>
/// Immutable-ish payload describing the result of a damage calculation and its
/// final application to a fighter.
/// </summary>
public struct DamageResult
{
    public Fighter attacker;
    public Fighter receiver;
    public Skill sourceSkill;
    public BodyPart targetPart;
    public DamageType damageType;
    public float baseAmount;
    public float finalAmount;
    public float appliedAmount;
    public float previousHealth;
    public float currentHealth;
    public float critChance;
    public float missChance;
    public bool isCritical;
    public bool isMiss;
    public bool affectedBodyPart;
    public bool destroyedBodyPart;
    public bool hasStatusChange;
    public PartStatus resultingStatus;
    public string[] messages;

    public bool IsDamage => appliedAmount < 0f || (!isMiss && finalAmount < 0f);

    public static DamageResult Create(
        Fighter attacker,
        Fighter receiver,
        Skill sourceSkill,
        BodyPart targetPart,
        DamageType damageType,
        float baseAmount,
        float finalAmount,
        bool isCritical,
        bool isMiss,
        float critChance,
        float missChance,
        bool hasStatusChange,
        PartStatus resultingStatus,
        string[] messages)
    {
        return new DamageResult
        {
            attacker = attacker,
            receiver = receiver,
            sourceSkill = sourceSkill,
            targetPart = targetPart,
            damageType = damageType,
            baseAmount = baseAmount,
            finalAmount = isMiss ? 0f : finalAmount,
            appliedAmount = isMiss ? 0f : finalAmount,
            previousHealth = 0f,
            currentHealth = 0f,
            critChance = critChance,
            missChance = missChance,
            isCritical = isCritical,
            isMiss = isMiss,
            affectedBodyPart = targetPart != BodyPart.None,
            destroyedBodyPart = false,
            hasStatusChange = hasStatusChange,
            resultingStatus = resultingStatus,
            messages = messages ?? new string[0]
        };
    }

    public static DamageResult FromLegacyAmount(
        Fighter attacker,
        Fighter receiver,
        Skill sourceSkill,
        BodyPart targetPart,
        float amount)
    {
        return Create(
            attacker,
            receiver,
            sourceSkill,
            targetPart,
            DamageType.Kinetic,
            amount,
            amount,
            false,
            amount == 0f,
            0f,
            0f,
            false,
            PartStatus.None,
            new string[0]);
    }

    public DamageResult WithApplication(
        float appliedAmount,
        float previousHealth,
        float currentHealth,
        bool affectedBodyPart,
        bool destroyedBodyPart)
    {
        DamageResult copy = this;
        copy.appliedAmount = appliedAmount;
        copy.previousHealth = previousHealth;
        copy.currentHealth = currentHealth;
        copy.affectedBodyPart = affectedBodyPart;
        copy.destroyedBodyPart = destroyedBodyPart;
        return copy;
    }
}
