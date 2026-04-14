using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
//TP2 FACUNDO FERREIRO
[RequireComponent(typeof(CanvasGroup))]
/// <summary>
/// Supports the combat system by handling log panel.
/// </summary>
public class LogPanel : MonoBehaviour
{
    //Referencia estatica al panel actual
    protected static LogPanel current;
    //Este panel tiene una referencia a la etiqueta de texto
    public TextMeshProUGUI logLabel;

    [Header("Fade Settings")]
    public float fadeInTime = 0.35f;
    public float visibleDuration = 2f;
    public float fadeOutTime = 0.35f;

    private CanvasGroup canvasGroup;
    private Coroutine fadeRoutine;

    /// <summary>
    /// Initializes cached references and runtime state before the component starts running.
    /// </summary>
    private void Awake()
    {
        current = this;
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
    }

    //Funcion estatica write para escribir un mensaje
    /// <summary>
    /// Executes the write workflow.
    /// </summary>
    /// <param name="message">The message.</param>
    public static void Write(string message)
    {
        if (current == null)
            return;
        if (current.fadeRoutine != null)
        {
            current.StopCoroutine(current.fadeRoutine);
            current.fadeRoutine = null;
        }
        current.logLabel.text = message;
        current.fadeRoutine = current.StartCoroutine(current.ShowMessageRoutine());
    }

    /// <summary>
    /// Shows the message routine.
    /// </summary>
    /// <returns>An enumerator that drives the coroutine sequence.</returns>
    private IEnumerator ShowMessageRoutine()
    {
        // Fade In
        yield return FadeTo(1f, fadeInTime);
        // Stay visible
        float t = 0f;
        while (t < visibleDuration)
        {
            t += Time.deltaTime;
            yield return null;
        }
        // Fade Out
        yield return FadeTo(0f, fadeOutTime);
        fadeRoutine = null;
    }

    /// <summary>
    /// Executes the fade to workflow.
    /// </summary>
    /// <param name="target">The target.</param>
    /// <param name="duration">The duration.</param>
    /// <returns>An enumerator that drives the coroutine sequence.</returns>
    private IEnumerator FadeTo(float target, float duration)
    {
        float start = canvasGroup.alpha;
        float elapsed = 0f;
        if (duration <= 0f)
        {
            canvasGroup.alpha = target;
            yield break;
        }
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            canvasGroup.alpha = Mathf.Lerp(start, target, t);
            yield return null;
        }
        canvasGroup.alpha = target;
    }

    /// <summary>
    /// Executes the write workflow.
    /// </summary>
    /// <param name="idName">The id name.</param>
    /// <param name="v">The v.</param>
    internal static void Write(string idName, string v)
    {
        throw new NotImplementedException();
    }
}
