using UnityEngine;
//TP2 FACUNDO FERREIRO
public enum HealthModType
{
    STAT_BASED, FIXED, PERCENTAGE
}

public class HealthModSkill : Skill
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

protected override void OnRun(Fighter receiver)
    {
        float baseDmg = this.GetModification(receiver);
        float dice = Random.Range(0f, 1f);
        float adjustedMissChance = GetAdjustedMissChance(receiver);
        Vector3 textPos = receiver.transform.position + Vector3.up * 2f;

        // 1. CHEQUEO DE FALLO
        if (dice <= adjustedMissChance)
        {
            this.messages.Enqueue($"{emitter.idName} missed on {receiver.idName}!");
            FloatingTextManager.Instance.ShowText("Miss!", textPos, Color.gray);
            receiver.ModifyHealth(0);                
            return;
        }

        // 2. CHEQUEO DE CRÍTICO Y GAME FEEL
        bool isCrit = (dice <= adjustedMissChance + this.critChance);
        if (isCrit)
        {
            baseDmg *= 2f; // Duplicamos el daño base si es crítico
            this.messages.Enqueue("Critical hit!");
            CameraManager.Instance.TriggerHitStop(0.15f);
            CameraManager.Instance.TriggerShake(2.5f);
            
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySFX(AudioManager.Instance.hitCriticalSound, 1f);
        }
        else
        {
            CameraManager.Instance.TriggerShake(1f);
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
                    receiver.ModifyBodyPartHealth(part.part, finalPartDmg);
                    totalAreaDmg += finalPartDmg;
                }
            }
            
            this.messages.Enqueue($"Hit for {(int)totalAreaDmg} to {receiver.idName} (AoE)");
            FloatingTextManager.Instance.ShowText($"-{(int)totalAreaDmg}", textPos, isCrit ? Color.yellow : Color.red, isCrit);
        }
        else if (this.BodyPartTarget != BodyPart.None)
        {
            Fighter.BodyPartData targetPart = receiver.GetBodyPart(this.BodyPartTarget);
            if (targetPart != null && !targetPart.IsDestroyed)
            {
                float finalDmg = ApplySynergy(targetPart, baseDmg);
                receiver.ModifyBodyPartHealth(this.BodyPartTarget, finalDmg);
                
                this.messages.Enqueue($"Hit for {(int)finalDmg} to {receiver.idName}");
                FloatingTextManager.Instance.ShowText($"-{(int)finalDmg}", textPos, isCrit ? Color.yellow : Color.red, isCrit);
            }
        }
        else
        {
            // ATAQUE GENERAL (A la vida base, sin apuntar a extremidades)
            receiver.ModifyHealth(baseDmg);
            this.messages.Enqueue($"Hit for {(int)baseDmg} to {receiver.idName}");
            FloatingTextManager.Instance.ShowText($"-{(int)baseDmg}", textPos, isCrit ? Color.yellow : Color.red, isCrit);
        }
    }

    // --- FUNCIÓN AUXILIAR PARA LAS SINERGIAS ---
    // Mantiene tu OnRun limpio y se asegura de que la matemática sea igual para todos
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
            this.messages.Enqueue($"{targetPart.part} ahora está {this.statusToApply}");
        }

        return dmg;
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

                // Fórmula: https://bulbapedia.bulbagarden.net/wiki/Damage
                float rawDamage = (((2 * emitterStats.level) / 5) + 2) * this.amount * (emitterStats.attack / receiverStats.deffense);

                return (rawDamage / 50) + 2;
            case HealthModType.FIXED:
                return this.amount;
            case HealthModType.PERCENTAGE:
                Stats rStats = receiver.GetCurrentStats();

                return rStats.maxHealth * this.amount;
        }

        throw new System.InvalidOperationException("HealthModSkill::GetDamage. Unreachable!");
    }
    
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
                else if (targetPart.currentStatus == PartStatus.Corroded && this.damageType == DamageType.Thermal)
                {
                    baseDmg *= 1.5f; // Multiplicador de Combustión
                }
                
                // (Si en el futuro agregas más sinergias, como daño x2 si está Electrified, las agregas aquí también)
            }
        }

        return baseDmg;
    }

}
