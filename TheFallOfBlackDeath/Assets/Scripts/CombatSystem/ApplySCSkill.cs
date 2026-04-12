using UnityEngine;
//TP2 FACUNDO FERREIRO
/// <summary>
/// Apply Body Part Status Condition Skill
/// </summary>
public class ApplySCSkill : BodyPartTargetSkill
{
    [Header("Initial Body Part Damage")]
    public float damageAmount = 0f;
    public Color initialDamageColor = new Color(0.6f, 0.2f, 0.8f);
    public float initialHitShake = 0.4f;

    private BodyPartStatusCondition condition;

    protected override void OnRun(Fighter receiver)
    {
        if (this.condition == null)
        {
            this.condition = this.GetComponentInChildren<BodyPartStatusCondition>();

            if (this.condition == null)
            {
                throw new System.InvalidOperationException(
                    $"{name} needs a child object with a BodyPartStatusCondition component."
                );
            }

            if (this.condition.gameObject == this.gameObject)
            {
                throw new System.InvalidOperationException(
                    "The BodyPartStatusCondition should be a child of the skill object because it needs to be cloned"
                );
            }
        }

        if (!this.TryGetTargetBodyPart(receiver, out Fighter.BodyPartData targetPartData))
        {
            return;
        }

        if (damageAmount > 0f)
        {
            receiver.ModifyBodyPartHealth(this.BodyPartTarget, -damageAmount);
            this.messages.Enqueue($"Hit {receiver.idName}'s {this.BodyPartTarget} for {(int)damageAmount}");

            Vector3 textPos = this.GetBodyPartTextPosition(receiver, this.BodyPartTarget);
            if (FloatingTextManager.Instance != null)
                FloatingTextManager.Instance.ShowText($"-{(int)damageAmount}", textPos, initialDamageColor);

            if (CameraManager.Instance != null && initialHitShake > 0f)
                CameraManager.Instance.TriggerShake(initialHitShake);

            AudioClip hitSound = this.customImpactSound != null ? this.customImpactSound : AudioManager.Instance != null ? AudioManager.Instance.hitNormalSound : null;
            if (AudioManager.Instance != null && hitSound != null)
                AudioManager.Instance.PlaySFX(hitSound, 0.8f);
        }

        BodyPartStatusCondition existingCondition = receiver.GetCurrentBodyPartStatusCondition(this.condition.GetType(), this.BodyPartTarget);
        if (existingCondition != null)
        {
            existingCondition.AddStack();
            this.messages.Enqueue(existingCondition.GetReceptionMessage());
            return;
        }

        GameObject go = Instantiate(this.condition.gameObject);
        go.transform.SetParent(receiver.transform);

        BodyPartStatusCondition clonedCondition = go.GetComponent<BodyPartStatusCondition>();
        clonedCondition.SetContext(receiver, this.BodyPartTarget);
        receiver.AddBodyPartStatusCondition(clonedCondition);

        this.messages.Enqueue(clonedCondition.GetReceptionMessage());
    }
}
