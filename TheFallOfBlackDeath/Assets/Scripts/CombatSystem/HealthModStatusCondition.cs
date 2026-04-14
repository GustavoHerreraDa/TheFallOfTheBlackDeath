
using UnityEngine;
//TP2 FACUNDO FERREIRO
/// <summary>
/// Supports the combat system by handling health mod status condition.
/// </summary>
public class HealthModStatusCondition : StatusCondition
{
    [Header("Health mod")]
    public float percentage;

    /// <summary>
    /// Executes the on apply workflow.
    /// </summary>
    /// <returns>True when the requested condition is met; otherwise, false.</returns>
    public override bool OnApply()
    {
        Stats rStats = receiver.GetCurrentStats();

        this.receiver.ModifyHealth(rStats.maxHealth * this.percentage);

        this.messages.Enqueue(this.applyMessage.Replace("(receiver)", this.receiver.idName));

        return true;
    }

    public override bool BlocksTurn() => false;
}
