using System.Collections;
using UnityEngine;

/// <summary>
/// Controla el ciclo de vida del efecto de lluvia digital:
///   1. Fade-IN opcional al activarse el GameObject.
///   2. Timeout automático o trigger manual para el fade-OUT.
///   3. Fade-OUT con curva SmoothStep, desactiva el GO al terminar.
///
/// Opera sobre el CanvasGroup del mismo GameObject, lo que permite
/// controlar la opacidad de todas las columnas con una sola propiedad
/// sin tocar cada columna individualmente.
///
/// INTEGRACIÓN CON MainPanel:
///   Conectar OnFadeOutComplete → MainPanel.ClosePanel() en el Inspector,
///   o llamar TriggerFadeOut() desde código cuando la carga de escena termine.
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class DigitalRainFader : MonoBehaviour
{
    // ── Configuración ──────────────────────────────────────────────────────────
    [Header("Timing")]
    [Tooltip("Segundos que el efecto corre antes del fade-out automático. 0 = solo manual vía TriggerFadeOut().")]
    public float autoFadeOutAfter = 5f;

    [Tooltip("Duración del fade-in al activarse. 0 = aparece instantáneo.")]
    public float fadeInDuration = 1.0f;

    [Tooltip("Duración del fade-out en segundos.")]
    public float fadeOutDuration = 1.5f;

    [Header("Callbacks")]
    [Tooltip("Se dispara al completar el fade-out. Conectar a MainPanel.ClosePanel() en el Inspector.")]
    public UnityEngine.Events.UnityEvent onFadeOutComplete;

    // ── Internos ───────────────────────────────────────────────────────────────
    private CanvasGroup _canvasGroup;
    private Coroutine   _activeFade;
    private bool        _fadingOut;

    // ─────────────────────────────────────────────────────────────────────────
    private void Awake()
    {
        _canvasGroup       = GetComponent<CanvasGroup>();
        _canvasGroup.alpha = (fadeInDuration > 0f) ? 0f : 1f;
    }

    /// <summary>
    /// OnEnable en lugar de Start para que el fade-in funcione cada vez que
    /// el GameObject se activa (no solo la primera vez).
    /// </summary>
    private void OnEnable()
    {
        _fadingOut         = false;
        _canvasGroup.alpha = (fadeInDuration > 0f) ? 0f : 1f;

        if (_activeFade != null)
            StopCoroutine(_activeFade);

        if (fadeInDuration > 0f)
            _activeFade = StartCoroutine(FadeRoutine(0f, 1f, fadeInDuration, OnFadeInComplete));
        else
            OnFadeInComplete();
    }

    private void OnDisable()
    {
        // Limpia corrutinas al desactivar para evitar errores si se reactiva rápido
        if (_activeFade != null)
        {
            StopCoroutine(_activeFade);
            _activeFade = null;
        }
    }

    // ── API Pública ────────────────────────────────────────────────────────────

    /// <summary>
    /// Inicia el fade-out manualmente. Llamar cuando la carga de escena termine,
    /// o desde un botón de "Saltar intro".
    /// </summary>
    public void TriggerFadeOut()
    {
        if (_fadingOut) return;
        _fadingOut = true;

        if (_activeFade != null)
            StopCoroutine(_activeFade);

        _activeFade = StartCoroutine(FadeRoutine(_canvasGroup.alpha, 0f, fadeOutDuration, OnFadeOutComplete));
    }

    // ── Callbacks internos ────────────────────────────────────────────────────
    private void OnFadeInComplete()
    {
        if (autoFadeOutAfter > 0f)
            _activeFade = StartCoroutine(AutoFadeOutRoutine());
    }

    private void OnFadeOutComplete()
    {
        onFadeOutComplete?.Invoke(); // ← dispara MainPanel.ClosePanel() u otro callback
        gameObject.SetActive(false);
    }

    // ── Corrutinas ─────────────────────────────────────────────────────────────
    private IEnumerator AutoFadeOutRoutine()
    {
        yield return new WaitForSeconds(autoFadeOutAfter);
        TriggerFadeOut();
    }

    /// <summary>
    /// Fade genérico con curva SmoothStep.
    ///
    /// MATEMÁTICA DE SMOOTHSTEP:
    ///   f(t) = t² * (3 - 2t)   donde t ∈ [0,1]
    ///   - f'(0) = 0, f'(1) = 0 → derivada nula en extremos = sin "corte" brusco.
    ///   - Acelera en el centro, desacelera al inicio y al final.
    ///   - Perceptualmente más suave que Lerp lineal para transiciones de alpha.
    ///
    ///   Mathf.SmoothStep(a, b, t) implementa:  a + (b - a) * t²(3 - 2t)
    /// </summary>
    private IEnumerator FadeRoutine(float from, float to, float duration, System.Action onComplete = null)
    {
        if (duration <= 0f)
        {
            _canvasGroup.alpha = to;
            onComplete?.Invoke();
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            _canvasGroup.alpha = Mathf.SmoothStep(from, to, Mathf.Clamp01(elapsed / duration));
            yield return null;
        }

        _canvasGroup.alpha = to; // garantiza valor exacto al terminar
        onComplete?.Invoke();
    }
}