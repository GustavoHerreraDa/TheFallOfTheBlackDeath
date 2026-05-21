using UnityEngine;

/// <summary>
/// Fachada para el sistema de cámaras refactorizado. 
/// Mantiene la compatibilidad con el código heredado mientras delega la responsabilidad
/// al CameraDirector y CameraFXManager siguiendo principios SOLID.
/// </summary>
public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance;

    [Header("New System Bridges")]
    [SerializeField] private CameraDirector director;
    [SerializeField] private CameraFXManager fxManager;

    [Header("Legacy References (For external access)")]
    public Camera mainCamera;
    public Camera shaderCamera;

    public int fighterIndex; // Mantenido por compatibilidad si algún script lo consulta

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        // Intento de auto-vinculación si no se asignaron en el inspector
        if (director == null) director = CameraDirector.Instance;
        if (fxManager == null) fxManager = FindObjectOfType<CameraFXManager>();
    }

    /// <summary>
    /// Activa el zoom y efectos de hover sobre un objetivo táctico.
    /// </summary>
    public void SetSelectionZoom(bool active, Fighter target = null)
    {
        if (director != null)
        {
            director.SetSelectionZoom(active, target);
        }
    }

    /// <summary>
    /// Dispara un efecto de sacudida de cámara.
    /// </summary>
    public void TriggerShake(float force)
    {
        if (fxManager != null)
        {
            // Disparamos solo el shake (duración de hitstop = 0)
            fxManager.PlayHitReactionEffects(force, 0f);
        }
    }

    /// <summary>
    /// Congela el tiempo brevemente para enfatizar un impacto.
    /// </summary>
    public void TriggerHitStop(float duration)
    {
        if (fxManager != null)
        {
            // Disparamos solo el hitstop (fuerza de shake = 0)
            fxManager.PlayHitReactionEffects(0f, duration);
        }
    }

    /// <summary>
    /// Ejecuta un efecto de aberración cromática (glitch).
    /// </summary>
    public void TriggerDamageGlitch()
    {
        if (fxManager != null)
        {
            fxManager.TriggerDamageGlitch();
        }
    }

    // Nota: Toda la lógica de suavizado, seguimiento de mouse y respiración 
    // ha sido migrada a CinemachineCameraModifier y CinemachineTargetSystem 
    // para eliminar el jittering y el Transform Stomping.
}
