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
    [SerializeField] private Color limbDestroyColor = new Color(1f, 0.2f, 0.2f); // rojo intenso
    [SerializeField] private Color limbDestroyLabelColor = new Color(1f, 0.6f, 0f); // naranja
    [SerializeField] private float limbDestroyTextDuration = 2.5f;

    [Header("Stat Mod Floating Text")]
    [SerializeField] private Color buffColor   = new Color(0.4f, 0.9f, 1f);   // celeste
    [SerializeField] private Color debuffColor = new Color(1f,   0.5f, 0f);   // naranja
    [SerializeField] private float statTextStackSpacing = 0.5f;

    private float lastStatModTime;
    private int statModStackCount;

    [Header("Camera Feedback")]
    [SerializeField] private float normalShake = 0.6f;
    [SerializeField] private float criticalShake = 1f;
    [SerializeField] private float criticalHitStop = 0.15f;
    [SerializeField] private bool glitchPlayerOnDamage;

    [Header("Audio")]
    [SerializeField, Range(0f, 1f)] private float normalHitVolume = 0.7f;
    [SerializeField, Range(0f, 1f)] private float criticalHitVolume = 1f;

    [Header("Anchors")]
    [SerializeField] private FloatingTextAnchorSet anchors;

    private void OnEnable()
    {
        ResolveFighter();

        if (fighter != null)
        {
            fighter.OnDamageResolved  += HandleDamageResolved;
            fighter.OnStatModApplied  += HandleStatModApplied;
        }
    }

    private void OnDisable()
    {
        if (fighter != null)
        {
            fighter.OnDamageResolved  -= HandleDamageResolved;
            fighter.OnStatModApplied  -= HandleStatModApplied;
        }
    }

    private void ResolveFighter()
    {
        if (fighter == null)
            fighter = GetComponent<Fighter>();

        if (fighter == null)
            fighter = GetComponentInParent<Fighter>();

        if (anchors == null)
            anchors = GetComponentInChildren<FloatingTextAnchorSet>();
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

        if (result.destroyedBodyPart)
            color = limbDestroyColor;

        ShowFloatingText($"-{displayDamage}", color, result.isCritical, result);

        if (result.destroyedBodyPart)
            ShowLimbDestroyText(result);

        PlayHitAudio(result);
        PlayCameraFeedback(result);
    }

    private void ShowLimbDestroyText(DamageResult result)
    {
        if (FloatingTextManager.Instance == null)
            return;

        string partName = result.targetPart switch
        {
            BodyPart.Head      => "HEAD",
            BodyPart.LeftArm   => "L.ARM",
            BodyPart.RightArm  => "R.ARM",
            BodyPart.LeftLeg   => "L.LEG",
            BodyPart.RightLeg  => "R.LEG",
            BodyPart.Torso     => "TORSO",
            _                  => result.targetPart.ToString().ToUpper()
        };

        FloatingTextManager.Instance.ShowText(
            $"[{partName} DESTROYED]",
            GetLimbDestroyPosition(result),
            limbDestroyLabelColor,
            isCritical: false,
            randomizePosition: false,
            duration: limbDestroyTextDuration);
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

        FloatingTextAnchorSet targetAnchors = target != null
            ? target.GetComponentInChildren<FloatingTextAnchorSet>()
            : anchors;

        if (targetAnchors != null)
            return targetAnchors.GetDamagePosition(result.isCritical);

        // Legacy fallback for Fighters without an AnchorSet
        Transform pivot = (target != null ? target.transform : transform);
        return pivot.position + textOffset;
    }

    private Vector3 GetLimbDestroyPosition(DamageResult result)
    {
        Fighter target = result.receiver != null ? result.receiver : fighter;

        FloatingTextAnchorSet targetAnchors = target != null
            ? target.GetComponentInChildren<FloatingTextAnchorSet>()
            : anchors;

        if (targetAnchors != null)
            return targetAnchors.GetLimbDestroyPosition();

        Transform pivot = (target != null ? target.transform : transform);
        return pivot.position + textOffset;
    }

    private void HandleStatModApplied(StatModAppliedEvent e)
    {
        if (FloatingTextManager.Instance == null) return;

        bool isBuff = e.amount >= 0f;
        string sign  = isBuff ? "+" : "";   // float ya trae el '-' si es negativo
        Color  color = isBuff ? buffColor : debuffColor;

        string statLabel = e.modType switch
        {
            StatusModType.ATTACK_MOD  => "ATK",
            StatusModType.DEFFENSE_MOD => "DEF",
            StatusModType.SPEED_MOD   => "SPD",
            _                         => e.modType.ToString()
        };

        string message = $"{sign}{Mathf.RoundToInt(e.amount)} {statLabel}";

        // Lógica de apilamiento
        if (Time.time - lastStatModTime > 0.1f)
        {
            statModStackCount = 0;
        }
        lastStatModTime = Time.time;

        // Posición base sobre el personaje
        FloatingTextAnchorSet sourceAnchors = e.fighter != null
            ? e.fighter.GetComponentInChildren<FloatingTextAnchorSet>()
            : anchors;

        Vector3 basePosition = sourceAnchors != null
            ? sourceAnchors.GetStatModPosition()
            : (e.fighter != null ? e.fighter.transform.position : transform.position)
              + Vector3.up * 2f;
        
        // Posición final con apilamiento
        Vector3 finalPosition = basePosition + Vector3.up * (statModStackCount * statTextStackSpacing);
        statModStackCount++;

        FloatingTextManager.Instance.ShowText(message, finalPosition, color, isCritical: false, randomizePosition: false);
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