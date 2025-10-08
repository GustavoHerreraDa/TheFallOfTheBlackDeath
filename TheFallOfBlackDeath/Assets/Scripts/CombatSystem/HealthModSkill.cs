using UnityEngine;
//TP2 FACUNDO FERREIRO
public enum HealthModType
{
    STAT_BASED, FIXED, PERCENTAGE
}

public class HealthModSkill : Skill
{
    [Header("Health Mod")]
    public float amount;

    public HealthModType modType;

    [Range(0f, 1f)]
    public float critChance = 0;
    [Range(0f, 1f)] public float missChance = 0f;
    bool missedAttack = false;

    protected override void OnRun(Fighter receiver)
    {
        float amount = this.GetModification(receiver);
        float dice = Random.Range(0f, 1f);
        bool missedAttack = false;

        // ❌ Fallo
        if (dice <= missChance)
        {
            missedAttack = true;
            this.messages.Enqueue($"{emitter.idName} missed the attack on {receiver.idName}!");
            Debug.Log($"{emitter.idName} miss the attack {receiver.idName}");
        }

        if (!missedAttack)
        {
            // 🎯 Crítico
            if (dice <= this.critChance + this.missChance && dice > this.missChance)
            {
                amount *= 2f;
                this.messages.Enqueue("Critical hit!");
                this.messages.Enqueue($"Hit for {(int)amount} to {receiver.idName}");
            }
            else
            {
                if (skillType == SkillType.Heal)
                    this.messages.Enqueue($"Heal for {(int)amount} to {receiver.idName}");
                else
                    this.messages.Enqueue($"Hit for {(int)amount} to {receiver.idName}");
            }

            // 💥 Aplicar el daño solo si no falló
            receiver.ModifyHealth((int)amount);
        }
    }


    public float GetModification(Fighter receiver)
    {
        switch (this.modType)
        {
            case HealthModType.STAT_BASED:
                Stats emitterStats = this.emitter.GetCurrentStats();
                Stats receiverStats = receiver.GetCurrentStats();

                // Fórmula de daño estilo Pokémon
                float rawDamage = (((2 * emitterStats.level) / 5) + 2) *
                                  this.amount *
                                  (emitterStats.attack / receiverStats.deffense);

                return (rawDamage / 50) + 2;

            case HealthModType.FIXED:
                return this.amount;

            case HealthModType.PERCENTAGE:
                Stats rStats = receiver.GetCurrentStats();
                return rStats.maxHealth * this.amount;
        }

        throw new System.InvalidOperationException("HealthModSkill::GetDamage. Unreachable!");
    }
}