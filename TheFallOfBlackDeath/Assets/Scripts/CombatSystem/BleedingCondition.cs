using UnityEngine;

public class BleedingCondition : BodyPartStatusCondition
{
    [Header("Bleeding Settings")]
    public float damagePerTurn = 15f;

    public override bool OnApply()
    {
        if (receiver == null)
            return false;

        float totalDamage = damagePerTurn * this.Stacks;
        receiver.ModifyBodyPartHealth(this.TargetPart, -totalDamage);

        messages.Enqueue($"{receiver.idName} sufre {(int)totalDamage} de dano por Sangrado en {this.TargetPart}.");

        Vector3 textPos = receiver.GetHitPoint(this.TargetPart).position + Vector3.up * 0.5f;

        if (FloatingTextManager.Instance != null)
            FloatingTextManager.Instance.ShowText($"-{(int)totalDamage}", textPos, new Color(0.7f, 0.1f, 0.1f));

        if (CameraManager.Instance != null)
            CameraManager.Instance.TriggerShake(0.3f);

        if (AudioManager.Instance != null && AudioManager.Instance.hitNormalSound != null)
            AudioManager.Instance.PlaySFX(AudioManager.Instance.hitNormalSound, 0.5f);

        return true;
    }
}
