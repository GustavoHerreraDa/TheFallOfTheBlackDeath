using System.Collections;
using UnityEngine;

/// <summary>
/// Base class for combat reactions that listen to Fighter damage events and enqueue
/// their execution in the CombatManager reaction queue.
/// </summary>
public abstract class ReactionComponent : MonoBehaviour
{
    [SerializeField] protected Fighter owner;
    [SerializeField] protected bool canReactToReactions;

    protected virtual void Awake()
    {
        ResolveOwner();
    }

    protected virtual void OnEnable()
    {
        ResolveOwner();

        if (owner != null)
            owner.OnDamageReceived += HandleDamageReceived;
    }

    protected virtual void OnDisable()
    {
        if (owner != null)
            owner.OnDamageReceived -= HandleDamageReceived;
    }

    private void ResolveOwner()
    {
        if (owner == null)
            owner = GetComponentInParent<Fighter>();
    }

    private void HandleDamageReceived(DamageReceivedEventData damageEvent)
    {
        if (!CanReact(damageEvent))
            return;

        CombatManager combatManager = owner != null ? owner.combatManager : null;
        if (combatManager == null)
        {
            OnReactionQueueRejected(damageEvent);
            return;
        }

        combatManager.EnqueueReaction(ExecuteReaction(damageEvent));
    }

    protected virtual bool CanReact(DamageReceivedEventData damageEvent)
    {
        if (owner == null || !owner.isAlive || !damageEvent.IsDamage)
            return false;

        if (owner.combatManager != null && owner.combatManager.IsProcessingReaction && !canReactToReactions)
            return false;

        return true;
    }

    protected virtual void OnReactionQueueRejected(DamageReceivedEventData damageEvent)
    {
    }

    protected abstract IEnumerator ExecuteReaction(DamageReceivedEventData damageEvent);
}
