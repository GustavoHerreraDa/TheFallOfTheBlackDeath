using System.Collections.Generic;
using UnityEngine;

public enum HealthModType
{
    STAT_BASED, FIXED, PERCENTAGE
}

public class HealthModSkill : Skill 
{
    public List<InventoryManager.InventoryObjectID> inventory;



    [Header("Health Mod")]
    public float amount;

    public HealthModType modType;
    [Range(0f, 1f)]
    public float critiChance = 0;

    protected override void OnRun(Fighter receiver)
    {

        float amount = this.GetModification();

        float dice = Random.Range(0f, 1f);
        if (dice <= this.critiChance)
        {
            amount *= 2f;
            this.messages.Enqueue("Critical hit");

        }

        if
        (this.selfInflicted)
        {
            this.messages.Enqueue("Heal for " + amount);
        }
        else
        {
            this.messages.Enqueue("Hit for " + (int) amount);
        }
        this.receiver.ModifyHealth(amount);

    }

    public float GetModification()
    {
        switch (this.modType)
        {
            case HealthModType.STAT_BASED:
                Stats emitterStats = this.emitter.GetCurrentStats();
                Stats receiverStats = this.receiver.GetCurrentStats();

                // Fórmula: https://bulbapedia.bulbagarden.net/wiki/Damage
                float rawDamage = (((2 * emitterStats.level) / 5) + 2) * this.amount * (emitterStats.attack / receiverStats.deffense);

                return (rawDamage / 50) + 2;
            case HealthModType.FIXED:
                return this.amount;
            case HealthModType.PERCENTAGE:
                Stats rStats = this.receiver.GetCurrentStats();

                return rStats.maxHealth * this.amount;
        }

        throw new System.InvalidOperationException("HealthModSkill::GetDamage. Unreachable!");
    }
}