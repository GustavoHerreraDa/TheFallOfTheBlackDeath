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
        float adjustedMissChance = GetAdjustedMissChance(receiver);

        // ❌ Fallo
        if (dice <= adjustedMissChance)
        {
            this.messages.Enqueue($"{emitter.idName} missed the attack on {receiver.idName}!");
            Debug.Log($"{emitter.idName} missed the attack on {receiver.idName}");
            return; // 🔥 no hacemos nada más
        }

        // 🎯 Crítico
        if (dice <= adjustedMissChance + this.critChance)
        {
            amount *= 2f;
            this.messages.Enqueue("Critical hit!");
            this.messages.Enqueue($"Hit for {(int)amount} to {receiver.idName}");
        }
        else
        {
            this.messages.Enqueue($"Hit for {(int)amount} to {receiver.idName}");
        }

        // 💥 Si el ataque tiene una parte del cuerpo objetivo, aplicamos ahí
        if (this.BodyPartTarget != BodyPart.None)
        {
            receiver.ModifyBodyPartHealth(this.BodyPartTarget, amount);
            this.messages.Enqueue($"{emitter.idName} hit on {this.BodyPartTarget}!");
        }
        else
        {
            receiver.ModifyHealth(amount);
        }
    }

    /// <summary>
    /// Ajusta la probabilidad de fallo según el contexto:
    /// - Si se apunta a la cabeza → aumenta chance de fallo.
    /// - Si el objetivo tiene las piernas destruidas → reduce chance de fallo.
    /// </summary>
    private float GetAdjustedMissChance(Fighter receiver)
    {
        float adjusted = missChance;

        // Si atacamos a la cabeza → +90% de chance de fallo (difícil de acertar)
        if (this.BodyPartTarget == BodyPart.Head)
            adjusted += 0.9f;

        // Si el receptor tiene una pierna destruida → -75% de chance de fallo (más fácil de acertar)
        Fighter.BodyPartData rightLeg = receiver.GetBodyPart(BodyPart.RightLeg);
        Fighter.BodyPartData leftLeg = receiver.GetBodyPart(BodyPart.LeftLeg);

        bool rightLegDestroyed = rightLeg != null && rightLeg.IsDestroyed;
        bool leftLegDestroyed = leftLeg != null && leftLeg.IsDestroyed;

        // Si ambas piernas están destruidas → penalización extra a los fallos de cabeza
        if (leftLegDestroyed && rightLegDestroyed && this.BodyPartTarget == BodyPart.Head)
        {
            adjusted -= 0.6f; // reduce bastante la probabilidad de fallar a la cabeza
        }
        else if (leftLegDestroyed || rightLegDestroyed)
        {
            adjusted -= 0.75f; // si al menos una pierna está destruida
        }

        // Clamp para mantener entre 0 y 1
        adjusted = Mathf.Clamp01(adjusted);

        return adjusted;
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
