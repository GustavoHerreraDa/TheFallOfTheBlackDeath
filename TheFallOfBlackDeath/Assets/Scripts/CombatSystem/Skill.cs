using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using InventoryNew;
//TP2 GUSTAVO TORRES/FACUNDO FERREIRO

// ── Enums (unchanged) ─────────────────────────────────────────────────────────

public enum SkillType      { AttackSimple, SpecialHability, Heal, BossHability, Melee, Range }
public enum BodyPart       { None, Head, Torso, LeftLeg, LeftArm, RightArm, RightLeg }
public enum SkillRarity    { Common, Rare, Epic }

/// <summary>
/// Defines the base behavior for combat skills, including targeting, messaging,
/// animation, item requirements, and execution flow.
///
/// CHANGE LOG (VFX refactor):
///   • Removed the old <c>effectPrfb</c> + <c>animationDuration</c> fields that
///     hard-coded a single "spawn at hit-point and destroy" pattern.
///   • Added <see cref="effectConfig"/> (<see cref="SkillEffectConfig"/>), a
///     ScriptableObject that declaratively describes the full visual behaviour
///     (melee splash, ranged projectile, emitter burst, or any combination).
///   • <see cref="effectPlayer"/> is auto-resolved at runtime via
///     <see cref="GetOrCreateEffectPlayer"/>; no manual wiring required.
///   • All per-receiver VFX is now triggered through
///     <see cref="SkillEffectPlayer.Play"/> inside the private <c>Animate</c> method.
///   • Every other existing public method, event, and flow is untouched.
/// </summary>
public abstract class Skill : MonoBehaviour
{
    [Header("Base Skill")]
    public string skillName;
    public string skillId; // NUEVO: ID estable opcional; si queda vacio se usa skillName como fallback.

    [Header("Rarity")]
    public SkillRarity rarity;

    // ── VFX (new system) ───────────────────────────────────────────────────────
    [Header("Visual Effect")]
    [Tooltip("ScriptableObject that describes the full visual behaviour of this skill. " +
             "Assign one of the assets under Assets/Combat/SkillEffects/.")]
    public SkillEffectConfig effectConfig;

    [Tooltip("Delay before the next action in the combat loop. Replaces the old animationDuration.")]
    public float actionDelay = 1.0f;

    [Tooltip("Tiempo en segundos antes de aplicar el impacto numérico (daño/curación/estado) tras iniciar el VFX.")]
    public float impactDelay = 0.0f;

    // ── Targeting & body ──────────────────────────────────────────────────────
    public SkillTargeting targeting;
    public BodyPart BodyPartTarget;

    // ── Descriptions & UI ─────────────────────────────────────────────────────
    [TextArea(3, 10)]
    public string SkillDesc;
    public SkillType skillType;
    public Sprite iconUI;
    public string animationName;
    public bool HasItemInInventory;

    // ── Item requirements ─────────────────────────────────────────────────────
    [System.Serializable]
    public class ItemRequirement
    {
        public string itemId;
        public int amount = 1;
    }

    [Header("Item Requirements")]
    public List<ItemRequirement> ItemsNeeded = new List<ItemRequirement>();

    // ── Audio ─────────────────────────────────────────────────────────────────
    [Header("SFX")]
    [Tooltip("Sound played when the skill is activated (shot, shout, magic charge…)")]
    public AudioClip activationSound;

    [Tooltip("Sound played on impact (explosion, sword slash…)")]
    public AudioClip customImpactSound;

    // ── Stats ─────────────────────────────────────────────────────────────────
    [Header("Sanity Cost")]
    public float sanityCost = 0f;

    // ── Body requirements ─────────────────────────────────────────────────────
    [Header("Body Requirements")]
    public List<BodyPart> requiredParts = new List<BodyPart>();

    // ── Runtime state (protected so subclasses can enqueue messages) ──────────
    protected Fighter emitter;
    protected List<Fighter> receivers;
    protected Queue<string> messages;

    // ── Lazy effect player ────────────────────────────────────────────────────
    private SkillEffectPlayer _effectPlayer;

    // ── Convenience properties ────────────────────────────────────────────────
    public Fighter MainTarget => (receivers != null && receivers.Count > 0) ? receivers[0] : null;

    public bool needsManualTargeting
    {
        get
        {
            switch (this.targeting)
            {
                case SkillTargeting.SINGLE_ALLY:
                case SkillTargeting.SINGLE_OPPONENT:
                    return true;
                default:
                    return false;
            }
        }
    }

    // ──────────────────────────────────────────────────────────────────────────
    //  Unity lifecycle
    // ──────────────────────────────────────────────────────────────────────────

    void Awake()
    {
        this.messages  = new Queue<string>();
        this.receivers = new List<Fighter>();
    }

    // ──────────────────────────────────────────────────────────────────────────
    //  VFX
    // ──────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Plays the visual effect for a single receiver.
    /// Delegates entirely to <see cref="SkillEffectPlayer"/> + <see cref="SkillEffectConfig"/>
    /// so this method stays lean and every skill stays decoupled from VFX logic.
    /// </summary>
    private void Animate(Fighter receiver)
    {
        if (effectConfig == null) return;
        GetOrCreateEffectPlayer().Play(effectConfig, emitter, receiver, this.BodyPartTarget);
    }

    /// <summary>
    /// Returns the scene-level <see cref="SkillEffectPlayer"/>, creating one on demand.
    /// We attach it to the emitter's GameObject so it has a proper MonoBehaviour context
    /// for coroutines and is cleaned up when the fighter is destroyed.
    /// </summary>
    private SkillEffectPlayer GetOrCreateEffectPlayer()
    {
        if (_effectPlayer != null) return _effectPlayer;

        // Try to reuse one already on the emitter
        if (emitter != null)
        {
            _effectPlayer = emitter.GetComponent<SkillEffectPlayer>();
            if (_effectPlayer == null)
                _effectPlayer = emitter.gameObject.AddComponent<SkillEffectPlayer>();
            return _effectPlayer;
        }

        // Fallback: attach to this skill's GameObject
        _effectPlayer = GetComponent<SkillEffectPlayer>();
        if (_effectPlayer == null)
            _effectPlayer = gameObject.AddComponent<SkillEffectPlayer>();

        return _effectPlayer;
    }

    // ──────────────────────────────────────────────────────────────────────────
    //  Execution
    // ──────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Runs the skill: validates body-part requirements, plays SFX, then for each
    /// receiver resolves the body-part target, spawns VFX, and calls <see cref="OnRun"/>.
    /// </summary>
    public void Run(bool resolveBodyPartTargetOnRun = false)
    {
        if (!CanUseSkill(emitter))
        {
            this.messages.Enqueue($"{emitter.idName} intentó usar {skillName}, pero no puede porque no tiene esa parte del cuerpo.");
            Debug.Log($"{emitter.idName} no puede usar {skillName} por partes destruidas.");
            return;
        }

        if (AudioManager.Instance != null && this.activationSound != null)
            AudioManager.Instance.PlaySFX(this.activationSound, 0.8f);

        foreach (var receiver in this.receivers)
        {
            // Cache local por objetivo para evitar sobrescrituras entre múltiples targets.
            BodyPart bodyPartTargetForReceiver = this.BodyPartTarget;

            if (resolveBodyPartTargetOnRun && this is BodyPartTargetSkill)
            {
                bodyPartTargetForReceiver = GetRandomTargetableBodyPart(receiver);

                if (bodyPartTargetForReceiver == BodyPart.None)
                {
                    string receiverName = receiver != null ? receiver.idName : "Target";
                    this.messages.Enqueue($"{receiverName} has no targetable body parts.");
                    continue;
                }
            }

            // 1) El VFX se dispara inmediatamente al iniciar la habilidad.
            this.Animate(receiver);

            // 2) La lógica numérica se aplica sincronizada con el momento de impacto.
            StartCoroutine(ApplyDamageDelayed(receiver, bodyPartTargetForReceiver));
        }

        this.receivers.Clear();
    }

    /// <summary>
    /// Espera <see cref="impactDelay"/> para sincronizar el impacto real con la animación.
    /// Mantiene el BodyPartTarget cacheado por objetivo para evitar condiciones de sobrescritura.
    /// </summary>
    private IEnumerator ApplyDamageDelayed(Fighter receiver, BodyPart cachedBodyPartTarget)
    {
        if (impactDelay > 0f)
            yield return new WaitForSeconds(impactDelay);

        BodyPart previousBodyPartTarget = this.BodyPartTarget;
        this.BodyPartTarget = cachedBodyPartTarget;

        this.OnRun(receiver);

        this.BodyPartTarget = previousBodyPartTarget;
    }

    // ──────────────────────────────────────────────────────────────────────────
    //  Helpers
    // ──────────────────────────────────────────────────────────────────────────

    private BodyPart GetRandomTargetableBodyPart(Fighter receiver)
    {
        if (receiver == null || receiver.bodyParts == null || receiver.bodyParts.Count == 0)
            return BodyPart.None;

        var targetableParts = new List<BodyPart>();
        foreach (var partData in receiver.bodyParts)
        {
            if (partData != null && partData.part != BodyPart.None && !partData.IsDestroyed)
                targetableParts.Add(partData.part);
        }

        return targetableParts.Count == 0
            ? BodyPart.None
            : targetableParts[Random.Range(0, targetableParts.Count)];
    }

    // ──────────────────────────────────────────────────────────────────────────
    //  Public API (unchanged signatures)
    // ──────────────────────────────────────────────────────────────────────────

    public void SetEmitter(Fighter _emitter)
    {
        this.emitter = _emitter;
    }

    public void AddReceiver(Fighter _receiver)
    {
        this.receivers.Add(_receiver);
        emitter.animator.Play(animationName);
    }

    public string GetNextMessage()
    {
        return this.messages.Count != 0 ? this.messages.Dequeue() : null;
    }

    public bool HasRequiredItems()
    {
        if (ItemsNeeded == null || ItemsNeeded.Count == 0) { HasItemInInventory = true; return true; }

        var inv = NewInventoryManager.Instance;
        if (inv == null) { HasItemInInventory = true; return true; }

        foreach (var req in ItemsNeeded)
        {
            if (req == null || string.IsNullOrEmpty(req.itemId)) continue;
            int have = inv.GetItemCount(req.itemId);
            if (have < (req.amount <= 0 ? 1 : req.amount))
            {
                HasItemInInventory = false;
                return false;
            }
        }

        HasItemInInventory = true;
        return true;
    }

    protected bool CanUseSkill(Fighter fighter)
    {
        if (requiredParts == null || requiredParts.Count == 0) return true;

        foreach (var part in requiredParts)
        {
            var bodyPart = fighter.GetBodyPart(part);
            if (bodyPart == null || bodyPart.IsDestroyed)
            {
                Debug.Log($"{fighter.idName} no puede usar {skillName}: {part} destruido");
                return false;
            }
        }
        return true;
    }

    public bool IsUsable(Fighter fighter)
    {
        if (requiredParts != null && requiredParts.Count > 0)
        {
            foreach (var part in requiredParts)
            {
                var bodyPart = fighter.GetBodyPart(part);
                if (bodyPart == null || bodyPart.IsDestroyed) return false;
            }
        }
        return HasRequiredItems();
    }

    public virtual bool CanTriggerSynergy(Fighter target, BodyPart part = BodyPart.None)
        => false;

    // ──────────────────────────────────────────────────────────────────────────
    //  Abstract contract
    // ──────────────────────────────────────────────────────────────────────────

    protected abstract void OnRun(Fighter receiver);
}
