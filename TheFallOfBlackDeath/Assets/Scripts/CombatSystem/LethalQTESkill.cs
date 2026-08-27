using System.Collections;
using UnityEngine;

/// <summary>
/// Lethal attack that gives the player a real-time parry window at the impact moment.
/// Resolves Miss (100% lethal damage), Guard (reduced non-lethal damage), and Parry (0% damage + counterattack).
/// </summary>
public class LethalQTESkill : BodyPartTargetSkill
{
    public static event System.Action OnLethalSkillExecuted;
    public static event System.Action OnLethalSkillFinished;

    [Header("Parry QTE")]
    [SerializeField] private float parryWindowDuration = 0.35f;
    [SerializeField] private float slowMoTimeScale = 0.2f;
    [SerializeField] private float lethalDamage = -99999f;
    [SerializeField] private float guardDamage = -35f;

    [Header("Damage")]
    [SerializeField] private DamageType damageType = DamageType.Kinetic;

    /// <summary>
    /// Waits until the animation impact frame, opens the parry QTE, then resolves the result (Miss, Guard, Parry).
    /// If Parry is achieved, enqueues an immediate counterattack before the enemy turn completes.
    /// </summary>
    protected override IEnumerator ApplyDamageDelayed(Fighter receiver, BodyPart cachedBodyPartTarget)
    {
        OnLethalSkillExecuted?.Invoke();

        if (impactDelay > 0f)
            yield return new WaitForSeconds(impactDelay);

        QTEParryResult qteResult = QTEParryResult.Miss;
        QTEParryManager parryManager = QTEParryManager.Instance;

        CameraFXManager fxManager = FindObjectOfType<CameraFXManager>();

        if (parryManager != null)
        {
            if (fxManager != null) fxManager.SetLethalWarning(true);

            yield return parryManager.WaitForParry(parryWindowDuration, slowMoTimeScale, result => qteResult = result);
        }
        else
        {
            Debug.LogWarning("[LethalQTESkill] No se encontró QTEParryManager en la escena. Se resolverá como Miss por defecto.");
        }

        if (fxManager != null) fxManager.SetLethalWarning(false);

        DamageResult result;

        switch (qteResult)
        {
            case QTEParryResult.Parry:
                result = CreateParryResult(receiver, cachedBodyPartTarget);
                receiver.ModifyHealth(result);
                EnqueueOutcomeMessage(receiver, qteResult);

                // Ejecución inmediata del contraataque justo en el momento de frenar el ataque enemigo
                if (receiver != null && receiver.combatManager != null && this.emitter != null)
                {
                    yield return receiver.combatManager.StartCoroutine(
                        receiver.combatManager.ExecuteCounterAttackRoutine(receiver, this.emitter)
                    );
                }
                break;

            case QTEParryResult.Guard:
                result = CreateGuardResult(receiver, cachedBodyPartTarget);
                receiver.ModifyHealth(result);
                EnqueueOutcomeMessage(receiver, qteResult);
                break;

            case QTEParryResult.Miss:
            default:
                result = CreateLethalResult(receiver, cachedBodyPartTarget);
                receiver.ModifyHealth(result);
                EnqueueOutcomeMessage(receiver, qteResult);
                break;
        }

        OnLethalSkillFinished?.Invoke();
    }

    protected override void OnRun(Fighter receiver)
    {
    }

    private DamageResult CreateParryResult(Fighter receiver, BodyPart targetPart)
    {
        return DamageResult.Create(
            this.emitter,
            receiver,
            this,
            targetPart,
            damageType,
            0f,
            0f,
            false,
            false,
            0f,
            0f,
            false,
            PartStatus.None,
            new[] { $"{receiver.idName} ejecuta un ¡PARRY PERFECTO! y desvía por completo {skillName}." });
    }

    private DamageResult CreateGuardResult(Fighter receiver, BodyPart targetPart)
    {
        return DamageResult.Create(
            this.emitter,
            receiver,
            this,
            targetPart,
            damageType,
            guardDamage,
            guardDamage,
            false,
            false,
            0f,
            0f,
            false,
            PartStatus.None,
            new[] { $"{receiver.idName} realiza una Guardia (Guard) mitigando el impacto letal a {guardDamage}." });
    }

    private DamageResult CreateLethalResult(Fighter receiver, BodyPart targetPart)
    {
        return new DamageResult
        {
            attacker = this.emitter,
            receiver = receiver,
            sourceSkill = this,
            targetPart = targetPart,
            damageType = damageType,
            baseAmount = lethalDamage,
            finalAmount = lethalDamage,
            appliedAmount = lethalDamage,
            previousHealth = 0f,
            currentHealth = 0f,
            critChance = 1f,
            missChance = 0f,
            isCritical = true,
            isMiss = false,
            affectedBodyPart = targetPart != BodyPart.None,
            destroyedBodyPart = false,
            hasStatusChange = false,
            resultingStatus = PartStatus.None,
            messages = new[] { $"{receiver.idName} falla la defensa y recibe el golpe letal completo." }
        };
    }

    private void EnqueueOutcomeMessage(Fighter receiver, QTEParryResult qteResult)
    {
        switch (qteResult)
        {
            case QTEParryResult.Parry:
                this.messages.Enqueue($"{receiver.idName} anula el ataque letal mediante un Parry perfecto y prepara su contraataque.");
                break;
            case QTEParryResult.Guard:
                this.messages.Enqueue($"{receiver.idName} levantó la guardia a tiempo, absorbiendo parte del impacto.");
                break;
            case QTEParryResult.Miss:
            default:
                this.messages.Enqueue($"{receiver.idName} no reaccionó a tiempo. {skillName} causó daño devastador.");
                break;
        }
    }
}