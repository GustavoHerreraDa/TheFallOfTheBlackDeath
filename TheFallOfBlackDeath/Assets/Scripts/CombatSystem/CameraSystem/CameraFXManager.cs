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

    [Header("Lethal Warning")]
    [SerializeField] private Color lethalFilterColor = new Color(0.35f, 0.02f, 0.02f, 1f);
    [SerializeField] private float lethalVignetteIntensity = 0.65f;
    [SerializeField] private float lethalTransitionSpeed = 10f;

    private LensDistortion _lensDistortion;
    private ChromaticAberration _chromaticAberration;
    private ColorAdjustments _colorAdjustments;
    private Vignette _vignette;
    private Coroutine _distortionCoroutine;
    private Coroutine _hitStopCoroutine;
    private Coroutine _glitchCoroutine;
    private Coroutine _lethalWarningCoroutine;

    private Color _originalFilterColor = Color.white;
    private float _originalVignetteIntensity;

    private void Awake()
    {
        if (globalVolume != null)
        {
            // Acceso seguro a los componentes
            globalVolume.profile.TryGet(out _lensDistortion);
            globalVolume.profile.TryGet(out _chromaticAberration);
            globalVolume.profile.TryGet(out _colorAdjustments);
            globalVolume.profile.TryGet(out _vignette);

            if (_colorAdjustments != null)
                _originalFilterColor = _colorAdjustments.colorFilter.value;
            if (_vignette != null)
                _originalVignetteIntensity = _vignette.intensity.value;
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
    /// Dispara un screen shake via Cinemachine Impulse. No toca el TimeScale.
    /// </summary>
    public void PlayShake(float shakeForce)
    {
        if (impulseSource == null || shakeForce <= 0f) return;

        Vector3 randomDir = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0f).normalized;
        impulseSource.GenerateImpulse(randomDir * shakeForce);
    }

    /// <summary>
    /// Frena el tiempo brevemente con recuperación elástica.
    /// Solo arranca una nueva rutina si no hay una en curso, o si la nueva
    /// duración es mayor a la que ya está corriendo (hits más fuertes ganan).
    /// </summary>
    public void PlayHitStop(float duration)
    {
        if (duration <= 0f) return;

        // Si ya hay un HitStop en curso y el nuevo es menos intenso, lo ignoramos
        // para que la destrucción de parte (más dramática) no sea cancelada por
        // el shake que Fighter.cs dispara en la línea siguiente.
        if (_hitStopCoroutine != null && _pendingHitStopDuration >= duration) return;

        if (_hitStopCoroutine != null) StopCoroutine(_hitStopCoroutine);
        _pendingHitStopDuration = duration;
        _hitStopCoroutine = StartCoroutine(HitStopRoutine(duration));
    }

    // Guardamos la duración del HitStop activo para comparar en llamadas concurrentes
    private float _pendingHitStopDuration;

    /// <summary>
    /// Mantiene compatibilidad con llamadas legacy que pasaban ambos valores juntos.
    /// Solo invoca los dos métodos separados — no contiene lógica propia.
    /// </summary>
    public void PlayHitReactionEffects(float shakeForce, float hitStopDuration)
    {
        PlayShake(shakeForce);
        PlayHitStop(hitStopDuration);
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
        
        _pendingHitStopDuration = 0f;
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

    /// <summary>
    /// Activa o desactiva la advertencia visual de ataque letal.
    /// Transiciona el color filter hacia rojo sangre y aumenta la viñeta.
    /// </summary>
    public void SetLethalWarning(bool active)
    {
        if (_lethalWarningCoroutine != null) StopCoroutine(_lethalWarningCoroutine);
        _lethalWarningCoroutine = StartCoroutine(LethalWarningRoutine(active));
    }

    private IEnumerator LethalWarningRoutine(bool active)
    {
        Color targetColor = active ? lethalFilterColor : _originalFilterColor;
        float targetVignette = active ? lethalVignetteIntensity : _originalVignetteIntensity;

        bool hasColor = _colorAdjustments != null;
        bool hasVignette = _vignette != null;

        if (!hasColor && !hasVignette) yield break;

        if (hasColor) _colorAdjustments.colorFilter.overrideState = true;
        if (hasVignette) _vignette.intensity.overrideState = true;

        while (true)
        {
            float dt = Time.unscaledDeltaTime;
            float lerpFactor = 1f - Mathf.Exp(-lethalTransitionSpeed * dt);

            bool colorDone = true;
            bool vignetteDone = true;

            if (hasColor)
            {
                _colorAdjustments.colorFilter.value = Color.Lerp(
                    _colorAdjustments.colorFilter.value, targetColor, lerpFactor);
                colorDone = ColorApprox(_colorAdjustments.colorFilter.value, targetColor);
            }

            if (hasVignette)
            {
                _vignette.intensity.value = Mathf.Lerp(
                    _vignette.intensity.value, targetVignette, lerpFactor);
                vignetteDone = Mathf.Abs(_vignette.intensity.value - targetVignette) < 0.005f;
            }

            if (colorDone && vignetteDone) break;
            yield return null;
        }

        if (hasColor) _colorAdjustments.colorFilter.value = targetColor;
        if (hasVignette) _vignette.intensity.value = targetVignette;

        _lethalWarningCoroutine = null;
    }

    private static bool ColorApprox(Color a, Color b)
    {
        return Mathf.Abs(a.r - b.r) < 0.005f &&
               Mathf.Abs(a.g - b.g) < 0.005f &&
               Mathf.Abs(a.b - b.b) < 0.005f;
    }

    private void OnDisable()
    {
        // Cleanup de seguridad
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;
        if (_lensDistortion != null) _lensDistortion.intensity.value = 0f;
        if (_colorAdjustments != null) _colorAdjustments.colorFilter.value = _originalFilterColor;
        if (_vignette != null) _vignette.intensity.value = _originalVignetteIntensity;
    }
}