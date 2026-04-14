using UnityEngine;

/// <summary>
/// Supports the combat system by handling poison condition.
/// </summary>
public class PoisonCondition : BodyPartStatusCondition
{
    [Header("Poison Settings")]
    public float poisonDamage = 10f;

    /// <summary>
    /// Executes the on apply workflow.
    /// </summary>
    /// <returns>True when the requested condition is met; otherwise, false.</returns>
    public override bool OnApply()
    {
        if (receiver == null)
            return false;

        float totalDamage = poisonDamage * this.Stacks;
        receiver.ModifyBodyPartHealth(this.TargetPart, -totalDamage);

        messages.Enqueue($"{receiver.idName} sufre {(int)totalDamage} de dano por Veneno en {this.TargetPart}.");

        Vector3 textPos = receiver.GetHitPoint(this.TargetPart).position + Vector3.up * 0.5f;

        if (FloatingTextManager.Instance != null)
            FloatingTextManager.Instance.ShowText($"-{(int)totalDamage}", textPos, new Color(0.2f, 0.9f, 0.2f));

        if (CameraManager.Instance != null)
            CameraManager.Instance.TriggerShake(0.2f);

        if (AudioManager.Instance != null && AudioManager.Instance.uiHoverSound != null)
            AudioManager.Instance.PlaySFX(AudioManager.Instance.uiHoverSound, 0.6f);

        return true;
    }
}
