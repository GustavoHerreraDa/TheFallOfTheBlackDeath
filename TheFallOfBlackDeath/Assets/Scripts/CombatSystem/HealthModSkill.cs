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
        float dmg = this.GetModification(receiver);
        float dice = Random.Range(0f, 1f);
        float adjustedMissChance = GetAdjustedMissChance(receiver);

        Vector3 textPos = receiver.transform.position + Vector3.up * 2f;

        // --- LÓGICA DE SINERGIAS (NUEVO) ---
        bool synergyTriggered = false;

        if (this.BodyPartTarget != BodyPart.None)
        {
            Fighter.BodyPartData targetPart = receiver.GetBodyPart(this.BodyPartTarget);
        
            if (targetPart != null && !targetPart.IsDestroyed)
            {
                // EJEMPLO 1: Impacto sobre armadura derretida
                if (targetPart.currentStatus == PartStatus.Corroded && this.damageType == DamageType.Kinetic)
                {
                    dmg *= 2.5f; // Multiplicador brutal
                    synergyTriggered = true;
                    targetPart.currentStatus = PartStatus.Bleeding; // Evoluciona el estado
                    this.messages.Enqueue("¡ASTILLAMIENTO CRÍTICO! Armadura rota.");
                }
                // EJEMPLO 2: Combustión (Fuego sobre Químico)
                else if (targetPart.currentStatus == PartStatus.Corroded && this.damageType == DamageType.Thermal)
                {
                    dmg *= 1.5f;
                    synergyTriggered = true;
                    targetPart.currentStatus = PartStatus.Burning;
                    this.messages.Enqueue("¡COMBUSTIÓN QUÍMICA!");
                }
            
                // Si la habilidad aplica un estado nuevo y no hubo sinergia que lo sobreescriba
                if (!synergyTriggered && this.statusToApply != PartStatus.None)
                {
                    targetPart.currentStatus = this.statusToApply;
                    this.messages.Enqueue($"{targetPart.part} ahora está {this.statusToApply}");
                }
            }
        }
        
        if (dice <= adjustedMissChance)
        {
            this.messages.Enqueue($"{emitter.idName} missed on {receiver.idName}!");
            FloatingTextManager.Instance.ShowText("Miss!", textPos, Color.gray);
            receiver.ModifyHealth(0);                
            return;
        }

        
        if (dice <= adjustedMissChance + this.critChance)
        {
            dmg *= 2f;
            this.messages.Enqueue("Critical hit!");
            this.messages.Enqueue($"Hit for {(int)dmg} to {receiver.idName}");
            FloatingTextManager.Instance.ShowText($"-{(int)dmg}!", textPos, Color.yellow);
        }
        else
        {
            this.messages.Enqueue($"Hit for {(int)dmg} to {receiver.idName}");
            FloatingTextManager.Instance.ShowText($"-{(int)dmg}", textPos, Color.red);
        }

        if (this.BodyPartTarget != BodyPart.None)
        {
            receiver.ModifyBodyPartHealth(this.BodyPartTarget, dmg);
            this.messages.Enqueue($"{emitter.idName} hit on {this.BodyPartTarget}!");
        }
        else
        {
            receiver.ModifyHealth(dmg);
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

}
