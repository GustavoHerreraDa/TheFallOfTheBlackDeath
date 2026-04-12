using System.Collections.Generic;
using UnityEngine;

public abstract class BodyPartStatusCondition : MonoBehaviour
{
    [Header("Base Body Part Status Condition")]
    public GameObject effectPrfb;
    public float animationDuration;

    public string receptionMessage;
    public string applyMessage;
    public string expireMessage;

    public int turnDuration = 1;

    protected Queue<string> messages;
    protected Fighter receiver;
    protected BodyPart targetPart;
    protected int stacks = 1;
    private int initialTurnDuration;

    public bool hasExpired => this.turnDuration <= 0;
    public Fighter Receiver => this.receiver;
    public BodyPart TargetPart => this.targetPart;
    public int Stacks => this.stacks;

    public void Awake()
    {
        this.messages = new Queue<string>();
        this.initialTurnDuration = Mathf.Max(1, this.turnDuration);
    }

    public void SetContext(Fighter recv, BodyPart part)
    {
        this.receiver = recv;
        this.targetPart = part;
    }

    public bool Matches(System.Type conditionType, BodyPart part)
    {
        return this.GetType() == conditionType && this.targetPart == part;
    }

    public void AddStack()
    {
        this.stacks++;
        this.turnDuration = Mathf.Max(this.turnDuration, this.initialTurnDuration);
    }

    public void Apply()
    {
        if (this.receiver == null)
            throw new System.InvalidOperationException("BodyPartStatusCondition needs a receiver");

        if (this.receiver.GetBodyPart(this.targetPart) == null)
            throw new System.InvalidOperationException("BodyPartStatusCondition needs a valid target part");

        if (this.OnApply())
            this.Animate();

        this.turnDuration--;

        if (this.hasExpired && !string.IsNullOrEmpty(this.expireMessage))
            this.messages.Enqueue(this.FormatMessage(this.expireMessage));
    }

    private void Animate()
    {
        if (this.effectPrfb == null)
            return;

        Transform hitPoint = this.receiver.GetHitPoint(this.targetPart);
        var go = Instantiate(this.effectPrfb, hitPoint.position, hitPoint.rotation);
        Destroy(go, this.animationDuration);
    }

    protected string FormatMessage(string template)
    {
        return template
            .Replace("(receiver)", this.receiver.idName)
            .Replace("(part)", this.targetPart.ToString())
            .Replace("(stacks)", this.stacks.ToString());
    }

    public string GetReceptionMessage()
    {
        return this.FormatMessage(this.receptionMessage);
    }

    public string GetNextMessage()
    {
        if (this.messages.Count != 0)
            return this.messages.Dequeue();
        else
            return null;
    }

    public virtual bool BlocksTurn() => false;
    public abstract bool OnApply();
}
