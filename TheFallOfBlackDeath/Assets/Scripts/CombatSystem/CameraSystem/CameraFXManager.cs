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

    [Header("Lethal Warning - Visuals")]
    [SerializeField] private Color lethalFilterColor = new Color(0.35f, 0.02f, 0.02f, 1f);
    [SerializeField] private float lethalVignetteIntensity = 0.65f;
    [SerializeField] private float lethalTransitionSpeed = 10f;
    [Tooltip("Velocidad de la oscilación de la alarma/pulsación.")]
    [SerializeField] private float pulseSpeed = 12f;

    [Header("Lethal Warning - Audio")]
    [SerializeField] private AudioClip lethalWarningSound;
    [Range(0f, 1f)] [SerializeField] private float warningAudioVolume = 0.8f;

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
    
    // Almacena el AudioSource persistente creado por el AudioManager
    private AudioSource _activeLethalAudioSource;

    private void Awake()
    {
        if (globalVolume != null)
        {
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

    public void SetHoverDistortion(bool active)
    {
        if (_distortionCoroutine != null) StopCoroutine(_distortionCoroutine);
        float target = active ? hoverDistortionIntensity : 0f;
        _distortionCoroutine = StartCoroutine(LerpDistortionRoutine(target));
    }

    private IEnumerator LerpDistortionRoutine(float target)
    {
        if (_lensDistortion == null) yield break;

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

    public void PlayShake(float shakeForce)
    {
        if (impulseSource == null || shakeForce <= 0f) return;
        Vector3 randomDir = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0f).normalized;
        impulseSource.GenerateImpulse(randomDir * shakeForce);
    }

    public void PlayHitStop(float duration)
    {
        if (duration <= 0f) return;
        if (_hitStopCoroutine != null && _pendingHitStopDuration >= duration) return;

        if (_hitStopCoroutine != null) StopCoroutine(_hitStopCoroutine);
        _pendingHitStopDuration = duration;
        _hitStopCoroutine = StartCoroutine(HitStopRoutine(duration));
    }

    private float _pendingHitStopDuration;

    public void PlayHitReactionEffects(float shakeForce, float hitStopDuration)
    {
        PlayShake(shakeForce);
        PlayHitStop(hitStopDuration);
    }

    private IEnumerator HitStopRoutine(float duration)
    {
        Time.timeScale = hitStopTimeScale;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;
            Time.timeScale = Mathf.Lerp(hitStopTimeScale, 1.0f, t);
            Time.fixedDeltaTime = 0.02f * Time.timeScale;
            yield return null;
        }

        Time.timeScale = 1.0f;
        Time.fixedDeltaTime = 0.02f;

        if (CameraDirector.Instance != null)
        {
            CameraDirector.Instance.ChangeState(CameraState.Overview);
        }
        
        _pendingHitStopDuration = 0f;
        _hitStopCoroutine = null;
    }

    public void TriggerDamageGlitch()
    {
        if (_chromaticAberration == null) return;
        if (_glitchCoroutine != null) StopCoroutine(_glitchCoroutine);
        _glitchCoroutine = StartCoroutine(GlitchRoutine());
    }

    private IEnumerator GlitchRoutine()
    {
        float t = 0;
        float peakTime = 0.05f;
        while (t < peakTime)
        {
            t += Time.unscaledDeltaTime;
            _chromaticAberration.intensity.value = Mathf.Lerp(0, maxGlitchIntensity, t / peakTime);
            yield return null;
        }

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
    /// Gestiona la alerta visual pulsante y el bucle de audio persistente del ataque letal.
    /// </summary>
    public void SetLethalWarning(bool active)
    {
        if (_lethalWarningCoroutine != null) StopCoroutine(_lethalWarningCoroutine);
        
        // Gestión del audio persistente usando el AudioManager existente
        if (active)
        {
            if (AudioManager.Instance != null && lethalWarningSound != null && _activeLethalAudioSource == null)
            {
                _activeLethalAudioSource = AudioManager.Instance.PlayPersistentSFX(lethalWarningSound, warningAudioVolume, true, false);
            }
        }
        else
        {
            if (AudioManager.Instance != null && _activeLethalAudioSource != null)
            {
                AudioManager.Instance.StopPersistentSFX(_activeLethalAudioSource);
                _activeLethalAudioSource = null;
            }
        }

        _lethalWarningCoroutine = StartCoroutine(LethalWarningPulsingRoutine(active));
    }

    private IEnumerator LethalWarningPulsingRoutine(bool active)
    {
        bool hasColor = _colorAdjustments != null;
        bool hasVignette = _vignette != null;

        if (!hasColor && !hasVignette) yield break;

        if (hasColor) _colorAdjustments.colorFilter.overrideState = true;
        if (hasVignette) _vignette.intensity.overrideState = true;

        if (active)
        {
            // Bucle continuo de pulsación mientras la alerta esté activa
            while (active)
            {
                // Usamos unscaledTime para que la onda de seno mantenga su velocidad real durante el slow-mo del QTE
                float sineWave = Mathf.Sin(Time.unscaledTime * pulseSpeed);
                float pulseFactor = (sineWave + 1f) * 0.5f; // Convertimos rango de (-1, 1) a (0, 1)

                if (hasColor)
                {
                    _colorAdjustments.colorFilter.value = Color.Lerp(_originalFilterColor, lethalFilterColor, pulseFactor);
                }

                if (hasVignette)
                {
                    _vignette.intensity.value = Mathf.Lerp(_originalVignetteIntensity, lethalVignetteIntensity, pulseFactor);
                }

                yield return null;
            }
        }
        else
        {
            // Retorno limpio a los valores originales de la escena al desactivarse
            while (true)
            {
                float dt = Time.unscaledDeltaTime;
                float lerpFactor = 1f - Mathf.Exp(-lethalTransitionSpeed * dt);

                bool colorDone = true;
                bool vignetteDone = true;

                if (hasColor)
                {
                    _colorAdjustments.colorFilter.value = Color.Lerp(_colorAdjustments.colorFilter.value, _originalFilterColor, lerpFactor);
                    colorDone = ColorApprox(_colorAdjustments.colorFilter.value, _originalFilterColor);
                }

                if (hasVignette)
                {
                    _vignette.intensity.value = Mathf.Lerp(_vignette.intensity.value, _originalVignetteIntensity, lerpFactor);
                    vignetteDone = Mathf.Abs(_vignette.intensity.value - _originalVignetteIntensity) < 0.005f;
                }

                if (colorDone && vignetteDone) break;
                yield return null;
            }

            if (hasColor) _colorAdjustments.colorFilter.value = _originalFilterColor;
            if (hasVignette) _vignette.intensity.value = _originalVignetteIntensity;
        }

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
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;
        
        if (AudioManager.Instance != null && _activeLethalAudioSource != null)
        {
            AudioManager.Instance.StopPersistentSFX(_activeLethalAudioSource);
            _activeLethalAudioSource = null;
        }

        if (_lensDistortion != null) _lensDistortion.intensity.value = 0f;
        if (_colorAdjustments != null) _colorAdjustments.colorFilter.value = _originalFilterColor;
        if (_vignette != null) _vignette.intensity.value = _originalVignetteIntensity;
    }
}