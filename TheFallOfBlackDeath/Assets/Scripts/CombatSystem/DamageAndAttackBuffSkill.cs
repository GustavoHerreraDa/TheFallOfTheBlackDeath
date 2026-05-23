using UnityEngine;
//TP2 FACUNDO FERREIRO
/// <summary>
/// Supports the combat system by handling damage and attack buff skill.
/// </summary>
public class DamageAndAttackBuffSkill : Skill
{
    [Header("Damage and Attack Buff")]
    public float damageAmount;
    public float attackBuffAmount;

    /// <summary>
    /// Executes the on run workflow.
    /// </summary>
    /// <param name="receiver">The receiver.</param>
    protected override void OnRun(Fighter receiver)
    {
        // Calcular el daño causado
        float damage = CalculateDamage(receiver);

        // Aplicar el aumento de ataque al emisor
        ApplyAttackBuff(emitter);

        // Mostrar mensajes de habilidad y daño
        messages.Enqueue("Hit for " + (int)damage + " to " + receiver.idName);
        messages.Enqueue("Your attack has increased!");

        // Reproducir animación de habilidad
        emitter.animator.Play(animationName);

        // Aplicar el daño
        receiver.ModifyHealth(-damage, this.emitter, this);
    }

    /// <summary>
    /// Executes the calculate damage workflow.
    /// </summary>
    /// <param name="receiver">The receiver.</param>
    /// <returns>The resulting value.</returns>
    private float CalculateDamage(Fighter receiver)
    {
        Stats emitterStats = emitter.GetCurrentStats();
        Stats receiverStats = receiver.GetCurrentStats();

        // Fórmula de cálculo de daño (puedes ajustarla según tus necesidades)
        float rawDamage = emitterStats.attack * damageAmount / receiverStats.deffense;

        return rawDamage;
    }

    /// <summary>
    /// Applies the attack buff.
    /// </summary>
    /// <param name="emitter">The emitter.</param>
    private void ApplyAttackBuff(Fighter emitter)
    {
        // Crear un nuevo objeto StatusMod para el aumento de ataque
        StatusMod attackBuff = gameObject.AddComponent<StatusMod>();
        attackBuff.type = StatusModType.ATTACK_MOD;
        attackBuff.amount = attackBuffAmount;

        // Agregar el objeto StatusMod al luchador emisor
        emitter.statusMods.Add(attackBuff);
    }
}
