using UnityEngine;

// Sanity Overload skill: consumes sanity instantly before executing the standard HealthModSkill logic
public class OverloadSkill : HealthModSkill
{
    protected override void OnRun(Fighter receiver)
    {
        // Consume sanity if configured
        float cost = this.sanityCost;
        if (cost > 0f)
        {
            var gm = GameManager.Instance;
            if (gm != null && gm.sanity != null)
            {
                gm.sanity.DecreaseSanityInstantly(cost);
                if (this.emitter != null)
                {
                    this.messages.Enqueue($"{this.emitter.idName} consumes {Mathf.RoundToInt(cost)} Sanity using {this.skillName}.");
                }
                else
                {
                    this.messages.Enqueue($"Consumed {Mathf.RoundToInt(cost)} Sanity using {this.skillName}.");
                }
            }
        }

        // Delegate the rest to the base class to reuse all damage/miss/crit logic
        base.OnRun(receiver);
    }
}
