using UnityEngine;

// TP2 FACUNDO FERREIRO
/// <summary>
/// Supports the combat system by handling protect skill.
/// </summary>
public class ProtectSkill : Skill
{
    private bool usedLastTurn = false;
    public float successRate = 0.8f; // Ajustar este valor según lo necesario

    /// <summary>
    /// Executes the on run workflow.
    /// </summary>
    /// <param name="receiver">The receiver.</param>
    protected override void OnRun(Fighter receiver)
    {
        if (!usedLastTurn)
        {
            // Protección tiene su éxito normal si no se usó en el turno anterior
            if (Random.value <= successRate)
            {
                ApplyProtection(emitter);
                messages.Enqueue(emitter.idName + " uses Protect! They are protected from attacks this turn.");
            }
            else
            {
                messages.Enqueue(emitter.idName + "'s Protect failed!");
            }
        }
        else
        {
            // Reducir el éxito en un 50% si se usó en el turno anterior
            float reducedSuccessRate = successRate * 0.5f;
            if (Random.value <= reducedSuccessRate)
            {
                // Protección falló este turno
                messages.Enqueue(emitter.idName + "'s Protect failed!");
            }
            else
            {
                // Protección tuvo éxito este turno
                ApplyProtection(emitter);
                messages.Enqueue(emitter.idName + " uses Protect! They are protected from attacks this turn.");
            }

            // Actualizar el estado para el próximo turno
            usedLastTurn = false;
        }

        // Reproducir animación de habilidad
        emitter.animator.Play(animationName);
    }

    /// <summary>
    /// Applies the protection.
    /// </summary>
    /// <param name="emitter">The emitter.</param>
    private void ApplyProtection(Fighter emitter)
    {
        // Crear un nuevo objeto StatusMod para el efecto de protección
        StatusMod protectionEffect = gameObject.AddComponent<StatusMod>();
        protectionEffect.type = StatusModType.DEFFENSE_MOD;
        protectionEffect.amount = 999; // Aumenta temporalmente la defensa a un valor muy alto

        // Agregar el objeto StatusMod al luchador emisor
        emitter.statusMods.Add(protectionEffect);
    }
}
