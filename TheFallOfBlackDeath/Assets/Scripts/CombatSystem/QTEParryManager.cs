using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles the parry reaction QTE presentation, input window, and slow-motion timing.
/// It is intentionally unaware of combat, damage, or health resolution.
/// </summary>
public class QTEParryManager : MonoBehaviour
{
    public static QTEParryManager Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private Image reactionFlash;

    [Header("Input")]
    [SerializeField] private string parryButtonName = "Parry";

    private bool isParryActive;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (reactionFlash != null)
            reactionFlash.gameObject.SetActive(false);
    }

    /// <summary>
    /// Opens a real-time parry window while the game runs in slow-motion.
    /// </summary>
    /// <param name="windowDuration">Reaction window duration in unscaled seconds.</param>
    /// <param name="slowMoTimeScale">Time scale used while the QTE is active.</param>
    /// <param name="onResult">Callback invoked with true when the player parries in time.</param>
    public IEnumerator WaitForParry(float windowDuration, float slowMoTimeScale, Action<bool> onResult)
    {
        if (isParryActive)
        {
            Debug.LogWarning("A parry QTE is already active. The new request will fail safely.");
            onResult?.Invoke(false);
            yield break;
        }

        isParryActive = true;
        bool parried = false;
        float elapsed = 0f;

        if (reactionFlash != null)
            reactionFlash.gameObject.SetActive(true);

        Time.timeScale = Mathf.Clamp(slowMoTimeScale, 0.01f, 1f);

        while (elapsed < windowDuration)
        {
            if (Input.GetButtonDown(parryButtonName))
            {
                parried = true;
                break;
            }

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        Time.timeScale = 1f;

        if (reactionFlash != null)
            reactionFlash.gameObject.SetActive(false);

        isParryActive = false;
        onResult?.Invoke(parried);
    }
}