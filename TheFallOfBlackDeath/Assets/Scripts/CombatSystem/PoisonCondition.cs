using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Supports the combat system by handling poison condition.
/// </summary>
public class PoisonCondition : BodyPartStatusCondition
{
    [Header("Poison Settings")]
    public float poisonDamage = 10f;

    private IDamageCalculator damageCalculator = new StandardDamageCalculator();

    /// <summary>
    /// Executes the on apply workflow.
    /// </summary>
    /// <returns>True when the requested condition is met; otherwise, false.</returns>
    public override bool OnApply()
    {
        if (receiver == null)
            return false;

        float totalDamage = poisonDamage * this.Stacks;

        // Use Damage Pipeline: construct context for DoT (True Damage / Bypass defense)
        DamageCalculationContext context = new DamageCalculationContext(
            null,
            receiver,
            null,
            this.TargetPart,
            -totalDamage,
            HealthModType.FIXED,
            DamageType.Chemical,
            PartStatus.None,
            0f, 0f, 0f, 0f,
            false // DoT bypasses miss/crit rolls
        );

        DamageResult result = damageCalculator.Calculate(context);
        receiver.ModifyBodyPartHealth(this.TargetPart, result, null, null);

        messages.Enqueue($"{receiver.idName} sufre {(int)totalDamage} de dano por Veneno en {this.TargetPart}.");

        return true;
    }
}
