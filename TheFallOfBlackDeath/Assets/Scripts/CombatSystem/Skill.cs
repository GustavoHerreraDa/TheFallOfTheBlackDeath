using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using InventoryNew;
//TP2 GUSTAVO TORRES/FACUNDO FERREIRO
/// <summary>
/// Defines the named values used by skill type.
/// </summary>
public enum SkillType
{
    AttackSimple,
    SpecialHability,
    Heal,
    BossHability,
    Melee,
    Range
}
/// <summary>
/// Defines the named values used by body part.
/// </summary>
public enum BodyPart
{
    None,
    Head,
    Torso,
    LeftLeg,
    LeftArm,
    RightArm,
    RightLeg
}
/// <summary>
/// Defines the named values used by skill rarity.
/// </summary>
public enum SkillRarity
{
    Common,
    Rare,
    Epic
}
/// <summary>
/// Defines the base behavior for combat skills, including targeting, messaging, animation, item requirements, and execution flow.
/// </summary>
public abstract class Skill : MonoBehaviour
{
    [Header("Base Skill")]
    public string skillName;

    [Header("Rarity")]
    public SkillRarity rarity;

    public float animationDuration;

    public SkillTargeting targeting;
    public BodyPart BodyPartTarget;

    public GameObject effectPrfb;

    protected Fighter emitter;
    protected List<Fighter> receivers;
    public Fighter MainTarget => (receivers != null && receivers.Count > 0) ? receivers[0] : null;
    [TextArea(3, 10)]
    public string SkillDesc;
    protected Queue<string> messages;
    public SkillType skillType;
    public Sprite iconUI;
    public string animationName;
    public bool HasItemInInventory;

    [System.Serializable]
    public class ItemRequirement
    {
        public string itemId;
        public int amount = 1;
    }

    [Header("Item Requirements (Nuevo Inventario)")]
    public List<ItemRequirement> ItemsNeeded = new List<ItemRequirement>();
    
    [Header("SFX - Habilidad")]
    [Tooltip("El sonido que hace al lanzarse (ej: disparo, grito, carga mágica)")]
    public AudioClip activationSound; 
    
    // (Opcional si quieres que cada ataque suene distinto al pegar)
    [Tooltip("El sonido que hace al impactar (ej: explosión de fuego, corte de espada)")]
    public AudioClip customImpactSound;

    [Header("Sanity Cost")]
    public float sanityCost = 0f;


    [Header("Body Requirements")]
    public List<BodyPart> requiredParts = new List<BodyPart>();
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

    /// <summary>
    /// Initializes cached references and runtime state before the component starts running.
    /// </summary>
    void Awake()
    {
        this.messages = new Queue<string>();
        this.receivers = new List<Fighter>();
    }

    /// <summary>
    /// Executes the animate workflow.
    /// </summary>
    /// <param name="receiver">The receiver.</param>
    private void Animate(Fighter receiver)
    {

        Transform hitPoint = receiver.GetHitPoint(this.BodyPartTarget);
        var go = Instantiate(this.effectPrfb, hitPoint.position, hitPoint.rotation);
        Destroy(go, this.animationDuration);
      
    }

    /// <summary>
    /// Executes the run workflow.
    /// </summary>
    public void Run(bool resolveBodyPartTargetOnRun = false)
    {
        if (!CanUseSkill(emitter))
        {
            this.messages.Enqueue($"{emitter.idName} intentó usar {skillName}, pero no puede xq no tiene esa parte del cuerpo.");
            Debug.Log($"{emitter.idName} no puede usar {skillName} por partes destruidas.");
            return;
        }
        
        if (AudioManager.Instance != null && this.activationSound != null)
        {
            AudioManager.Instance.PlaySFX(this.activationSound, 0.8f);
        }
        foreach (var receiver in this.receivers)
        {
            if (resolveBodyPartTargetOnRun && this is BodyPartTargetSkill)
            {
                this.BodyPartTarget = this.GetRandomTargetableBodyPart(receiver);

                if (this.BodyPartTarget == BodyPart.None)
                {
                    string receiverName = receiver != null ? receiver.idName : "Target";
                    this.messages.Enqueue($"{receiverName} has no targetable body parts.");
                    continue;
                }
            }

            this.Animate(receiver);
            this.OnRun(receiver);
        }

        this.receivers.Clear();
    }

    private BodyPart GetRandomTargetableBodyPart(Fighter receiver)
    {
        if (receiver == null || receiver.bodyParts == null || receiver.bodyParts.Count == 0)
            return BodyPart.None;

        List<BodyPart> targetableParts = new List<BodyPart>();
        foreach (var partData in receiver.bodyParts)
        {
            if (partData != null && partData.part != BodyPart.None && !partData.IsDestroyed)
                targetableParts.Add(partData.part);
        }

        if (targetableParts.Count == 0)
            return BodyPart.None;

        return targetableParts[Random.Range(0, targetableParts.Count)];
    }

    /// <summary>
    /// Sets the emitter.
    /// </summary>
    /// <param name="_emitter">The emitter.</param>
    public void SetEmitter(Fighter _emitter)
    {
        this.emitter = _emitter;
    }

    /// <summary>
    /// Adds the receiver.
    /// </summary>
    /// <param name="_receiver">The receiver.</param>
    public void AddReceiver(Fighter _receiver)
    {
        this.receivers.Add(_receiver);
        emitter.animator.Play(animationName);
    }

    /// <summary>
    /// Gets the next message.
    /// </summary>
    /// <returns>The resulting value.</returns>
    public string GetNextMessage()
    {
        if (this.messages.Count != 0)
            return this.messages.Dequeue();
        else
            return null;
    }

    /// <summary>
    /// Verifica si el jugador tiene los ítems requeridos usando el nuevo sistema de inventario.
    /// </summary>
    public bool HasRequiredItems()
    {
        // Si no hay requisitos, se considera que puede usarse.
        if (ItemsNeeded == null || ItemsNeeded.Count == 0)
        {
            HasItemInInventory = true;
            return true;
        }

        var inv = NewInventoryManager.Instance;
        if (inv == null)
        {
            // Si el inventario aún no está en escena, no bloqueamos el uso para no romper flujo en editor.
            HasItemInInventory = true;
            return true;
        }

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

    /// <summary>
    /// Determines whether the component can use skill.
    /// </summary>
    /// <param name="fighter">The fighter.</param>
    /// <returns>True when the requested condition is met; otherwise, false.</returns>
    protected bool CanUseSkill(Fighter fighter)
    {
        if (requiredParts == null || requiredParts.Count == 0)
            return true;

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
    /// <summary>
    /// Determines whether the component is usable.
    /// </summary>
    /// <param name="fighter">The fighter.</param>
    /// <returns>True when the requested condition is met; otherwise, false.</returns>
    public bool IsUsable(Fighter fighter)
    {
        // Primero, chequear requisitos de partes del cuerpo
        if (requiredParts != null && requiredParts.Count > 0)
        {
            foreach (var part in requiredParts)
            {
                var bodyPart = fighter.GetBodyPart(part);
                if (bodyPart == null || bodyPart.IsDestroyed)
                    return false;
            }
        }

        // Luego, chequear requisitos de inventario (nuevo sistema)
        return HasRequiredItems();
    }


    /// <summary>
    /// Determines whether the component can trigger synergy.
    /// </summary>
    /// <param name="target">The target.</param>
    /// <param name="part">The part.</param>
    /// <returns>True when the requested condition is met; otherwise, false.</returns>
    public virtual bool CanTriggerSynergy(Fighter target, BodyPart part = BodyPart.None)
    {
        return false;
    }

    protected abstract void OnRun(Fighter receiver);
}
