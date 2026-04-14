using UnityEngine;

/// <summary>
/// Supports the combat system by handling body part target skill.
/// </summary>
public abstract class BodyPartTargetSkill : Skill
{
    /// <summary>
    /// Attempts to get the target body part.
    /// </summary>
    /// <param name="receiver">The receiver.</param>
    /// <param name="targetPart">The target part.</param>
    /// <returns>True when the requested condition is met; otherwise, false.</returns>
    protected bool TryGetTargetBodyPart(Fighter receiver, out Fighter.BodyPartData targetPart)
    {
        targetPart = null;

        if (this.BodyPartTarget == BodyPart.None)
        {
            this.messages.Enqueue("This skill requires a body part target!");
            return false;
        }

        targetPart = receiver.GetBodyPart(this.BodyPartTarget);
        if (targetPart == null || targetPart.IsDestroyed)
        {
            this.messages.Enqueue($"{receiver.idName}'s {this.BodyPartTarget} cannot be targeted.");
            return false;
        }

        return true;
    }

    /// <summary>
    /// Gets the body part text position.
    /// </summary>
    /// <param name="receiver">The receiver.</param>
    /// <param name="part">The part.</param>
    /// <returns>The resulting value.</returns>
    protected Vector3 GetBodyPartTextPosition(Fighter receiver, BodyPart part)
    {
        Transform hitPoint = receiver.GetHitPoint(part);
        return hitPoint.position + Vector3.up * 0.5f;
    }
}
