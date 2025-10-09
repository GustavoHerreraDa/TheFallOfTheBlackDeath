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
    Legs,
    Arms
}
public abstract class Skill : MonoBehaviour
{
    [Header("Base Skill")]
    public string skillName;
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
        var go = Instantiate(this.effectPrfb, receiver.DamagePivot.position, Quaternion.identity);
        Destroy(go, this.animationDuration);
    }

    public void Run()
    {
        if (!CanUseSkill(emitter))
        {
            this.messages.Enqueue($"{emitter.idName} intentó usar {skillName}, pero no puede por daño corporal.");
            Debug.LogWarning($"{emitter.idName} no puede usar {skillName} por partes destruidas.");
            return;
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
            return true; // no necesita partes específicas

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


    protected abstract void OnRun(Fighter receiver);
}