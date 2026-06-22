using System.Collections;
using UnityEngine;

/// <summary>
/// Lethal attack that gives the player a short real-time parry window at the impact moment.
/// The QTE manager only reports success or failure; this skill resolves combat consequences.
/// </summary>
public class LethalQTESkill : BodyPartTargetSkill
{
    public static event System.Action OnLethalSkillExecuted;
    public static event System.Action OnLethalSkillFinished;

    [Header("Parry QTE")]
    [SerializeField] private float parryWindowDuration = 0.3f;
    [SerializeField] private float slowMoTimeScale = 0.2f;
    [SerializeField] private float lethalDamage = -99999f;

    [Header("Damage")]
    [SerializeField] private DamageType damageType = DamageType.Kinetic;

    /// <summary>
    /// Waits until the animation impact frame, opens the parry QTE, then applies either
    /// a harmless parry result or instant lethal damage.
    /// </summary>
    /// <param name="receiver">Fighter receiving the lethal attack.</param>
    /// <param name="cachedBodyPartTarget">Body part target cached by the base skill execution.</param>
    protected override IEnumerator ApplyDamageDelayed(Fighter receiver, BodyPart cachedBodyPartTarget)
    {
        OnLethalSkillExecuted?.Invoke();

        if (impactDelay > 0f)
            yield return new WaitForSeconds(impactDelay);

        bool parried = false;
        QTEParryManager parryManager = QTEParryManager.Instance;

        CameraFXManager fxManager = FindObjectOfType<CameraFXManager>();

        if (parryManager != null)
        {
            if (fxManager != null) fxManager.SetLethalWarning(true);

            yield return parryManager.WaitForParry(parryWindowDuration, slowMoTimeScale, result => parried = result);
        }
        else
        {
            Debug.LogWarning("LethalQTESkill could not find a QTEParryManager in the scene.");
        }

        if (fxManager != null) fxManager.SetLethalWarning(false);

        DamageResult result = parried
            ? CreateParryResult(receiver, cachedBodyPartTarget)
            : CreateLethalResult(receiver, cachedBodyPartTarget);

        receiver.ModifyHealth(result);
        EnqueueOutcomeMessage(receiver, parried);

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
            new[] { $"{receiver.idName} desvía {skillName} con un parry perfecto." });
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
            messages = new[] { $"{receiver.idName} falla el parry y recibe un golpe letal." }
        };
    }

    private void EnqueueOutcomeMessage(Fighter receiver, bool parried)
    {
        if (parried)
            this.messages.Enqueue($"{receiver.idName} realiza un Parry y desvía el ataque letal.");
        else
            this.messages.Enqueue($"{receiver.idName} no reacciona a tiempo. {skillName} ejecuta un golpe letal.");
    }
}