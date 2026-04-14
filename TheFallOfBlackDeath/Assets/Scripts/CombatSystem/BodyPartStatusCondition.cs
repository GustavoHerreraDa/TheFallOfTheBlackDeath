using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Supports the combat system by handling body part status condition.
/// </summary>
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

    /// <summary>
    /// Initializes cached references and runtime state before the component starts running.
    /// </summary>
    public void Awake()
    {
        this.messages = new Queue<string>();
        this.initialTurnDuration = Mathf.Max(1, this.turnDuration);
    }

    /// <summary>
    /// Sets the context.
    /// </summary>
    /// <param name="recv">The recv.</param>
    /// <param name="part">The part.</param>
    public void SetContext(Fighter recv, BodyPart part)
    {
        this.receiver = recv;
        this.targetPart = part;
    }

    /// <summary>
    /// Executes the matches workflow.
    /// </summary>
    /// <param name="conditionType">The condition type.</param>
    /// <param name="part">The part.</param>
    /// <returns>True when the requested condition is met; otherwise, false.</returns>
    public bool Matches(System.Type conditionType, BodyPart part)
    {
        return this.GetType() == conditionType && this.targetPart == part;
    }

    /// <summary>
    /// Adds the stack.
    /// </summary>
    public void AddStack()
    {
        this.stacks++;
        this.turnDuration = Mathf.Max(this.turnDuration, this.initialTurnDuration);
    }

    /// <summary>
    /// Applies the value.
    /// </summary>
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

    /// <summary>
    /// Executes the animate workflow.
    /// </summary>
    private void Animate()
    {
        if (this.effectPrfb == null)
            return;

        Transform hitPoint = this.receiver.GetHitPoint(this.targetPart);
        var go = Instantiate(this.effectPrfb, hitPoint.position, hitPoint.rotation);
        Destroy(go, this.animationDuration);
    }

    /// <summary>
    /// Executes the format message workflow.
    /// </summary>
    /// <param name="template">The template.</param>
    /// <returns>The resulting value.</returns>
    protected string FormatMessage(string template)
    {
        return template
            .Replace("(receiver)", this.receiver.idName)
            .Replace("(part)", this.targetPart.ToString())
            .Replace("(stacks)", this.stacks.ToString());
    }

    /// <summary>
    /// Gets the reception message.
    /// </summary>
    /// <returns>The resulting value.</returns>
    public string GetReceptionMessage()
    {
        return this.FormatMessage(this.receptionMessage);
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

    public virtual bool BlocksTurn() => false;
    public abstract bool OnApply();
}
