using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.SceneManagement;
using InventoryNew;

/// <summary>
/// Persistent singleton that intercepts item additions to NewInventoryManager
/// and queues toast notifications. The UI side (ItemNotificationUI) lives in
/// each world scene and pulls from this queue when active.
///
/// Notifications are silently queued during combat (BATTLE_STATE) and flushed
/// automatically when the world scene loads.
/// </summary>
public class ItemNotificationManager : MonoBehaviour
{
    public static ItemNotificationManager Instance { get; private set; }

    // ─── Public notification data ────────────────────────────────────────────

    [System.Serializable]
    public class NotificationData
    {
        public string itemName;
        public Sprite icon;
        public int amount;
        public NotificationType type;
        public float timestamp;

        public enum NotificationType { Pickup, Loot }
    }

    // ─── Internal state ──────────────────────────────────────────────────────

    private readonly Queue<NotificationData> pendingQueue = new Queue<NotificationData>();
    private ItemNotificationUI activeUI;
    private Coroutine deferredFlushRoutine;

    [Header("Scene Filter")]
    [SerializeField] private int worldSceneBuildIndex = 1;
    [SerializeField] private string worldSceneName;

    // ─── Events ──────────────────────────────────────────────────────────────

    public event Action<NotificationData> OnNotificationReady;

    // ─────────────────────────────────────────────────────────────────────────

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;

        if (deferredFlushRoutine != null)
        {
            StopCoroutine(deferredFlushRoutine);
            deferredFlushRoutine = null;
        }
    }

    // ─── Scene lifecycle ─────────────────────────────────────────────────────

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // The UI registers itself via RegisterUI(); flush pending queue once it does.
        // We also unhook from the previous UI automatically.
        activeUI = null;

        if (deferredFlushRoutine != null)
        {
            StopCoroutine(deferredFlushRoutine);
        }

        deferredFlushRoutine = StartCoroutine(FlushPendingQueueAtEndOfFrame());
    }

    /// <summary>
    /// Called by ItemNotificationUI.OnEnable() in world scenes.
    /// </summary>
    public void RegisterUI(ItemNotificationUI ui)
    {
        if (ui == null) return;

        activeUI = ui;

        if (IsWorldScene())
        {
            FlushPendingQueue();
        }
    }

    /// <summary>
    /// Called by ItemNotificationUI.OnDisable().
    /// </summary>
    public void UnregisterUI(ItemNotificationUI ui)
    {
        if (activeUI == ui)
            activeUI = null;
    }

    // ─── Public API ──────────────────────────────────────────────────────────

    /// <summary>
    /// Queue a pickup notification (called from NewItemPickup or any world pickup).
    /// </summary>
    public void NotifyPickup(NewItemData item, int amount = 1)
    {
        if (item == null) return;
        Enqueue(item.itemName, item.icon, amount, NotificationData.NotificationType.Pickup);
    }

    /// <summary>
    /// Queue a loot notification (called after combat victory for each loot entry).
    /// Notifications queued during combat will be shown when the world scene resumes.
    /// </summary>
    public void NotifyLoot(NewItemData item, int amount = 1)
    {
        if (item == null) return;
        Enqueue(item.itemName, item.icon, amount, NotificationData.NotificationType.Loot);
    }

    // ─── Internal helpers ────────────────────────────────────────────────────

    private void Enqueue(string name, Sprite icon, int amount, NotificationData.NotificationType type)
    {
        var data = new NotificationData
        {
            itemName  = name,
            icon      = icon,
            amount    = amount,
            type      = type,
            timestamp = Time.time
        };

        // If we're in a world scene and a UI is ready, fire immediately.
        if (activeUI != null && IsWorldScene())
        {
            activeUI.ShowNotification(data);
        }
        else
        {
            // During combat or before UI is ready: buffer for later.
            pendingQueue.Enqueue(data);
        }
    }

    private void FlushPendingQueue()
    {
        if (activeUI == null) return;

        while (pendingQueue.Count > 0)
        {
            var data = pendingQueue.Dequeue();
            activeUI.ShowNotification(data);
        }
    }

    private IEnumerator FlushPendingQueueAtEndOfFrame()
    {
        yield return new WaitForEndOfFrame();

        if (activeUI != null && IsWorldScene())
        {
            FlushPendingQueue();
        }

        deferredFlushRoutine = null;
    }

    private bool IsWorldScene()
    {
        var activeScene = SceneManager.GetActiveScene();

        bool matchesScene = true;
        bool hasSceneFilter = false;

        if (worldSceneBuildIndex >= 0)
        {
            hasSceneFilter = true;
            matchesScene &= activeScene.buildIndex == worldSceneBuildIndex;
        }

        if (!string.IsNullOrWhiteSpace(worldSceneName))
        {
            hasSceneFilter = true;
            matchesScene &= string.Equals(activeScene.name, worldSceneName, StringComparison.Ordinal);
        }

        if (!hasSceneFilter)
        {
            matchesScene = true;
        }

        bool inBattleState = GameManager.Instance != null
                             && GameManager.Instance.gameState == GameManager.GameStates.BATTLE_STATE;

        return matchesScene && !inBattleState;
    }
}