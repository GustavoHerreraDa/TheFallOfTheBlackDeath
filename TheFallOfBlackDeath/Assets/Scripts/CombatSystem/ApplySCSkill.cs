using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Supports the combat system by handling apply sc skill.
/// </summary>
public class ApplySCSkill : BodyPartTargetSkill
{
    [Header("Initial Body Part Damage")]
    public float damageAmount = 0f;
    public DamageType damageType = DamageType.Kinetic;
    public HealthModType modType = HealthModType.FIXED;

    [Header("Damage Calculation Settings")]
    [Range(0f, 1f)] public float baseCritChance = 0.05f;
    public float spiritSoftCap = 50f;
    [Range(0f, 1f)] public float maxSpiritCritBonus = 0.40f;
    [Range(0f, 1f)] public float missChance = 0f;

    private IDamageCalculator damageCalculator = new StandardDamageCalculator();
    private BodyPartStatusCondition condition;

    protected override void OnRun(Fighter receiver)
    {
        if (this.condition == null)
        {
            this.condition = this.GetComponentInChildren<BodyPartStatusCondition>();

            if (this.condition == null)
            {
                throw new System.InvalidOperationException(
                    $"{name} needs a child object with a BodyPartStatusCondition component."
                );
            }

            if (this.condition.gameObject == this.gameObject)
            {
                throw new System.InvalidOperationException(
                    "The BodyPartStatusCondition should be a child of the skill object because it needs to be cloned"
                );
            }
        }

        if (!this.TryGetTargetBodyPart(receiver, out Fighter.BodyPartData targetPartData))
        {
            return;
        }

        // 1. SIEMPRE calculamos el contexto, incluso si el daño es 0, para evaluar el Miss Chance.
        DamageCalculationContext context = new DamageCalculationContext(
            this.emitter,
            receiver,
            this,
            this.BodyPartTarget,
            -damageAmount, // Será 0 si la habilidad no hace daño directo
            this.modType,
            this.damageType,
            PartStatus.None,
            this.baseCritChance,
            this.spiritSoftCap,
            this.maxSpiritCritBonus,
            this.missChance,
            true // Permitimos la aleatoriedad para calcular evasión/fallo
        );

        DamageResult result = damageCalculator.Calculate(context);

        // 2. Aplicamos el resultado al receptor (esto disparará el texto de "Miss!" o el daño numérico)
        receiver.ModifyBodyPartHealth(this.BodyPartTarget, result, this.emitter, this);

        // 3. LA CORRECCIÓN: Si el ataque falló, salimos de la función inmediatamente.
        if (result.isMiss)
        {
            this.messages.Enqueue($"{emitter.idName} falló al intentar aplicar estado a {receiver.idName}!");
            return; 
        }

        // Encolar mensajes de daño si el ataque conectó e hizo más de 0 de daño
        if (damageAmount > 0f)
        {
            this.messages.Enqueue($"Hit {receiver.idName}'s {this.BodyPartTarget} for {Mathf.Abs(Mathf.RoundToInt(result.appliedAmount))}");
            if (result.messages != null)
            {
                foreach (string msg in result.messages)
                    if (!string.IsNullOrEmpty(msg)) this.messages.Enqueue(msg);
            }
        }

        // 4. Si el código llega hasta aquí, significa que el ataque conectó. Aplicamos el estado alterado.
        BodyPartStatusCondition existingCondition = receiver.GetCurrentBodyPartStatusCondition(this.condition.GetType(), this.BodyPartTarget);
        if (existingCondition != null)
        {
            existingCondition.AddStack();
            this.messages.Enqueue(existingCondition.GetReceptionMessage());
            return;
        }

        GameObject go = Instantiate(this.condition.gameObject);
        go.transform.SetParent(receiver.transform);

        BodyPartStatusCondition clonedCondition = go.GetComponent<BodyPartStatusCondition>();
        clonedCondition.SetContext(receiver, this.BodyPartTarget);
        receiver.AddBodyPartStatusCondition(clonedCondition);

        this.messages.Enqueue(clonedCondition.GetReceptionMessage());
    }
}