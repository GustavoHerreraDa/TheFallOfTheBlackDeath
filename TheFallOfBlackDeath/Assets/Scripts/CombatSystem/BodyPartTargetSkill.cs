using UnityEngine;

public abstract class BodyPartTargetSkill : Skill
{
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

    protected Vector3 GetBodyPartTextPosition(Fighter receiver, BodyPart part)
    {
        Transform hitPoint = receiver.GetHitPoint(part);
        return hitPoint.position + Vector3.up * 0.5f;
    }
}
