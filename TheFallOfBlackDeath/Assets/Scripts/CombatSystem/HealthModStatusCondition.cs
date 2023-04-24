using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthModStatusCondition : StatusCondition
{
    [Header("Health mod")]
    public float percentage;

    public override void OnApply()
    {
        Stats rStats = receiver.GetCurrentStats();

        this.receiver.ModifyHealth(rStats.maxHealth * this.percentage);

        this.messages.Enqueue(this.applyMessage.Replace("(receiver)", this.receiver.idName));
    }
    public override bool BlocksTurn() => false;
}
