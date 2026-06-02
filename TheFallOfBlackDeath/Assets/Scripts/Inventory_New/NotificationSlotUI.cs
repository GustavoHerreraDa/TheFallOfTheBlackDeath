using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// A single toast notification row.
/// Attach to the NotificationSlot prefab.
/// Animates in from the side and fades out.
///
/// PREFAB HIERARCHY:
///   NotificationSlot (RectTransform — this component goes here)
///   ├── Background    (Image)              ← tinted by notification type
///   ├── TypeBadge     (Image)              ← small icon: pickup vs loot
///   ├── Icon          (Image)              ← item sprite
///   ├── TextGroup     (RectTransform)
///   │   ├── TitleText (TMP_Text)           ← "Recogiste" / "Botín"
///   │   └── NameText  (TMP_Text)           ← item name + amount
///   └── Glow          (Image, optional)    ← rim glow overlay
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class NotificationSlotUI : MonoBehaviour
{
    [Header("UI References")]
    public Image         background;
    public Image         typeBadge;
    public Image         itemIcon;
    public TMP_Text      titleText;
    public TMP_Text      nameText;

    [Header("Type Styling")]
    public Color pickupColor  = new Color(0.13f, 0.53f, 0.40f, 0.92f);  // teal-green
    public Color lootColor    = new Color(0.55f, 0.25f, 0.75f, 0.92f);  // deep purple
    public Sprite pickupBadgeSprite;
    public Sprite lootBadgeSprite;

    [Header("Animation")]
    [Tooltip("How far off-screen the toast slides in from (pixels).")]
    public float slideOffset = 80f;

    // ─── Internal ─────────────────────────────────────────────────────────────

    private CanvasGroup  canvasGroup;
    private RectTransform rt;
    private Vector2       anchoredOrigin;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        rt          = GetComponent<RectTransform>();
    }

    // ─── Public ───────────────────────────────────────────────────────────────

    public void Populate(ItemNotificationManager.NotificationData data)
    {
        bool isLoot = data.type == ItemNotificationManager.NotificationData.NotificationType.Loot;

        // ── Colour tint ────────────────────────────────────────────────────
        if (background != null)
            background.color = isLoot ? lootColor : pickupColor;

        // ── Badge icon ─────────────────────────────────────────────────────
        if (typeBadge != null)
        {
            typeBadge.sprite = isLoot ? lootBadgeSprite : pickupBadgeSprite;
            typeBadge.enabled = (typeBadge.sprite != null);
        }

        // ── Item icon ──────────────────────────────────────────────────────
        if (itemIcon != null)
        {
            itemIcon.sprite  = data.icon;
            itemIcon.enabled = (data.icon != null);
        }

        // ── Texts ──────────────────────────────────────────────────────────
        if (titleText != null)
            titleText.text = isLoot ? "¡Botín!" : "¡Recogiste!";

        if (nameText != null)
            nameText.text = data.amount > 1
                ? $"{data.itemName}  <size=85%>x{data.amount}</size>"
                : data.itemName;

        // ── Initial animation state ────────────────────────────────────────
        if (canvasGroup != null) canvasGroup.alpha = 0f;
        anchoredOrigin = rt != null ? rt.anchoredPosition : Vector2.zero;
    }

    // ─── Animations ───────────────────────────────────────────────────────────

    public IEnumerator AnimateIn(float duration)
    {
        float elapsed = 0f;
        Vector2 startPos = anchoredOrigin + new Vector2(slideOffset, 0f);
        Vector2 endPos   = anchoredOrigin;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);

            if (canvasGroup != null) canvasGroup.alpha           = t;
            if (rt          != null) rt.anchoredPosition = Vector2.Lerp(startPos, endPos, t);

            yield return null;
        }

        if (canvasGroup != null) canvasGroup.alpha           = 1f;
        if (rt          != null) rt.anchoredPosition = endPos;
    }

    public IEnumerator AnimateOut(float duration)
    {
        float elapsed  = 0f;
        float startAlpha = canvasGroup != null ? canvasGroup.alpha : 1f;
        Vector2 startPos = rt != null ? rt.anchoredPosition : Vector2.zero;
        Vector2 endPos   = startPos + new Vector2(slideOffset * 0.5f, 0f);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            if (canvasGroup != null) canvasGroup.alpha           = Mathf.Lerp(startAlpha, 0f, t);
            if (rt          != null) rt.anchoredPosition = Vector2.Lerp(startPos, endPos, t);

            yield return null;
        }

        if (canvasGroup != null) canvasGroup.alpha = 0f;
    }
}