using UnityEngine;

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
    [Range(0f, 1f)]
    public float missChance = 0f;

    protected override void OnRun(Fighter receiver)
    {
        // Valor base (siempre positivo desde GetModification)
        float value = Mathf.Abs(this.GetModification(receiver));

        // Consideramos que es curación si el enum dice Heal o si el campo amount > 0
        bool isHeal = this.skillType == SkillType.Heal || this.amount > 0f;

        Vector3 textPos = receiver.transform.position + Vector3.up * 2f;

        if (isHeal)
        {
            // --- CURACIÓN ---

            // Si targeting global tipo "ALL_*" -> curar todas las partes NO destruidas
            if (this.targeting == SkillTargeting.ALL_ALLIES || this.targeting == SkillTargeting.ALL_OPPONENTS)
            {
                int healedParts = 0;

                if (receiver.bodyParts != null && receiver.bodyParts.Count > 0)
                {
                    foreach (var partData in receiver.bodyParts)
                    {
                        if (partData == null) continue;

                        // No curamos partes destruidas
                        if (partData.IsDestroyed) continue;

                        receiver.ModifyBodyPartHealth(partData.part, value);
                        healedParts++;
                        // Muestra texto en el punto del hit si existe, sino en textPos
                        Vector3 pos = partData.hitPoint != null ? partData.hitPoint.position : textPos;
                        FloatingTextManager.Instance.ShowText($"+{(int)value}", pos, Color.green);
                    }
                }
                else
                {
                    // Si no tiene partes, fallback a curar salud global
                    receiver.ModifyHealth(value);
                    FloatingTextManager.Instance.ShowText($"+{(int)value} HP", textPos, Color.green);
                }

                this.messages.Enqueue($"{emitter.idName} curó {healedParts} partes de {receiver.idName} (+{(int)value} cada una).");
            }
            else
            {
                // SINGLE_* o AUTO -> curar sólo BodyPartTarget si está definido y no destruido
                if (this.BodyPartTarget != BodyPart.None)
                {
                    var targetPart = receiver.GetBodyPart(this.BodyPartTarget);
                    if (targetPart == null)
                    {
                        // No existe la parte -> fallback a curar salud global
                        receiver.ModifyHealth(value);
                        FloatingTextManager.Instance.ShowText($"+{(int)value} HP", textPos, Color.green);
                        this.messages.Enqueue($"{emitter.idName} curó a {receiver.idName} (+{(int)value} HP) (parte objetivo no encontrada).");
                    }
                    else if (targetPart.IsDestroyed)
                    {
                        // Parte destruida => no se puede curar
                        this.messages.Enqueue($"{emitter.idName} intentó curar {receiver.idName} en {this.BodyPartTarget}, pero está destruida.");
                        FloatingTextManager.Instance.ShowText("Can't heal (destroyed part)", textPos, Color.gray);
                    }
                    else
                    {
                        // Parte existente y no destruida -> aplicar curación
                        receiver.ModifyBodyPartHealth(this.BodyPartTarget, value);
                        FloatingTextManager.Instance.ShowText($"+{(int)value}", targetPart.hitPoint != null ? targetPart.hitPoint.position : textPos, Color.green);
                        this.messages.Enqueue($"{emitter.idName} curó {receiver.idName} en {this.BodyPartTarget} (+{(int)value}).");
                    }
                }
                else
                {
                    // No hay BodyPartTarget -> curación global sobre salud
                    receiver.ModifyHealth(value);
                    FloatingTextManager.Instance.ShowText($"+{(int)value} HP", textPos, Color.green);
                    this.messages.Enqueue($"{emitter.idName} curó a {receiver.idName} (+{(int)value} HP).");
                }
            }

            return;
        }

        // --- DAÑO: mantenemos miss/crit y aplicamos daño a la parte o al health global ---
        float dice = Random.Range(0f, 1f);
        float adjustedMissChance = GetAdjustedMissChance(receiver);

        if (dice <= adjustedMissChance)
        {
            this.messages.Enqueue($"{emitter.idName} missed on {receiver.idName}!");
            FloatingTextManager.Instance.ShowText("Miss!", textPos, Color.gray);
            receiver.ModifyHealth(0);
            return;
        }

        bool isCrit = dice <= adjustedMissChance + this.critChance;
        if (isCrit)
        {
            value *= 2f;
            this.messages.Enqueue("Critical hit!");
            FloatingTextManager.Instance.ShowText($"-{(int)value}!", textPos, Color.yellow);
        }
        else
        {
            this.messages.Enqueue($"Hit for {(int)value} to {receiver.idName}");
            FloatingTextManager.Instance.ShowText($"-{(int)value}", textPos, Color.red);
        }

        // Aplicar daño negativo sobre la parte / salud global
        if (this.BodyPartTarget != BodyPart.None)
        {
            var targetPart = receiver.GetBodyPart(this.BodyPartTarget);
            if (targetPart != null)
            {

                receiver.ModifyBodyPartHealth(this.BodyPartTarget, -value);
                this.messages.Enqueue($"{emitter.idName} hit {receiver.idName} on {this.BodyPartTarget} (-{(int)value}).");
            }
            else
            {
                // fallback a salud global si no existe la parte
                receiver.ModifyHealth(-value);
            }
        }
        else
        {
            receiver.ModifyHealth(-value);
        }
    }

    private float GetAdjustedMissChance(Fighter receiver)
    {
        float adjusted = missChance;

        if (this.BodyPartTarget == BodyPart.Head)
            adjusted += 0.9f;

        Fighter.BodyPartData rightLeg = receiver.GetBodyPart(BodyPart.RightLeg);
        Fighter.BodyPartData leftLeg = receiver.GetBodyPart(BodyPart.LeftLeg);

        bool rightLegDestroyed = rightLeg != null && rightLeg.IsDestroyed;
        bool leftLegDestroyed = leftLeg != null && leftLeg.IsDestroyed;

        if (leftLegDestroyed && rightLegDestroyed && this.BodyPartTarget == BodyPart.Head)
        {
            adjusted -= 1;
        }
        else if (leftLegDestroyed || rightLegDestroyed)
        {
            adjusted -= 0.5f;
        }

        return Mathf.Clamp01(adjusted);
    }

    public float GetModification(Fighter receiver)
    {
        switch (this.modType)
        {
            case HealthModType.STAT_BASED:
                Stats emitterStats = this.emitter.GetCurrentStats();
                Stats receiverStats = receiver.GetCurrentStats();

                float rawDamage = (((2 * emitterStats.level) / 5) + 2) * this.amount * (emitterStats.attack / receiverStats.deffense);
                return Mathf.Abs((rawDamage / 50) + 2);

            case HealthModType.FIXED:
                return Mathf.Abs(this.amount);

            case HealthModType.PERCENTAGE:
                Stats rStats = receiver.GetCurrentStats();
                return Mathf.Abs(rStats.maxHealth * this.amount);
        }

        throw new System.InvalidOperationException("HealthModSkill::GetDamage. Unreachable!");
    }
}
