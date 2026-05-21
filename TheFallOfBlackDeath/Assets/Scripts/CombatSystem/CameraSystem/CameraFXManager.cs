using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Cinemachine;
using System.Collections;

/// <summary>
/// Gestiona efectos visuales desacoplados: Post-Processing, Shakes e HitStop.
/// Utiliza tiempo real desescalado para evitar bloqueos físicos.
/// </summary>
public class CameraFXManager : MonoBehaviour
{
    [Header("Post-Processing (URP)")]
    [SerializeField] private Volume globalVolume;
    [SerializeField] private float distortionSpeed = 8f;
    [SerializeField] private float hoverDistortionIntensity = -0.4f;

    [Header("Hit Reaction Settings")]
    [SerializeField] private CinemachineImpulseSource impulseSource;
    [Range(0f, 1f)] [SerializeField] private float hitStopTimeScale = 0.02f;

    [Header("Damage Glitch")]
    [SerializeField] private float glitchDuration = 0.2f;
    [SerializeField] private float maxGlitchIntensity = 1f;

    private LensDistortion _lensDistortion;
    private ChromaticAberration _chromaticAberration;
    private Coroutine _distortionCoroutine;
    private Coroutine _hitStopCoroutine;
    private Coroutine _glitchCoroutine;

    private void Awake()
    {
        if (globalVolume != null)
        {
            // Acceso seguro a los componentes
            globalVolume.profile.TryGet(out _lensDistortion);
            globalVolume.profile.TryGet(out _chromaticAberration);
        }
    }

    /// <summary>
    /// Aplica una distorsión de lente suave al hacer hover sobre un objetivo.
    /// </summary>
    public void SetHoverDistortion(bool active)
    {
        if (_distortionCoroutine != null) StopCoroutine(_distortionCoroutine);
        float target = active ? hoverDistortionIntensity : 0f;
        _distortionCoroutine = StartCoroutine(LerpDistortionRoutine(target));
    }

    private IEnumerator LerpDistortionRoutine(float target)
    {
        if (_lensDistortion == null) yield break;

        // Uso de unscaledDeltaTime para que la UI/Hover responda bien incluso en HitStop
        while (Mathf.Abs(_lensDistortion.intensity.value - target) > 0.001f)
        {
            _lensDistortion.intensity.value = Mathf.Lerp(
                _lensDistortion.intensity.value, 
                target, 
                1f - Mathf.Exp(-distortionSpeed * Time.unscaledDeltaTime)
            );
            yield return null;
        }
        _lensDistortion.intensity.value = target;
    }

    /// <summary>
    /// Dispara el efecto de impacto: Shake e HitStop elástico.
    /// </summary>
    public void PlayHitReactionEffects(float shakeForce, float hitStopDuration)
    {
        // Screen Shake nativo de Cinemachine (caótico y perpendicular)
        if (impulseSource != null)
        {
            Vector3 randomDir = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0).normalized;
            impulseSource.GenerateImpulse(randomDir * shakeForce);
        }

        // Control de corrutinas previas para evitar race conditions en el TimeScale
        if (_hitStopCoroutine != null) StopCoroutine(_hitStopCoroutine);
        _hitStopCoroutine = StartCoroutine(HitStopRoutine(hitStopDuration));
    }

    private IEnumerator HitStopRoutine(float duration)
    {
        // Descenso súbito del tiempo
        Time.timeScale = hitStopTimeScale;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            
            // Recuperación elástica/suavizada del tiempo hacia 1.0
            float t = elapsed / duration;
            Time.timeScale = Mathf.Lerp(hitStopTimeScale, 1.0f, t);
            
            // Sincronización del motor de física (importante para evitar stuttering en colisiones si las hubiera)
            Time.fixedDeltaTime = 0.02f * Time.timeScale;
            
            yield return null;
        }

        // Aseguramos estado normal
        Time.timeScale = 1.0f;
        Time.fixedDeltaTime = 0.02f;

        // Invocamos al director para retornar al estado base de Overview
        if (CameraDirector.Instance != null)
        {
            CameraDirector.Instance.ChangeState(CameraState.Overview);
        }
        
        _hitStopCoroutine = null;
    }

    /// <summary>
    /// Ejecuta un glitch cromático rápido al recibir daño.
    /// </summary>
    public void TriggerDamageGlitch()
    {
        if (_chromaticAberration == null) return;
        if (_glitchCoroutine != null) StopCoroutine(_glitchCoroutine);
        _glitchCoroutine = StartCoroutine(GlitchRoutine());
    }

    private IEnumerator GlitchRoutine()
    {
        float t = 0;
        // Pico rápido
        float peakTime = 0.05f;
        while (t < peakTime)
        {
            t += Time.unscaledDeltaTime;
            _chromaticAberration.intensity.value = Mathf.Lerp(0, maxGlitchIntensity, t / peakTime);
            yield return null;
        }

        // Recuperación suave
        t = 0;
        while (t < glitchDuration)
        {
            t += Time.unscaledDeltaTime;
            _chromaticAberration.intensity.value = Mathf.Lerp(maxGlitchIntensity, 0, t / glitchDuration);
            yield return null;
        }

        _chromaticAberration.intensity.value = 0;
        _glitchCoroutine = null;
    }

    private void OnDisable()
    {
        // Cleanup de seguridad
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;
        if (_lensDistortion != null) _lensDistortion.intensity.value = 0f;
    }
}
