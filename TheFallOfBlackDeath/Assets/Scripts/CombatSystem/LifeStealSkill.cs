using UnityEngine;
//TP2 FACUNDO FERREIRO
/// <summary>
/// Supports the combat system by handling life steal skill.
/// </summary>
public class LifeStealSkill : Skill
{
    [Header("Life Steal")]
    public float lifeStealPercentage;
    public float amount;

    /// <summary>
    /// Executes the on run workflow.
    /// </summary>
    /// <param name="receiver">The receiver.</param>
    protected override void OnRun(Fighter receiver)
    {
        float damage = GetDamage(receiver);

        float healedAmount = damage * lifeStealPercentage;
        float remainingDamage = damage - healedAmount;

   

        messages.Enqueue("Hit for " + (int)remainingDamage + " to " + receiver.idName);
        messages.Enqueue("Stole " + (int)healedAmount + " life from " + receiver.idName);

        receiver.ModifyHealth(-(int)remainingDamage);
        emitter.ModifyHealth((int)healedAmount);
    }

    /// <summary>
    /// Gets the damage.
    /// </summary>
    /// <param name="receiver">The receiver.</param>
    /// <returns>The resulting value.</returns>
    protected float GetDamage(Fighter receiver)
    {
        Stats emitterStats = emitter.GetCurrentStats();
        Stats receiverStats = receiver.GetCurrentStats();

        float rawDamage = (((2 * emitterStats.level) / 5) + 2) * amount * (emitterStats.attack / receiverStats.deffense);
        return (rawDamage / 50) + 2;
    }
}
