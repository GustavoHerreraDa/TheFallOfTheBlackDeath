using UnityEngine;
using UnityEngine.Serialization;
//TP2 FACUNDO FERREIRO
/// <summary>
/// Defines the named values used by health mod type.
/// </summary>
public enum HealthModType
{
    STAT_BASED, FIXED, PERCENTAGE
}

/// <summary>
/// Applies calculated health changes to one or more body parts. Damage math,
/// critical rolls, miss chance, and synergy rules live in IDamageCalculator.
/// </summary>
public class HealthModSkill : BodyPartTargetSkill
{
    [Header("Synergy Settings")]
    public DamageType damageType;
    public PartStatus statusToApply = PartStatus.None;

    [Header("Health Mod")]
    public float amount;
    public HealthModType modType;

    [Header("Critical Chance")]
    [FormerlySerializedAs("critChance")]
    [Range(0f, 1f)] public float baseCritChance = 0.05f;
    public float spiritSoftCap = 50f;
    [Range(0f, 1f)] public float maxSpiritCritBonus = 0.40f;

    [Header("Accuracy")]
    [Range(0f, 1f)] public float missChance = 0f;

    private IDamageCalculator damageCalculator = new StandardDamageCalculator();

    public void SetDamageCalculator(IDamageCalculator calculator)
    {
        damageCalculator = calculator ?? new StandardDamageCalculator();
    }

    protected override void OnRun(Fighter receiver)
    {
        if (this.targeting == SkillTargeting.ALL_OPPONENTS)
        {
            ApplyToAllBodyParts(receiver);
            return;
        }

        if (this.TryGetTargetBodyPart(receiver, out Fighter.BodyPartData targetPart))
            ApplyToBodyPart(receiver, targetPart);
    }

    public override bool CanTriggerSynergy(Fighter target, BodyPart part = BodyPart.None)
    {
        return damageCalculator.CanTriggerSynergy(CreateContext(target, part, false));
    }

    public float GetAdjustedMissChance(Fighter receiver)
    {
        return damageCalculator.CalculateMissChance(CreateContext(receiver, this.BodyPartTarget, false));
    }

    public float GetEstimatedDamage(Fighter receiver, BodyPart targetPartType)
    {
        return damageCalculator.EstimateDamage(CreateContext(receiver, targetPartType, false));
    }

    private void ApplyToAllBodyParts(Fighter receiver)
    {
        if (receiver == null || receiver.bodyParts == null)
            return;

        foreach (Fighter.BodyPartData part in receiver.bodyParts)
        {
            if (part != null && !part.IsDestroyed)
                ApplyToBodyPart(receiver, part);
        }
    }

    private void ApplyToBodyPart(Fighter receiver, Fighter.BodyPartData targetPart)
    {
        if (receiver == null || targetPart == null)
            return;

        DamageResult result = damageCalculator.Calculate(CreateContext(receiver, targetPart.part, true));
        receiver.ModifyBodyPartHealth(targetPart.part, result, this.emitter, this);
        EnqueueResultMessages(result);
    }

    private DamageCalculationContext CreateContext(Fighter receiver, BodyPart targetPart, bool rollRandomness)
    {
        return new DamageCalculationContext(
            this.emitter,
            receiver,
            this,
            targetPart,
            this.amount,
            this.modType,
            this.damageType,
            this.statusToApply,
            this.baseCritChance,
            this.spiritSoftCap,
            this.maxSpiritCritBonus,
            this.missChance,
            rollRandomness);
    }

    private void EnqueueResultMessages(DamageResult result)
    {
        if (result.messages == null)
            return;

        for (int i = 0; i < result.messages.Length; i++)
        {
            if (!string.IsNullOrEmpty(result.messages[i]))
                this.messages.Enqueue(result.messages[i]);
        }
    }
}
