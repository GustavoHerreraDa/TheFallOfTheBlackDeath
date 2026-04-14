using UnityEngine;

// TP2 FACUNDO FERREIRO
/// <summary>
/// Supports the combat system by handling healing and buff skill.
/// </summary>
public class HealingAndBuffSkill : Skill
{
    [Header("Healing and Buff")]
    public float healAmount;
    public float attackBuffAmount;
    public float defenseBuffAmount;

    /// <summary>
    /// Executes the on run workflow.
    /// </summary>
    /// <param name="receiver">The receiver.</param>
    protected override void OnRun(Fighter receiver)
    {
        // Realizar curación al emisor
        emitter.ModifyHealth((int)healAmount);

        // Aplicar el aumento de ataque y defensa al emisor
        ApplyBuff(emitter);

        // Mostrar mensajes de habilidad, curación y buffs
        messages.Enqueue("You have been healed for " + (int)healAmount + " health.");
        messages.Enqueue("Your attack and defense have increased!");

        // Reproducir animación de habilidad
        emitter.animator.Play(animationName);
    }

    /// <summary>
    /// Applies the buff.
    /// </summary>
    /// <param name="emitter">The emitter.</param>
    private void ApplyBuff(Fighter emitter)
    {
        // Crear nuevos objetos StatusMod para el aumento de ataque y defensa
        StatusMod attackBuff = gameObject.AddComponent<StatusMod>();
        attackBuff.type = StatusModType.ATTACK_MOD;
        attackBuff.amount = attackBuffAmount;

        StatusMod defenseBuff = gameObject.AddComponent<StatusMod>();
        defenseBuff.type = StatusModType.DEFFENSE_MOD;
        defenseBuff.amount = defenseBuffAmount;

        // Agregar los objetos StatusMod al luchador emisor
        emitter.statusMods.Add(attackBuff);
        emitter.statusMods.Add(defenseBuff);
    }
}
