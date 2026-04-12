using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
//TP2 GUSTAVO TORRES/FACUNDO FERREIRO
public enum SkillType
{
    AttackSimple,
    SpecialHability,
    Heal,
    BossHability
}
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
public enum SkillRarity
{
    Common,
    Rare,
    Epic
}
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
    public string SkillDesc;
    protected Queue<string> messages;
    public SkillType skillType;
    public Sprite iconUI;
    public string animationName;
    public bool HasItemInInventory;
    public List<InventoryManager.InventoryObjectID> ItemsNeeded;
    
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

    void Awake()
    {
        this.messages = new Queue<string>();
        this.receivers = new List<Fighter>();
    }

    private void Animate(Fighter receiver)
    {

        Transform hitPoint = receiver.GetHitPoint(this.BodyPartTarget);
        var go = Instantiate(this.effectPrfb, hitPoint.position, hitPoint.rotation);
        Destroy(go, this.animationDuration);
      
    }

    public void Run()
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
            this.Animate(receiver);
            this.OnRun(receiver);
        }

        this.receivers.Clear();
    }

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
        if (this.messages.Count != 0)
            return this.messages.Dequeue();
        else
            return null;
    }

    public void HasItemsInInventory()
    {
        var hasItems = InventoryManager.instance == null ? true : InventoryManager.instance.HasItemInIventory(ItemsNeeded);
        HasItemInInventory = hasItems;
    }

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
    public bool IsUsable(Fighter fighter)
    {
        if (requiredParts == null || requiredParts.Count == 0)
            return true;

        foreach (var part in requiredParts)
        {
            var bodyPart = fighter.GetBodyPart(part);
            if (bodyPart == null || bodyPart.IsDestroyed)
                return false;
        }

        return true;
    }


    public virtual bool CanTriggerSynergy(Fighter target, BodyPart part = BodyPart.None)
    {
        return false;
    }

    protected abstract void OnRun(Fighter receiver);
}