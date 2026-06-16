using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Attach this to a UI panel in the world scene Canvas (NOT the combat scene).
/// Displays queued item toast notifications in a bottom corner of the screen.
///
/// HIERARCHY SUGGESTION:
///   Canvas (Screen Space - Overlay)
///   └── ItemNotificationUI  [this script]
///       └── NotificationSlot (prefab reference — assign in Inspector)
///           ├── Background     (Image)
///           ├── Icon           (Image)
///           ├── TextGroup
///           │   ├── TitleText  (TMP_Text)  e.g. "Item Obtained" / "Loot Drop"
///           │   └── NameText   (TMP_Text)  e.g. "Iron Sword x1"
///           └── AmountBadge
///               └── AmountText (TMP_Text)
/// </summary>
public class ItemNotificationUI : MonoBehaviour
{
    [Header("Prefab & Spawn")]
    [Tooltip("Prefab for a single toast row. See NotificationSlotUI.")]
    public NotificationSlotUI slotPrefab;

    [Tooltip("Parent RectTransform where slots are stacked. Use a Vertical Layout Group.")]
    public RectTransform slotsContainer;

    [Header("Timing")]
    [Tooltip("How long each toast stays fully visible.")]
    public float displayDuration = 3.0f;

    [Tooltip("Slide-in and fade-in duration.")]
    public float animInDuration  = 0.3f;

    [Tooltip("Fade-out duration.")]
    public float animOutDuration = 0.4f;

    [Header("Queue")]
    [Tooltip("Maximum simultaneous toasts on screen.")]
    public int maxSimultaneous = 3;

    // ─── Internal ─────────────────────────────────────────────────────────────

    private readonly Queue<ItemNotificationManager.NotificationData> displayQueue
        = new Queue<ItemNotificationManager.NotificationData>();

    private readonly List<NotificationSlotUI> activeSlots = new List<NotificationSlotUI>();

    // ─────────────────────────────────────────────────────────────────────────

    private void OnEnable()
    {
        TryRegister("OnEnable");
    }

    private void Start()
    {
        TryRegister("Start");
    }

    private void OnDisable()
    {
        if (ItemNotificationManager.Instance != null)
            ItemNotificationManager.Instance.UnregisterUI(this);
    }

    // ─── Public API ───────────────────────────────────────────────────────────

    public void ShowNotification(ItemNotificationManager.NotificationData data)
    {
        if (activeSlots.Count < maxSimultaneous)
        {
            SpawnSlot(data);
        }
        else
        {
            displayQueue.Enqueue(data);
        }
    }

    // ─── Internal helpers ─────────────────────────────────────────────────────

    private void SpawnSlot(ItemNotificationManager.NotificationData data)
    {
        if (slotPrefab == null || slotsContainer == null)
        {
            Debug.LogWarning("[ItemNotificationUI] slotPrefab or slotsContainer not assigned.");
            return;
        }

        var slot = Instantiate(slotPrefab, slotsContainer);
        slot.Populate(data);
        activeSlots.Add(slot);

        StartCoroutine(SlotLifetime(slot));
    }

    private void TryRegister(string source)
    {
        var manager = ItemNotificationManager.Instance;
        if (manager != null)
        {
            manager.RegisterUI(this);
            return;
        }

        Debug.LogWarning($"[ItemNotificationUI] ItemNotificationManager.Instance es null en {source}. Se reintentará cuando el manager exista.");
    }

    private IEnumerator SlotLifetime(NotificationSlotUI slot)
    {
        // ── Animate in ─────────────────────────────────────────────────────
        yield return StartCoroutine(slot.AnimateIn(animInDuration));

        // ── Hold ───────────────────────────────────────────────────────────
        yield return new WaitForSeconds(displayDuration);

        // ── Animate out ────────────────────────────────────────────────────
        yield return StartCoroutine(slot.AnimateOut(animOutDuration));

        // ── Cleanup ────────────────────────────────────────────────────────
        activeSlots.Remove(slot);
        Destroy(slot.gameObject);

        // ── Dequeue next if any ────────────────────────────────────────────
        if (displayQueue.Count > 0)
        {
            SpawnSlot(displayQueue.Dequeue());
        }
    }
}