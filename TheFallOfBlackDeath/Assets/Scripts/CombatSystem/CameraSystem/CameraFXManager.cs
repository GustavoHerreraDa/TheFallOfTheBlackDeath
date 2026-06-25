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
    public static CameraFXManager Instance { get; private set; }

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

    [Header("Scan Effect")]
    [SerializeField] private Color scanFilterColor = new Color(0.05f, 0.55f, 0.25f, 1f);
    [SerializeField] private float scanFlashDuration = 0.08f;
    [SerializeField] private float scanDecayDuration = 0.6f;
    [SerializeField] private float scanHueShift = 80f;       // grados, verde = ~80-120
    [SerializeField] private float scanSaturation = 30f;     // boost de saturación

    [Header("Combat Scanner Effect")]
    [SerializeField] private Color combatScanColor = new Color(0f, 1f, 0.4f, 1f);
    [SerializeField] private float combatScanHueShift = 100f;
    [SerializeField] private float combatScanSaturation = 60f;
    [SerializeField] private float combatScanFadeInDuration = 0.2f;
    [SerializeField] private float combatScanFadeOutDuration = 0.15f;
    [SerializeField] private float combatScanPulseIntensity = 0.2f;
    [SerializeField] private float combatScanPulseSpeed = 8f;
    [SerializeField] private AudioClip combatScanSound;

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
    private float _originalHueShift;
    private float _originalSaturation;
    
    // Almacena el AudioSource persistente creado por el AudioManager
    private AudioSource _activeLethalAudioSource;
    private Coroutine _scanCoroutine;
    private Coroutine _combatScanCoroutine;

    private void Awake()
    {
        if (Instance == null) Instance = this; else Destroy(gameObject);

        if (globalVolume != null)
        {
            globalVolume.profile.TryGet(out _lensDistortion);
            globalVolume.profile.TryGet(out _chromaticAberration);
            globalVolume.profile.TryGet(out _colorAdjustments);
            globalVolume.profile.TryGet(out _vignette);

            if (_colorAdjustments != null)
            {
                _originalFilterColor = _colorAdjustments.colorFilter.value;
                _originalHueShift = _colorAdjustments.hueShift.value;
                _originalSaturation = _colorAdjustments.saturation.value;
            }
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

        if (CameraDirector.Instance != null && CameraDirector.Instance.CurrentState != CameraState.Ui)
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

    public void SetScanEffect(bool active)
    {
        if (_scanCoroutine != null) StopCoroutine(_scanCoroutine);
        _scanCoroutine = StartCoroutine(ScanEffectRoutine(active));
    }

    /// <summary>
    /// Efecto de escaneo persistente para el scanner de combate.
    /// A diferencia de SetScanEffect, la pantalla se mantiene verde mientras
    /// active == true. Llamar con false para restaurar.
    /// </summary>
    public void SetCombatScanEffect(bool active)
    {
        if (_combatScanCoroutine != null)
        {
            StopCoroutine(_combatScanCoroutine);
            _combatScanCoroutine = null;
        }

        if (active && combatScanSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(combatScanSound);
        }

        _combatScanCoroutine = StartCoroutine(CombatScanRoutine(active));
    }

    private IEnumerator ScanEffectRoutine(bool active)
    {
        if (_colorAdjustments == null) yield break;

        _colorAdjustments.colorFilter.overrideState = true;
        _colorAdjustments.hueShift.overrideState = true;
        _colorAdjustments.saturation.overrideState = true;

        if (active)
        {
            // Flash inmediato al color de escaneo
            _colorAdjustments.colorFilter.value = scanFilterColor;
            _colorAdjustments.hueShift.value = scanHueShift;
            _colorAdjustments.saturation.value = _originalSaturation + scanSaturation;

            yield return new WaitForSecondsRealtime(scanFlashDuration);

            // Decay suave hacia el color original
            float elapsed = 0f;
            Color startColor = scanFilterColor;
            float startHue = scanHueShift;
            float startSat = _originalSaturation + scanSaturation;

            while (elapsed < scanDecayDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / scanDecayDuration;

                _colorAdjustments.colorFilter.value = Color.Lerp(startColor, _originalFilterColor, t);
                _colorAdjustments.hueShift.value = Mathf.Lerp(startHue, _originalHueShift, t);
                _colorAdjustments.saturation.value = Mathf.Lerp(startSat, _originalSaturation, t);

                yield return null;
            }

            // Snap final para evitar drift numérico
            _colorAdjustments.colorFilter.value = _originalFilterColor;
            _colorAdjustments.hueShift.value = _originalHueShift;
            _colorAdjustments.saturation.value = _originalSaturation;
        }
        else
        {
            // Cierre: restaurar inmediatamente
            _colorAdjustments.colorFilter.value = _originalFilterColor;
            _colorAdjustments.hueShift.value = _originalHueShift;
            _colorAdjustments.saturation.value = _originalSaturation;
        }

        _scanCoroutine = null;
    }

    private IEnumerator CombatScanRoutine(bool active)
    {
        if (_colorAdjustments == null) yield break;

        _colorAdjustments.colorFilter.overrideState = true;
        _colorAdjustments.hueShift.overrideState    = true;
        _colorAdjustments.saturation.overrideState  = true;

        if (active)
        {
            // Fade in hacia el color de escaneo
            float elapsed = 0f;
            Color  startColor = _colorAdjustments.colorFilter.value;
            float  startHue   = _colorAdjustments.hueShift.value;
            float  startSat   = _colorAdjustments.saturation.value;

            while (elapsed < combatScanFadeInDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / combatScanFadeInDuration);

                _colorAdjustments.colorFilter.value  = Color.Lerp(startColor, combatScanColor, t);
                _colorAdjustments.hueShift.value     = Mathf.Lerp(startHue, combatScanHueShift, t);
                _colorAdjustments.saturation.value   = Mathf.Lerp(startSat, _originalSaturation + combatScanSaturation, t);

                yield return null;
            }

            // Bucle continuo de pulsación mientras el scanner esté activo
            while (active)
            {
                // Usamos unscaledTime para que la pulsación sea constante incluso en slow-motion
                float sineWave = Mathf.Sin(Time.unscaledTime * combatScanPulseSpeed);
                float pulseFactor = (sineWave + 1f) * 0.5f; // Rango (0, 1)
                
                // Variamos la saturación y el color ligeramente para el efecto de pulsación
                _colorAdjustments.colorFilter.value = Color.Lerp(combatScanColor, Color.white, pulseFactor * combatScanPulseIntensity);
                _colorAdjustments.saturation.value = (_originalSaturation + combatScanSaturation) + (sineWave * 10f);
                
                yield return null;
            }
        }
        else
        {
            // Fade out hacia los valores originales
            float elapsed    = 0f;
            Color startColor = _colorAdjustments.colorFilter.value;
            float startHue   = _colorAdjustments.hueShift.value;
            float startSat   = _colorAdjustments.saturation.value;

            while (elapsed < combatScanFadeOutDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / combatScanFadeOutDuration);

                _colorAdjustments.colorFilter.value  = Color.Lerp(startColor, _originalFilterColor, t);
                _colorAdjustments.hueShift.value     = Mathf.Lerp(startHue, _originalHueShift, t);
                _colorAdjustments.saturation.value   = Mathf.Lerp(startSat, _originalSaturation, t);

                yield return null;
            }

            // Snap final
            _colorAdjustments.colorFilter.value  = _originalFilterColor;
            _colorAdjustments.hueShift.value     = _originalHueShift;
            _colorAdjustments.saturation.value   = _originalSaturation;
        }

        _combatScanCoroutine = null;
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

        if (_combatScanCoroutine != null)
        {
            StopCoroutine(_combatScanCoroutine);
            _combatScanCoroutine = null;
        }

        if (_lensDistortion != null) _lensDistortion.intensity.value = 0f;
        if (_colorAdjustments != null)
        {
            _colorAdjustments.colorFilter.value = _originalFilterColor;
            _colorAdjustments.hueShift.value = _originalHueShift;
            _colorAdjustments.saturation.value = _originalSaturation;
        }
        if (_vignette != null) _vignette.intensity.value = _originalVignetteIntensity;
    }
}