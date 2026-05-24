using System.Collections.Generic;
using UnityEngine;

public class StandardDamageCalculator : IDamageCalculator
{
    private const float CriticalMultiplier = 2f;
    private readonly List<IDamageSynergyRule> synergyRules;

    public StandardDamageCalculator()
    {
        synergyRules = new List<IDamageSynergyRule>
        {
            new CorrodedKineticSynergyRule(),
            new CorrodedThermalSynergyRule()
        };
    }

    public DamageResult Calculate(DamageCalculationContext context)
    {
        Fighter.BodyPartData targetPart = ResolveTargetPart(context);
        float baseAmount = CalculateBaseAmount(context);
        float finalAmount = baseAmount;
        float adjustedMissChance = CalculateMissChance(context);
        float adjustedCritChance = CalculateCriticalChance(context);
        bool isDamage = finalAmount < 0f;
        bool isMiss = isDamage && context.rollRandomness && Random.value < adjustedMissChance;
        bool isCritical = false;
        SynergyApplication synergy = SynergyApplication.None;

        if (!isMiss && isDamage)
        {
            isCritical = context.rollRandomness && Random.value < adjustedCritChance;
            if (isCritical)
                finalAmount *= CriticalMultiplier;

            synergy = ApplySynergy(context, targetPart, ref finalAmount);
        }

        return DamageResult.Create(
            context.emitter,
            context.receiver,
            context.sourceSkill,
            context.targetPart,
            context.damageType,
            baseAmount,
            finalAmount,
            isCritical,
            isMiss,
            adjustedCritChance,
            adjustedMissChance,
            synergy.hasStatusChange,
            synergy.resultingStatus,
            synergy.messages);
    }

    public float EstimateDamage(DamageCalculationContext context)
    {
        Fighter.BodyPartData targetPart = ResolveTargetPart(context);
        float estimatedAmount = CalculateBaseAmount(context);

        if (estimatedAmount < 0f)
            ApplySynergy(context, targetPart, ref estimatedAmount);

        return estimatedAmount;
    }

    public float CalculateMissChance(DamageCalculationContext context)
    {
        float adjusted = Mathf.Clamp01(context.missChance);

        if (context.targetPart == BodyPart.Head)
            adjusted += 0.9f;

        if (context.receiver != null)
        {
            Fighter.BodyPartData rightLeg = context.receiver.GetBodyPart(BodyPart.RightLeg);
            Fighter.BodyPartData leftLeg = context.receiver.GetBodyPart(BodyPart.LeftLeg);

            bool rightLegDestroyed = rightLeg != null && rightLeg.IsDestroyed;
            bool leftLegDestroyed = leftLeg != null && leftLeg.IsDestroyed;

            if (leftLegDestroyed && rightLegDestroyed)
                adjusted -= 1f;
            else if (leftLegDestroyed || rightLegDestroyed)
                adjusted -= 0.4f;
        }

        return Mathf.Clamp01(adjusted);
    }

    public bool CanTriggerSynergy(DamageCalculationContext context)
    {
        if (context.receiver == null)
            return false;

        if (context.targetPart != BodyPart.None)
            return CanAnyRuleApply(context, context.receiver.GetBodyPart(context.targetPart));

        if (context.receiver.bodyParts == null)
            return false;

        foreach (Fighter.BodyPartData part in context.receiver.bodyParts)
        {
            if (CanAnyRuleApply(context, part))
                return true;
        }

        return false;
    }

    private float CalculateBaseAmount(DamageCalculationContext context)
    {
        switch (context.modType)
        {
            case HealthModType.STAT_BASED:
                return CalculateStatBasedAmount(context);
            case HealthModType.FIXED:
                return context.amount;
            case HealthModType.PERCENTAGE:
                Stats receiverStats = context.receiver != null ? context.receiver.GetCurrentStats() : null;
                return receiverStats != null ? receiverStats.maxHealth * context.amount : context.amount;
        }

        throw new System.InvalidOperationException("StandardDamageCalculator::CalculateBaseAmount. Unreachable!");
    }

    private float CalculateStatBasedAmount(DamageCalculationContext context)
    {
        Stats emitterStats = context.emitter != null ? context.emitter.GetCurrentStats() : null;
        Stats receiverStats = context.receiver != null ? context.receiver.GetCurrentStats() : null;
        if (emitterStats == null || receiverStats == null)
            return context.amount;

        float receiverDefense = Mathf.Max(1f, receiverStats.deffense);
        float rawDamage = (((2 * emitterStats.level) / 5f) + 2f) * context.amount * (emitterStats.attack / receiverDefense);

        return (rawDamage / 50f) + 2f;
    }

    private float CalculateCriticalChance(DamageCalculationContext context)
    {
        Stats emitterStats = context.emitter != null ? context.emitter.GetCurrentStats() : null;
        float spirit = emitterStats != null ? Mathf.Max(0f, emitterStats.spirit) : 0f;
        float softCap = Mathf.Max(0.0001f, context.spiritSoftCap);
        float spiritBonus = (spirit / (spirit + softCap)) * Mathf.Clamp01(context.maxSpiritCritBonus);

        return Mathf.Clamp01(Mathf.Clamp01(context.baseCritChance) + spiritBonus);
    }

    private Fighter.BodyPartData ResolveTargetPart(DamageCalculationContext context)
    {
        if (context.receiver == null || context.targetPart == BodyPart.None)
            return null;

        return context.receiver.GetBodyPart(context.targetPart);
    }

    private bool CanAnyRuleApply(DamageCalculationContext context, Fighter.BodyPartData targetPart)
    {
        if (targetPart == null || targetPart.IsDestroyed)
            return false;

        foreach (IDamageSynergyRule rule in synergyRules)
        {
            if (rule.CanApply(context, targetPart))
                return true;
        }

        return false;
    }

    private SynergyApplication ApplySynergy(
        DamageCalculationContext context,
        Fighter.BodyPartData targetPart,
        ref float amount)
    {
        if (targetPart == null || targetPart.IsDestroyed)
            return SynergyApplication.None;

        foreach (IDamageSynergyRule rule in synergyRules)
        {
            if (rule.TryApply(context, targetPart, ref amount, out SynergyApplication application))
                return application;
        }

        if (context.statusToApply != PartStatus.None)
        {
            return new SynergyApplication(
                true,
                context.statusToApply,
                new[] { $"{targetPart.part} is now {context.statusToApply}" });
        }

        return SynergyApplication.None;
    }
}

public interface IDamageSynergyRule
{
    bool CanApply(DamageCalculationContext context, Fighter.BodyPartData targetPart);
    bool TryApply(
        DamageCalculationContext context,
        Fighter.BodyPartData targetPart,
        ref float amount,
        out SynergyApplication application);
}

public struct SynergyApplication
{
    public static readonly SynergyApplication None = new SynergyApplication(false, PartStatus.None, new string[0]);

    public readonly bool hasStatusChange;
    public readonly PartStatus resultingStatus;
    public readonly string[] messages;

    public SynergyApplication(bool hasStatusChange, PartStatus resultingStatus, string[] messages)
    {
        this.hasStatusChange = hasStatusChange;
        this.resultingStatus = resultingStatus;
        this.messages = messages ?? new string[0];
    }
}

public class CorrodedKineticSynergyRule : IDamageSynergyRule
{
    public bool CanApply(DamageCalculationContext context, Fighter.BodyPartData targetPart)
    {
        return targetPart != null &&
               !targetPart.IsDestroyed &&
               targetPart.currentStatus == PartStatus.Corroded &&
               context.damageType == DamageType.Kinetic;
    }

    public bool TryApply(
        DamageCalculationContext context,
        Fighter.BodyPartData targetPart,
        ref float amount,
        out SynergyApplication application)
    {
        if (!CanApply(context, targetPart))
        {
            application = SynergyApplication.None;
            return false;
        }

        amount *= 2.5f;
        application = new SynergyApplication(true, PartStatus.Bleeding, new[] { "EXTREME CRIT!" });
        return true;
    }
}

public class CorrodedThermalSynergyRule : IDamageSynergyRule
{
    public bool CanApply(DamageCalculationContext context, Fighter.BodyPartData targetPart)
    {
        return targetPart != null &&
               !targetPart.IsDestroyed &&
               targetPart.currentStatus == PartStatus.Corroded &&
               context.damageType == DamageType.Thermal;
    }

    public bool TryApply(
        DamageCalculationContext context,
        Fighter.BodyPartData targetPart,
        ref float amount,
        out SynergyApplication application)
    {
        if (!CanApply(context, targetPart))
        {
            application = SynergyApplication.None;
            return false;
        }

        amount *= 1.5f;
        application = new SynergyApplication(true, PartStatus.Burning, new[] { "COMBUSTION QUIMICA!" });
        return true;
    }
}
