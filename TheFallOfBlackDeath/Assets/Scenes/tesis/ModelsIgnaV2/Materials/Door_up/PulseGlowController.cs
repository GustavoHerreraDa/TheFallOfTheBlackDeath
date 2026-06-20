using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controlador para el shader PulseGlowUI.
/// Coloca este componente en cualquier Image o Text (TMP funciona con
/// un material que use el mismo shader).
///
/// Flujo típico:
///   - En reposo: el shader anima el pulso solo con _PulseSpeed.
///   - OnBodyPartDestroyed / OnProstheticDestroyed / StatusModSkill:
///     llama a TriggerEvent() para forzar un pulso inmediato.
/// </summary>
[RequireComponent(typeof(Graphic))]
public class PulseGlowController : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Configuración del shader
    // -------------------------------------------------------------------------
    [Header("Pulse")]
    [SerializeField, Range(0.1f, 6f)]  float pulseSpeed     = 1.2f;
    [SerializeField, Range(0f, 1f)]    float pulseMinAlpha  = 0.55f;
    [SerializeField, Range(0f, 1f)]    float pulseMaxAlpha  = 1.0f;

    [Header("Glow on peak")]
    [SerializeField] Color  glowColor     = new Color(0.6f, 0.2f, 1f, 1f);
    [SerializeField, Range(0.5f, 5f)]  float glowIntensity  = 2.0f;
    [SerializeField, Range(0f, 1f)]    float glowRadius     = 0.35f;
    [SerializeField, Range(0.5f, 1f)]  float pulseThreshold = 0.85f;

    [Header("Peak flicker")]
    [SerializeField, Range(1f, 20f)]   float flickerSpeed   = 8f;
    [SerializeField, Range(0f, 0.3f)]  float flickerAmount  = 0.08f;

    [Header("Evento: colores por tipo")]
    [SerializeField] Color destroyColor  = new Color(1f, 0.3f, 0.1f, 1f);
    [SerializeField] Color statModColor  = new Color(0f, 0.9f, 0.6f, 1f);
    [SerializeField] Color defaultColor  = new Color(0.6f, 0.2f, 1f, 1f);

    // -------------------------------------------------------------------------
    // IDs de propiedades (cacheados para no hacer string lookups en Update)
    // -------------------------------------------------------------------------
    static readonly int ID_PulseSpeed    = Shader.PropertyToID("_PulseSpeed");
    static readonly int ID_MinAlpha      = Shader.PropertyToID("_PulseMinAlpha");
    static readonly int ID_MaxAlpha      = Shader.PropertyToID("_PulseMaxAlpha");
    static readonly int ID_GlowColor     = Shader.PropertyToID("_GlowColor");
    static readonly int ID_GlowIntensity = Shader.PropertyToID("_GlowIntensity");
    static readonly int ID_GlowRadius    = Shader.PropertyToID("_GlowRadius");
    static readonly int ID_Threshold     = Shader.PropertyToID("_PulseThreshold");
    static readonly int ID_FlickerSpeed  = Shader.PropertyToID("_FlickerSpeed");
    static readonly int ID_FlickerAmt    = Shader.PropertyToID("_FlickerAmount");
    static readonly int ID_ForcePulse    = Shader.PropertyToID("_ForcePulse");

    // -------------------------------------------------------------------------
    // Runtime
    // -------------------------------------------------------------------------
    Material        _mat;
    Graphic         _graphic;
    Coroutine       _forcePulseRoutine;

    // -------------------------------------------------------------------------
    void Awake()
    {
        _graphic = GetComponent<Graphic>();

        // Instanciamos el material para no compartir estado entre elementos
        _mat = new Material(_graphic.material);
        _graphic.material = _mat;

        ApplyStaticProperties();
    }

    void ApplyStaticProperties()
    {
        _mat.SetFloat(ID_PulseSpeed,    pulseSpeed);
        _mat.SetFloat(ID_MinAlpha,      pulseMinAlpha);
        _mat.SetFloat(ID_MaxAlpha,      pulseMaxAlpha);
        _mat.SetColor(ID_GlowColor,     glowColor);
        _mat.SetFloat(ID_GlowIntensity, glowIntensity);
        _mat.SetFloat(ID_GlowRadius,    glowRadius);
        _mat.SetFloat(ID_Threshold,     pulseThreshold);
        _mat.SetFloat(ID_FlickerSpeed,  flickerSpeed);
        _mat.SetFloat(ID_FlickerAmt,    flickerAmount);
        _mat.SetFloat(ID_ForcePulse,    0f);
    }

    // -------------------------------------------------------------------------
    // API pública — llama desde tus sistemas de combate
    // -------------------------------------------------------------------------

    public enum EventType { Default, BodyPartDestroyed, ProstheticDestroyed, StatMod }

    /// <summary>
    /// Dispara un pulso forzado con la curva y color del tipo de evento.
    /// Duración total ~0.8 s (sube en 0.3 s, baja en 0.5 s).
    /// </summary>
    public void TriggerEvent(EventType type = EventType.Default, float duration = 0.8f)
    {
        Color eventColor = type switch
        {
            EventType.BodyPartDestroyed   => destroyColor,
            EventType.ProstheticDestroyed => destroyColor,
            EventType.StatMod             => statModColor,
            _                             => defaultColor,
        };

        if (_forcePulseRoutine != null) StopCoroutine(_forcePulseRoutine);
        _forcePulseRoutine = StartCoroutine(ForcePulseRoutine(eventColor, duration));
    }

    /// <summary>Versión con color personalizado (útil para damage de colores por elemento).</summary>
    public void TriggerEvent(Color color, float duration = 0.8f)
    {
        if (_forcePulseRoutine != null) StopCoroutine(_forcePulseRoutine);
        _forcePulseRoutine = StartCoroutine(ForcePulseRoutine(color, duration));
    }

    // -------------------------------------------------------------------------
    // Coroutine interna
    // -------------------------------------------------------------------------
    IEnumerator ForcePulseRoutine(Color eventColor, float duration)
    {
        // Guarda el color original y sobreescribe temporalmente
        Color originalColor = _mat.GetColor(ID_GlowColor);
        _mat.SetColor(ID_GlowColor, eventColor);

        float riseTime  = duration * 0.35f;
        float holdTime  = duration * 0.05f;
        float fallTime  = duration * 0.60f;

        // Subida
        float elapsed = 0f;
        while (elapsed < riseTime)
        {
            _mat.SetFloat(ID_ForcePulse, Mathf.SmoothStep(0f, 1f, elapsed / riseTime));
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Hold en peak
        _mat.SetFloat(ID_ForcePulse, 1f);
        yield return new WaitForSeconds(holdTime);

        // Caída
        elapsed = 0f;
        while (elapsed < fallTime)
        {
            _mat.SetFloat(ID_ForcePulse, Mathf.SmoothStep(1f, 0f, elapsed / fallTime));
            elapsed += Time.deltaTime;
            yield return null;
        }

        _mat.SetFloat(ID_ForcePulse, 0f);
        _mat.SetColor(ID_GlowColor, originalColor);
        _forcePulseRoutine = null;
    }

    // -------------------------------------------------------------------------
    // Actualizar en caliente desde Inspector (útil en Play Mode)
    // -------------------------------------------------------------------------
    void OnValidate()
    {
        if (_mat == null) return;
        ApplyStaticProperties();
    }

    void OnDestroy()
    {
        if (_mat != null) Destroy(_mat);
    }

    // -------------------------------------------------------------------------
    // Helpers de integración con tus sistemas existentes
    // -------------------------------------------------------------------------

    /// <summary>
    /// Llama esto desde Fighter.cs en el loop de OnBodyPartDestroyed.
    /// Ejemplo:
    ///   GetComponentInChildren&lt;PulseGlowController&gt;()?
    ///       .TriggerBodyPartDestroyed();
    /// </summary>
    public void TriggerBodyPartDestroyed() =>
        TriggerEvent(EventType.BodyPartDestroyed, duration: 1.0f);

    /// <summary>
    /// Llama esto desde Fighter.cs en el loop de OnProstheticDestroyed.
    /// </summary>
    public void TriggerProstheticDestroyed() =>
        TriggerEvent(EventType.ProstheticDestroyed, duration: 1.0f);

    /// <summary>
    /// Llama esto desde StatusModSkill al aplicar una modificación de stat.
    /// </summary>
    public void TriggerStatMod() =>
        TriggerEvent(EventType.StatMod, duration: 0.7f);
}
