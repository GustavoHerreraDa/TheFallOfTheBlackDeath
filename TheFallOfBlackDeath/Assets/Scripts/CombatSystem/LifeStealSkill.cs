using UnityEngine;
//TP2 FACUNDO FERREIRO
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

        receiver.ModifyHealth(-(int)remainingDamage); // REDUCE LA VIDA DEL RECEIVER
        emitter.ModifyHealth((int)healedAmount); // AUMENTA LA VIDA DEL EMISOR
    }

    protected float GetDamage(Fighter receiver)
    {
        Stats emitterStats = emitter.GetCurrentStats();
        Stats receiverStats = receiver.GetCurrentStats();

        float rawDamage = (((2 * emitterStats.level) / 5) + 2) * amount * (emitterStats.attack / receiverStats.deffense);
        return (rawDamage / 50) + 2;
    }
}