using System.Collections;
using UnityEngine;

/// <summary>
/// Queues a counterattack when this fighter receives direct damage from another fighter.
/// </summary>
public class CounterAttackReaction : ReactionComponent
{
    [Header("Counter Attack")]
    [SerializeField] private Skill counterAttackSkill;
    [SerializeField] private BodyPart requiredBodyPart = BodyPart.RightArm;
    [SerializeField, Range(0f, 1f)] private float triggerChance = 1f;
    [SerializeField] private float reactionStartDelay = 0.1f;
    [SerializeField] private bool requireAttackerAlive = true;

    private bool reactionQueued;

    protected override bool CanReact(DamageReceivedEventData damageEvent)
    {
        if (reactionQueued || !base.CanReact(damageEvent))
            return false;

        if (damageEvent.attacker == null || damageEvent.attacker == owner)
            return false;

        if (requireAttackerAlive && !damageEvent.attacker.isAlive)
            return false;

        if (!HasRequiredBodyPart())
            return false;

        if (counterAttackSkill == null || !counterAttackSkill.IsUsable(owner))
            return false;

        if (Random.value > triggerChance)
            return false;

        reactionQueued = true;
        return true;
    }

    protected override void OnReactionQueueRejected(DamageReceivedEventData damageEvent)
    {
        reactionQueued = false;
    }

    protected override IEnumerator ExecuteReaction(DamageReceivedEventData damageEvent)
    {
        if (reactionStartDelay > 0f)
            yield return new WaitForSeconds(reactionStartDelay);

        if (!CanStillCounter(damageEvent))
        {
            reactionQueued = false;
            yield break;
        }

        LogPanel.Write($"{owner.idName} counterattacks.");

        counterAttackSkill.SetEmitter(owner);
        counterAttackSkill.AddReceiver(damageEvent.attacker);
        counterAttackSkill.Run(resolveBodyPartTargetOnRun: true);

        while (true)
        {
            string nextMessage = counterAttackSkill.GetNextMessage();
            if (nextMessage == null) break;

            LogPanel.Write(nextMessage);
        }

        if (counterAttackSkill.actionDelay > 0f)
            yield return new WaitForSeconds(counterAttackSkill.actionDelay);

        reactionQueued = false;
    }

    private bool CanStillCounter(DamageReceivedEventData damageEvent)
    {
        if (owner == null || !owner.isAlive)
            return false;

        if (damageEvent.attacker == null)
            return false;

        if (requireAttackerAlive && !damageEvent.attacker.isAlive)
            return false;

        if (!HasRequiredBodyPart())
            return false;

        return counterAttackSkill != null && counterAttackSkill.IsUsable(owner);
    }

    private bool HasRequiredBodyPart()
    {
        if (requiredBodyPart == BodyPart.None)
            return true;

        Fighter.BodyPartData bodyPart = owner != null ? owner.GetBodyPart(requiredBodyPart) : null;
        return bodyPart != null && !bodyPart.IsDestroyed;
    }
}
