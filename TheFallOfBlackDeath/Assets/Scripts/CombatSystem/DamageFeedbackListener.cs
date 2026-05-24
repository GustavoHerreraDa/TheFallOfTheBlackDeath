using UnityEngine;

/// <summary>
/// Bridges Fighter damage events with presentation systems. Keep this on the
/// receiving fighter or assign a fighter explicitly in the Inspector.
/// </summary>
public class DamageFeedbackListener : MonoBehaviour
{
    [SerializeField] private Fighter fighter;

    [Header("Floating Text")]
    [SerializeField] private Vector3 textOffset = Vector3.up * 0.5f;
    [SerializeField] private Color normalDamageColor = Color.red;
    [SerializeField] private Color criticalDamageColor = Color.yellow;
    [SerializeField] private Color missColor = Color.gray;
    [SerializeField] private Color healColor = Color.green;

    [Header("Camera Feedback")]
    [SerializeField] private float normalShake = 0.6f;
    [SerializeField] private float criticalShake = 1f;
    [SerializeField] private float criticalHitStop = 0.15f;
    [SerializeField] private bool glitchPlayerOnDamage;

    [Header("Audio")]
    [SerializeField, Range(0f, 1f)] private float normalHitVolume = 0.7f;
    [SerializeField, Range(0f, 1f)] private float criticalHitVolume = 1f;

    private void OnEnable()
    {
        ResolveFighter();

        if (fighter != null)
            fighter.OnDamageResolved += HandleDamageResolved;
    }

    private void OnDisable()
    {
        if (fighter != null)
            fighter.OnDamageResolved -= HandleDamageResolved;
    }

    private void ResolveFighter()
    {
        if (fighter == null)
            fighter = GetComponent<Fighter>();

        if (fighter == null)
            fighter = GetComponentInParent<Fighter>();
    }

    private void HandleDamageResolved(DamageResult result)
    {
        if (result.isMiss)
        {
            ShowFloatingText("Miss!", missColor, false, result);
            return;
        }

        if (result.appliedAmount < 0f)
        {
            ShowDamageFeedback(result);
            return;
        }

        if (result.appliedAmount > 0f)
            ShowFloatingText($"+{Mathf.RoundToInt(result.appliedAmount)}", healColor, false, result);
    }

    private void ShowDamageFeedback(DamageResult result)
    {
        int displayDamage = Mathf.Abs(Mathf.RoundToInt(result.appliedAmount));
        Color color = result.isCritical ? criticalDamageColor : normalDamageColor;

        ShowFloatingText($"-{displayDamage}", color, result.isCritical, result);
        PlayHitAudio(result);
        PlayCameraFeedback(result);
    }

    private void ShowFloatingText(string message, Color color, bool isCritical, DamageResult result)
    {
        if (FloatingTextManager.Instance == null)
            return;

        FloatingTextManager.Instance.ShowText(message, GetTextPosition(result), color, isCritical);
    }

    private Vector3 GetTextPosition(DamageResult result)
    {
        Fighter target = result.receiver != null ? result.receiver : fighter;
        if (target == null)
            return transform.position + textOffset;

        Transform anchor = result.targetPart != BodyPart.None
            ? target.GetHitPoint(result.targetPart)
            : target.DamagePivot;

        if (anchor == null)
            anchor = target.transform;

        return anchor.position + textOffset;
    }

    private void PlayHitAudio(DamageResult result)
    {
        if (AudioManager.Instance == null)
            return;

        if (result.isCritical)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.hitCriticalSound, criticalHitVolume);
            return;
        }

        AudioClip hitSound = result.sourceSkill != null && result.sourceSkill.customImpactSound != null
            ? result.sourceSkill.customImpactSound
            : AudioManager.Instance.hitNormalSound;

        AudioManager.Instance.PlaySFX(hitSound, normalHitVolume);
    }

    private void PlayCameraFeedback(DamageResult result)
    {
        if (CameraManager.Instance == null)
            return;

        if (result.isCritical)
        {
            CameraManager.Instance.TriggerShake(criticalShake);
            CameraManager.Instance.TriggerHitStop(criticalHitStop);
        }
        else
        {
            CameraManager.Instance.TriggerShake(normalShake);
        }

        if (glitchPlayerOnDamage && fighter is PlayerFighter)
            CameraManager.Instance.TriggerDamageGlitch();
    }
}
