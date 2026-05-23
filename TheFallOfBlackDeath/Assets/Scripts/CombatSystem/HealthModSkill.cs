using UnityEngine;
//TP2 FACUNDO FERREIRO
/// <summary>
/// Defines the named values used by health mod type.
/// </summary>
public enum HealthModType
{
    STAT_BASED, FIXED, PERCENTAGE
}

/// <summary>
/// Supports the combat system by handling health mod skill.
/// </summary>
public class HealthModSkill : BodyPartTargetSkill
{
    [Header("Synergy Settings")]
    public DamageType damageType;
    public PartStatus statusToApply = PartStatus.None;
    
    [Header("Health Mod")]
    public float amount;
    public HealthModType modType;

    [Range(0f, 1f)]
    public float critChance = 0;
    [Range(0f, 1f)] public float missChance = 0f;

    bool missedAttack = false;

    // Calculates final crit chance with emitter stats and Desperation passive
    /// <summary>
    /// Gets the adjusted crit chance.
    /// </summary>
    /// <returns>The resulting value.</returns>
    protected virtual float GetAdjustedCritChance()
    {
        float adjusted = Mathf.Clamp01(this.critChance);

        // Bonus from emitter's speed
        var stats = this.emitter != null ? this.emitter.GetCurrentStats() : null;
        if (stats != null)
        {
            adjusted += stats.speed / 200f;
        }

        // Desperation bonus if emitter is PlayerFighter and sanity is low
        var gm = GameManager.Instance;
        if (this.emitter is PlayerFighter && gm != null && gm.sanity != null && gm.sanity.IsInDesperation())
        {
            adjusted += 0.30f;
        }

        return Mathf.Clamp01(adjusted);
    }

/// <summary>
/// Executes the on run workflow.
/// </summary>
/// <param name="receiver">The receiver.</param>
protected override void OnRun(Fighter receiver)
    {
        float baseDmg = this.GetModification(receiver);
        float missRoll = Random.value;
        float adjustedMissChance = GetAdjustedMissChance(receiver);
        Vector3 textPos = receiver.transform.position + Vector3.up * 2f;

        // 1. CHEQUEO DE FALLO
        if (missRoll < adjustedMissChance)
        {
            FloatingTextManager.Instance.ShowText("Miss!", textPos, Color.gray);
            receiver.ModifyHealth(0, this.emitter, this);
            return;
        }

        // 2. CHEQUEO DE CRÍTICO Y GAME FEEL
        float adjustedCritChance = GetAdjustedCritChance();
        float denom = Mathf.Max(1f - adjustedMissChance, 0.0001f);
        float effectiveCritChance = Mathf.Clamp01(adjustedCritChance / denom);
        float critRoll = Random.value;
        bool isCrit = (critRoll < effectiveCritChance);
        if (isCrit)
        {
            baseDmg *= 2f; // Duplicamos el daño base si es crítico
            CameraManager.Instance.TriggerShake(1f);
            CameraManager.Instance.TriggerHitStop(0.15f);
            
            
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySFX(AudioManager.Instance.hitCriticalSound, 1f);
        }
        else
        {
            CameraManager.Instance.TriggerShake(0.6f);
            AudioClip hitSoundToPlay = this.customImpactSound != null ? this.customImpactSound : AudioManager.Instance.hitNormalSound;
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySFX(hitSoundToPlay, 0.7f);
        }

        // 3. APLICACIÓN DE DAÑO Y SINERGIAS
        if (this.targeting == SkillTargeting.ALL_OPPONENTS)
        {
            // ATAQUE EN ÁREA: Le pega a TODAS las partes no destruidas
            float totalAreaDmg = 0;
            foreach (var part in receiver.bodyParts)
            {
                if (!part.IsDestroyed)
                {
                    float finalPartDmg = ApplySynergy(part, baseDmg);
                    receiver.ModifyBodyPartHealth(part.part, finalPartDmg, this.emitter, this);
                    totalAreaDmg += finalPartDmg;
                }
            }
            
            FloatingTextManager.Instance.ShowText($"-{(int)totalAreaDmg}", textPos, isCrit ? Color.yellow : Color.red, isCrit);
        }
        /// <summary>
        /// Executes the if workflow.
        /// </summary>
        /// <param name="this.TryGetTargetBodyPart(receiver">The this.try get target body part(receiver.</param>
        /// <param name="targetPart)">The target part).</param>
        /// <returns>The resulting value.</returns>
        else if (this.TryGetTargetBodyPart(receiver, out Fighter.BodyPartData targetPart))
        {
            float finalDmg = ApplySynergy(targetPart, baseDmg);
            receiver.ModifyBodyPartHealth(this.BodyPartTarget, finalDmg, this.emitter, this);
            
            FloatingTextManager.Instance.ShowText($"-{(int)finalDmg}", textPos, isCrit ? Color.yellow : Color.red, isCrit);
        }
        
        else
        
        {
            return;
        }
    }

    /// <summary>
    /// Determines whether the component can trigger synergy.
    /// </summary>
    /// <param name="target">The target.</param>
    /// <param name="part">The part.</param>
    /// <returns>True when the requested condition is met; otherwise, false.</returns>
    public override bool CanTriggerSynergy(Fighter target, BodyPart part = BodyPart.None)
    {
        if (target == null) return false;

        if (part != BodyPart.None)
        {
            Fighter.BodyPartData targetPart = target.GetBodyPart(part);
            return IsSynergyPossible(targetPart);
        }
        else
        {
            // Si no se especifica parte, revisamos si alguna parte del enemigo permite sinergia
            foreach (var p in target.bodyParts)
            {
                if (!p.IsDestroyed && IsSynergyPossible(p))
                    return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Determines whether the component is synergy possible.
    /// </summary>
    /// <param name="targetPart">The target part.</param>
    /// <returns>True when the requested condition is met; otherwise, false.</returns>
    private bool IsSynergyPossible(Fighter.BodyPartData targetPart)
    {
        if (targetPart == null || targetPart.IsDestroyed) return false;

        if (targetPart.currentStatus == PartStatus.Corroded && (this.damageType == DamageType.Kinetic || this.damageType == DamageType.Thermal))
        {
            return true;
        }
        
        // Aquí se pueden agregar más condiciones en el futuro fácilmente
        
        return false;
    }

    // --- FUNCIÓN AUXILIAR PARA LAS SINERGIAS ---
    // Mantiene tu OnRun limpio y se asegura de que la matemática sea igual para todos
    /// <summary>
    /// Applies the synergy.
    /// </summary>
    /// <param name="targetPart">The target part.</param>
    /// <param name="dmg">The dmg.</param>
    /// <returns>The resulting value.</returns>
    private float ApplySynergy(Fighter.BodyPartData targetPart, float dmg)
    {
        bool synergyTriggered = false;

        if (targetPart.currentStatus == PartStatus.Corroded && this.damageType == DamageType.Kinetic)
        {
            dmg *= 2.5f; 
            synergyTriggered = true;
            targetPart.currentStatus = PartStatus.Bleeding; 
            this.messages.Enqueue("¡EXTREME CRÍTIC!");
        }
        /// <summary>
        /// Executes the if workflow.
        /// </summary>
        /// <param name="targetPart.currentStatus">The target part.current status.</param>
        /// <returns>The resulting value.</returns>
        else if (targetPart.currentStatus == PartStatus.Corroded && this.damageType == DamageType.Thermal)
        {
            dmg *= 1.5f;
            synergyTriggered = true;
            targetPart.currentStatus = PartStatus.Burning;
            this.messages.Enqueue("¡COMBUSTIÓN QUÍMICA!");
        }

        if (!synergyTriggered && this.statusToApply != PartStatus.None)
        {
            targetPart.currentStatus = this.statusToApply;
            this.messages.Enqueue($"{targetPart.part} Is now {this.statusToApply}");
        }

        return dmg;
    }

    /// <summary>
    /// Gets the adjusted miss chance.
    /// </summary>
    /// <param name="receiver">The receiver.</param>
    /// <returns>The resulting value.</returns>
    public float GetAdjustedMissChance(Fighter receiver)
    {
        float adjusted = missChance;


        if (this.BodyPartTarget == BodyPart.Head)
            adjusted += 0.9f;

 
        Fighter.BodyPartData rightLeg = receiver.GetBodyPart(BodyPart.RightLeg);
        Fighter.BodyPartData leftLeg = receiver.GetBodyPart(BodyPart.LeftLeg);

        bool rightLegDestroyed = rightLeg != null && rightLeg.IsDestroyed;
        bool leftLegDestroyed = leftLeg != null && leftLeg.IsDestroyed;

    
        if (leftLegDestroyed && rightLegDestroyed)
        {
            adjusted -= 1f; // más fácil pegar en general
        }
        else if (leftLegDestroyed || rightLegDestroyed)
        {
            adjusted -= 0.4f;
        }

        return Mathf.Clamp01(adjusted);
    }


    /// <summary>
    /// Gets the modification.
    /// </summary>
    /// <param name="receiver">The receiver.</param>
    /// <returns>The resulting value.</returns>
    public float GetModification(Fighter receiver)
    {
        switch (this.modType)
        {
            case HealthModType.STAT_BASED:
                Stats emitterStats = this.emitter != null ? this.emitter.GetCurrentStats() : null;
                Stats receiverStats = receiver != null ? receiver.GetCurrentStats() : null;
                if (emitterStats == null || receiverStats == null)
                {
                    return this.amount; // Fallback to base amount if stats are unavailable
                }

                float receiverDefense = receiverStats.deffense;
                var gm = GameManager.Instance;
                if (receiver is PlayerFighter && gm != null && gm.sanity != null && gm.sanity.IsInDesperation())
                {
                    receiverDefense *= 0.8f; // -20% defense under Desperation
                }

                receiverDefense = Mathf.Max(1f, receiverDefense); // avoid div by zero or extreme values

                // Fórmula: https://bulbapedia.bulbagarden.net/wiki/Damage
                float rawDamage = (((2 * emitterStats.level) / 5f) + 2f) * this.amount * (emitterStats.attack / receiverDefense);

                return (rawDamage / 50f) + 2f;
            case HealthModType.FIXED:
                return this.amount;
            case HealthModType.PERCENTAGE:
                Stats rStats = receiver.GetCurrentStats();

                return rStats.maxHealth * this.amount;
        }

        throw new System.InvalidOperationException("HealthModSkill::GetDamage. Unreachable!");
    }
    
    /// <summary>
    /// Gets the estimated damage.
    /// </summary>
    /// <param name="receiver">The receiver.</param>
    /// <param name="targetPartType">The target part type.</param>
    /// <returns>The resulting value.</returns>
    public float GetEstimatedDamage(Fighter receiver, BodyPart targetPartType)
    {
        // 1. Calculamos el daño base
        float baseDmg = this.GetModification(receiver);

        // 2. Simulamos la sinergia
        if (targetPartType != BodyPart.None)
        {
            Fighter.BodyPartData targetPart = receiver.GetBodyPart(targetPartType);
        
            if (targetPart != null && !targetPart.IsDestroyed)
            {
                // Copiamos la misma lógica matemática que tienes en el OnRun()
                if (targetPart.currentStatus == PartStatus.Corroded && this.damageType == DamageType.Kinetic)
                {
                    baseDmg *= 2.5f; // Multiplicador brutal
                }
        /// <summary>
        /// Executes the if workflow.
        /// </summary>
        /// <param name="targetPart.currentStatus">The target part.current status.</param>
        /// <returns>The resulting value.</returns>
                else if (targetPart.currentStatus == PartStatus.Corroded && this.damageType == DamageType.Thermal)
                {
                    baseDmg *= 1.5f; // Multiplicador de Combustión
                }
            }
        }

        return baseDmg;
    }
    
    

}
