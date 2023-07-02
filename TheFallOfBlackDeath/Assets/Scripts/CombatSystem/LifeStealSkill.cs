using UnityEngine;

public class LifeStealSkill : Skill
{
    [Header("Life Steal")]
    public float lifeStealPercentage;
    public float amount;

    protected override void OnRun(Fighter receiver)
    {
        float damage = GetDamage(receiver);

        float healedAmount = damage * lifeStealPercentage;
        float remainingDamage = damage - healedAmount;

   

        messages.Enqueue("Hit for " + (int)remainingDamage + " to " + receiver.idName);
        messages.Enqueue("Stole " + (int)healedAmount + " life from " + receiver.idName);

        receiver.ModifyHealth(-(int)remainingDamage); // Reduce receiver's health
        emitter.ModifyHealth((int)healedAmount); // Increase emitter's health
    }

    protected float GetDamage(Fighter receiver)
    {
        Stats emitterStats = emitter.GetCurrentStats();
        Stats receiverStats = receiver.GetCurrentStats();

        float rawDamage = (((2 * emitterStats.level) / 5) + 2) * amount * (emitterStats.attack / receiverStats.deffense);
        return (rawDamage / 50) + 2;
    }
}