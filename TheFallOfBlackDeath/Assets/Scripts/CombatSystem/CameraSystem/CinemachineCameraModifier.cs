using UnityEngine;
using Cinemachine;

/// <summary>
/// Extensión de Cinemachine para aplicar efectos procedimentales de Mouse Tracking y Breathing
/// sin interferir con los solvers nativos, evitando el "Transform Stomping".
/// </summary>
public class CinemachineCameraModifier : CinemachineExtension
{
    [Header("Mouse Tracking (Stage.Aim)")]
    [Tooltip("Ángulo máximo de rotación inducido por el mouse.")]
    public float maxRotationAngle = 2f;
    [Tooltip("Suavizado del seguimiento del mouse.")]
    public float trackingSmoothness = 5f;

    [Header("Breathing Effect (Stage.Lens)")]
    public float normalAmplitude = 0.5f;
    public float normalFrequency = 2f;
    public float panicAmplitude = 2.5f;
    public float panicFrequency = 7f;

    [Header("Runtime State")]
    [Range(0f, 1f)] public float panicFactor = 0f; // Controlado por el CameraDirector

    private Vector2 _smoothedMousePos;
    private float _breathingCycle;

    protected override void PostPipelineStageCallback(
        CinemachineVirtualCameraBase vcam,
        CinemachineCore.Stage stage,
        ref Cinemachine.CameraState state,
        float deltaTime)
    {
        // Usamos unscaledDeltaTime para que los efectos visuales sigan fluyendo durante el HitStop
        float delta = Time.unscaledDeltaTime;

        // Implementamos el seguimiento del mouse en la etapa de Aim para no romper el Follow
        if (stage == CinemachineCore.Stage.Aim)
        {
            ApplyMouseTracking(ref state, delta);
        }

        // Implementamos la respiración en la etapa de Lens para modificar el FOV limpiamente
        if (stage == CinemachineCore.Stage.Finalize)
        {
            ApplyBreathing(ref state, delta);
        }
    }

    private void ApplyMouseTracking(ref Cinemachine.CameraState state, float deltaTime)
    {
        if (Screen.width <= 0 || Screen.height <= 0) return;

        // Normalizamos la posición del mouse (-1 a 1)
        Vector2 mouseInput = new Vector2(
            Mathf.Clamp((Input.mousePosition.x / Screen.width) * 2f - 1f, -1f, 1f),
            Mathf.Clamp((Input.mousePosition.y / Screen.height) * 2f - 1f, -1f, 1f)
        );

        // Lerp amortiguado por deltaTime
        _smoothedMousePos = Vector2.Lerp(_smoothedMousePos, mouseInput, 1f - Mathf.Exp(-trackingSmoothness * deltaTime));

        Quaternion trackingRot = Quaternion.Euler(
            -_smoothedMousePos.y * maxRotationAngle,
            _smoothedMousePos.x * maxRotationAngle,
            0f
        );

        // Aplicamos la rotación sobre la orientación ya calculada por Cinemachine
        state.RawOrientation *= trackingRot;
    }

    private void ApplyBreathing(ref Cinemachine.CameraState state, float deltaTime)
    {
        // Interpolamos frecuencia y amplitud basado en el nivel de pánico (vida baja)
        float currentFreq = Mathf.Lerp(normalFrequency, panicFrequency, panicFactor);
        float currentAmp = Mathf.Lerp(normalAmplitude, panicAmplitude, panicFactor);

        _breathingCycle += deltaTime * currentFreq;
        float offset = Mathf.Sin(_breathingCycle) * currentAmp;

        // Modificamos el FOV de forma aditiva
        state.Lens.FieldOfView += offset;
    }
}
