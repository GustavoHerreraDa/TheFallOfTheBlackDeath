using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Cinemachine; // ADD THIS

/// <summary>
/// Handles camera manager for the current project workflow.
/// </summary>
public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance;
    
    [Header("Mouse Tracking (Menu Effect)")]
    public bool enableMouseTracking = false; // Solo activar en menús o momentos tranquilos
    public float maxRotationAngle = 2f;      // Qué tanto rotará la cámara
    public float trackingSmoothness = 5f;    // Suavidad del movimiento
    
    [Header("Breathing Effect (Health Based)")]
    public bool enableBreathing = true;
    public float normalBreathSpeed = 2f;
    public float normalBreathAmplitude = 0.5f; // Movimiento suave del FOV
    
    public float panicBreathSpeed = 7f; // Hiperventilación
    public float panicBreathAmplitude = 2.5f; // Pulso profundo
    [Range(0f, 1f)] public float lowHealthThreshold = 0.4f; // Se agita a partir del 40% de vida

    private float currentBreathTime = 0f;
    
    [Header("Juice / Game Feel")]
    [SerializeField] private float defaultShakeDuration = 0.2f;
    [SerializeField] private float defaultShakeMagnitude = 0.3f;
    [SerializeField] private float defaultHitStopDuration = 0.1f;

    private Coroutine shakeCoroutine;
    
    
    
    private CombatManager combatManager;
    private ChromaticAberration chromaticAberration;
    [Header("Cameras")]
    public Camera mainCamera;       
    public Camera shaderCamera;     

    public int currentCameraIndex;
    public int FighterIndex;
    public GameObject gameObjectFighter;

    [Header("Post Processing (FX)")]
    public Volume globalVolume; // <--- ARRASTRA TU GLOBAL VOLUME AQUÍ
    private LensDistortion lensDistortion; // Referencia interna al efecto
    [Range(-1f, 1f)]
    public float targetDistortion = -0.4f; // Intensidad al seleccionar (-0.5 es un buen valor tipo "succión")
    public float distortionSpeed = 5f;

    [Header("Hit Camera Effect")]
    [SerializeField] private float hitZoomFOV = 40f;
    [SerializeField] private float hitZoomSpeed = 12f;
    [SerializeField] private float hitRecoverSpeed = 8f;
    [SerializeField] private float hitMoveAmount = 0.3f;
    
    [Header("Glitch Effect Settings")]
    public float glitchDuration = 0.2f;
    public float maxGlitchIntensity = 1f;
    
    private Coroutine glitchCoroutine;
    
    [Header("Selection Zoom (UI Hover)")]
    [SerializeField] private float zoomDistance = 0.6f;
    [SerializeField] private Vector3 cameraOffset = new Vector3(1.5f, 0f, 0f);
    [SerializeField] private float selectionZoomSpeed = 5f;
    private bool isHoveringEnemy = false;
    private Fighter hoverTarget;
    private bool isHitActive = false;

    private float defaultFOV;
    private Coroutine hitCoroutine;

    [SerializeField]
    private float cameraSpeed;

    [Header("Screen Shake (Cinemachine)")]
    [SerializeField] private CinemachineImpulseSource impulseSource; // ADD THIS

    /// <summary>
    /// Initializes cached references and runtime state before the component starts running.
    /// </summary>
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        combatManager = FindObjectOfType<CombatManager>();
    }

    /// <summary>
    /// Initializes the component once the scene dependencies are ready.
    /// </summary>
    void Start()
    {
        currentCameraIndex = combatManager.fighterIndex;
        
        if (mainCamera != null) defaultFOV = mainCamera.fieldOfView;
        
        UpdateFOV(defaultFOV);

        // === INICIALIZAR LENS DISTORTION ===
        if (globalVolume != null)
        {
            globalVolume.profile.TryGet(out lensDistortion);
            globalVolume.profile.TryGet(out chromaticAberration); // <--- OBTENEMOS EL EFECTO
        }
        else
        {
            Debug.LogWarning("CameraManager: No has asignado el 'Global Volume' en el inspector.");
        }
    }

    /// <summary>
    /// Updates the component each frame while it is active.
    /// </summary>
    private void Update()
    {
        // Solo proceder si el combate está activo y listo
        if (combatManager == null || !combatManager.isCombatActive) return;

        FighterIndex = combatManager.fighterIndex;

        // Identificar jugador activo
        if (FighterIndex < 0 || FighterIndex >= combatManager.fighters.Length) return;
        var activeFighter = combatManager.fighters[FighterIndex];
        if (activeFighter == null || activeFighter.CameraPivot == null) return;

        // 1. CALCULO DEL PUNTO DE ENFOQUE DINÁMICO
        Vector3 enemyFocusPoint = Vector3.zero;
        int aliveEnemies = 0;

        if (isHoveringEnemy && hoverTarget != null)
        {
            enemyFocusPoint = hoverTarget.transform.position;
        }
        else
        {
            // Centro del grupo enemigo
            foreach (var enemy in combatManager.enemyTeam)
            {
                if (enemy != null && enemy.isAlive)
                {
                    enemyFocusPoint += enemy.transform.position;
                    aliveEnemies++;
                }
            }
            if (aliveEnemies > 0) enemyFocusPoint /= aliveEnemies;
            else enemyFocusPoint = activeFighter.transform.position + activeFighter.transform.forward * 5f; // Fallback
        }

        Vector3 midpoint = (activeFighter.transform.position + enemyFocusPoint) * 0.5f;

        // 2. CÁLCULO DE POSICIÓN Y ROTACIÓN OBJETIVO
        if (!isHitActive && mainCamera != null)
        {
            // Posición base desde el pivote del jugador
            Vector3 basePos = activeFighter.CameraPivot.position;
            
            // Si estamos haciendo zoom, nos movemos hacia el midpoint
            float currentZoomFactor = isHoveringEnemy ? zoomDistance : 0.1f; // Pequeño zoom incluso en idle para encuadrar
            Vector3 targetPos = Vector3.Lerp(basePos, midpoint, currentZoomFactor);

            // Aplicar Offset Lateral Estable (Regla de los tercios)
            Vector3 dirToEnemy = (enemyFocusPoint - activeFighter.transform.position).normalized;
            Vector3 sideDir = Vector3.Cross(Vector3.up, dirToEnemy).normalized;
            
            targetPos += sideDir * cameraOffset.x;
            targetPos += Vector3.up * cameraOffset.y;
            targetPos += dirToEnemy * cameraOffset.z;

            // Interpolación suave de posición
            mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, targetPos, Time.deltaTime * selectionZoomSpeed);

            // Rotación: Mirar siempre al midpoint
            if (midpoint != mainCamera.transform.position)
            {
                Quaternion targetRot = Quaternion.LookRotation(midpoint - mainCamera.transform.position);
                mainCamera.transform.rotation = Quaternion.Slerp(mainCamera.transform.rotation, targetRot, Time.deltaTime * selectionZoomSpeed);
            }

            // FOV Constante (ej. 60) para evitar distorsiones, sumando respiración si aplica
            float breathOffset = 0f;
            if (enableBreathing)
            {
                breathOffset = CalculateBreathingOffset(activeFighter);
            }
            UpdateFOV(60f + breathOffset);

            if (enableMouseTracking) 
            {
                ApplyMouseTracking();
            }
        }

        // === LÓGICA DE DISTORSIÓN DE LENTE ===
        if (lensDistortion != null && !isHitActive)
        {
            // Si estamos encima del enemigo, usamos el valor target (ej: -0.4), si no, 0.
            float targetValue = isHoveringEnemy ? targetDistortion : 0f;
            
            // Interpolamos suavemente el valor actual hacia el target
            float newValue = Mathf.Lerp(lensDistortion.intensity.value, targetValue, Time.deltaTime * distortionSpeed);
            
            lensDistortion.intensity.value = newValue;
        }
    }

    /// <summary>
    /// Updates the fov.
    /// </summary>
    /// <param name="fov">The fov.</param>
    private void UpdateFOV(float fov)
    {
        if (mainCamera != null) mainCamera.fieldOfView = fov;
        if (shaderCamera != null) shaderCamera.fieldOfView = fov;
    }

    /// <summary>
    /// Sets the selection zoom.
    /// </summary>
    /// <param name="active">The active.</param>
    /// <param name="target">The target fighter.</param>
    public void SetSelectionZoom(bool active, Fighter target = null)
    {
        isHoveringEnemy = active;
        hoverTarget = target;
    }

    /// <summary>
    /// Executes the play hit camera effect workflow.
    /// </summary>
    /// <param name="attacker">The attacker.</param>
    /// <param name="defender">The defender.</param>
    public void PlayHitCameraEffect(Transform attacker, Transform defender)
    {
        if (hitCoroutine != null)
            StopCoroutine(hitCoroutine);

        hitCoroutine = StartCoroutine(HitCameraEffect(attacker, defender));
    }

    /// <summary>
    /// Executes the hit camera effect workflow.
    /// </summary>
    /// <param name="attacker">The attacker.</param>
    /// <param name="defender">The defender.</param>
    /// <returns>An enumerator that drives the coroutine sequence.</returns>
    IEnumerator HitCameraEffect(Transform attacker, Transform defender)
    {
        isHitActive = true;
        
        Vector3 originalPos = mainCamera.transform.position;
        // Guardamos la distorsión actual para restaurarla o manipularla en el golpe si quisieras
        float startDistortion = lensDistortion != null ? lensDistortion.intensity.value : 0f;

        Vector3 hitPoint = (attacker.position + defender.position) * 0.5f;
        Vector3 dirToHit = (hitPoint - mainCamera.transform.position).normalized;
        Vector3 zoomPos = mainCamera.transform.position + dirToHit * hitMoveAmount;

        float t = 0f;

        // ZOOM IN (GOLPE)
        while (t < 1f)
        {
            t += Time.deltaTime * hitZoomSpeed;
            
            float currentFOV = Mathf.Lerp(defaultFOV, hitZoomFOV, t);
            Vector3 currentPos = Vector3.Lerp(originalPos, zoomPos, t);

            // Opcional: Aumentar distorsión violentamente en el golpe
            if (lensDistortion != null) 
                lensDistortion.intensity.value = Mathf.Lerp(startDistortion, -0.6f, t); 

            UpdateFOV(currentFOV);
            mainCamera.transform.position = currentPos;
            
            yield return null;
        }

        yield return new WaitForSeconds(0.05f);

        t = 0f;

        // RECOVER (RECUPERACIÓN)
        while (t < 1f)
        {
            t += Time.deltaTime * hitRecoverSpeed;
            
            float currentFOV = Mathf.Lerp(hitZoomFOV, defaultFOV, t);
            Vector3 currentPos = Vector3.Lerp(zoomPos, originalPos, t);
            
            // Regresar distorsión a 0
            if (lensDistortion != null) 
                lensDistortion.intensity.value = Mathf.Lerp(-0.6f, 0f, t);

            UpdateFOV(currentFOV);
            mainCamera.transform.position = currentPos;
            
            yield return null;
        }

        UpdateFOV(defaultFOV);
        mainCamera.transform.position = originalPos;
        if(lensDistortion != null) lensDistortion.intensity.value = 0f; // Asegurar reset
        
        isHitActive = false;
    }

    /// <summary>
    /// Changes the camera position to current fighter.
    /// </summary>
    private void ChangeCameraPositionToCurrentFighter()
    {
        
        var currentFighter = combatManager.fighters[FighterIndex];
        StartCoroutine(MoveCameraSmoothly(mainCamera.transform.position, currentFighter.CameraPivot.position, mainCamera.transform.rotation, currentFighter.CameraPivot.rotation, cameraSpeed));
        
    }

    /// <summary>
    /// Executes the move camera smoothly workflow.
    /// </summary>
    /// <param name="startPos">The start pos.</param>
    /// <param name="endPos">The end pos.</param>
    /// <param name="startRot">The start rot.</param>
    /// <param name="endRot">The end rot.</param>
    /// <param name="speed">The speed.</param>
    /// <returns>An enumerator that drives the coroutine sequence.</returns>
    IEnumerator MoveCameraSmoothly(Vector3 startPos, Vector3 endPos, Quaternion startRot, Quaternion endRot, float speed)
    {
        float t = 0;

        while (t < 1)
        {
            t += Time.deltaTime * speed;
            mainCamera.transform.position = Vector3.Lerp(startPos, endPos, t);
            mainCamera.transform.rotation = Quaternion.Slerp(startRot, endRot, t);
            yield return null;
        }
    }
    
    /// <summary>
    /// Executes the trigger damage glitch workflow.
    /// </summary>
    public void TriggerDamageGlitch()
    { if (chromaticAberration == null) return;

        // Si ya hay un glitch ocurriendo, lo reiniciamos
        if (glitchCoroutine != null) StopCoroutine(glitchCoroutine);
        glitchCoroutine = StartCoroutine(GlitchRoutine());
        
    }

        /// <summary>
        /// Executes the glitch routine workflow.
        /// </summary>
        /// <returns>An enumerator that drives the coroutine sequence.</returns>
        IEnumerator GlitchRoutine()
    {
        // 1. Subida brusca (Impacto)
        float t = 0;
        while(t < 0.05f) // Muy rápido
        {
            t += Time.deltaTime;
            chromaticAberration.intensity.value = Mathf.Lerp(0, maxGlitchIntensity, t / 0.05f);
            yield return null;
        }

        // 2. Bajada suave (Recuperación)
        t = 0;
        while (t < glitchDuration)
        {
            t += Time.deltaTime;
            chromaticAberration.intensity.value = Mathf.Lerp(maxGlitchIntensity, 0, t / glitchDuration);
            yield return null;
        }

        chromaticAberration.intensity.value = 0;
    }
    
    
// --- NUEVA LÓGICA DE SCREEN SHAKE CON CINEMACHINE ---
    /// <summary>
    /// Executes the trigger shake workflow.
    /// </summary>
    /// <param name="force">The force.</param>
    public void TriggerShake(float force)
    {
        if (impulseSource != null)
        {
            // Generamos una dirección aleatoria para que cada impacto vibre distinto
            Vector3 randomDirection = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0).normalized;
            
            // Disparamos el impulso multiplicando la dirección por la fuerza
            impulseSource.GenerateImpulse(randomDirection * force); 
        }
    }

    /// <summary>
    /// Executes the shake routine workflow.
    /// </summary>
    /// <param name="duration">The duration.</param>
    /// <param name="magnitude">The magnitude.</param>
    /// <returns>An enumerator that drives the coroutine sequence.</returns>
    private IEnumerator ShakeRoutine(float duration, float magnitude)
    {
        Vector3 originalLocalPos = mainCamera.transform.localPosition;
        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            // Generamos posiciones aleatorias para simular la vibración
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            mainCamera.transform.localPosition = new Vector3(originalLocalPos.x + x, originalLocalPos.y + y, originalLocalPos.z);
            
            // IMPORTANTE: Usamos unscaledDeltaTime para que la cámara tiemble incluso si el tiempo está congelado
            elapsed += Time.unscaledDeltaTime; 
            yield return null;
        }

        // Devolvemos la cámara a su posición original
        mainCamera.transform.localPosition = originalLocalPos;
    }

    // --- LÓGICA DE HIT STOP (Pausa de Impacto) ---
    /// <summary>
    /// Executes the trigger hit stop workflow.
    /// </summary>
    /// <param name="duration">The duration.</param>
    public void TriggerHitStop(float duration = -1f)
    {
        if (duration < 0) duration = defaultHitStopDuration;
        StartCoroutine(HitStopRoutine(duration));
    }

    // Modifica este método en CameraManager.cs
    /// <summary>
    /// Executes the hit stop routine workflow.
    /// </summary>
    /// <param name="duration">The duration.</param>
    /// <returns>An enumerator that drives the coroutine sequence.</returns>
    private IEnumerator HitStopRoutine(float duration)
    {
        float elapsed = 0f;
        // Bajamos la velocidad drásticamente al inicio para marcar el impacto
        float targetTimeScale = 0.1f; 
    
        Time.timeScale = targetTimeScale;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;

        // Mientras dure el efecto, vamos devolviendo el tiempo a la normalidad
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime; // Importante usar unscaled aquí
        
            // Curva de recuperación: va de 0.1f a 1f
            Time.timeScale = Mathf.Lerp(targetTimeScale, 1f, elapsed / duration);
            Time.fixedDeltaTime = 0.02f * Time.timeScale;
        
            yield return null;
        }

        // Aseguramos el estado final
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;
    }
    
    /// <summary>
    /// Calculates the breathing offset based on fighter health.
    /// </summary>
    private float CalculateBreathingOffset(Fighter currentFighter)
    {
        if (currentFighter == null || currentFighter.GetCurrentStats() == null || !currentFighter.isAlive) return 0f;

        float currentHp = currentFighter.GetCurrentStats().health;
        float maxHp = currentFighter.GetCurrentStats().maxHealth;
        float hpPercent = currentHp / Mathf.Max(1f, maxHp);

        float currentSpeed = normalBreathSpeed;
        float currentAmplitude = normalBreathAmplitude;

        if (hpPercent <= lowHealthThreshold)
        {
            float panicFactor = 1f - (hpPercent / lowHealthThreshold);
            currentSpeed = Mathf.Lerp(normalBreathSpeed, panicBreathSpeed, panicFactor);
            currentAmplitude = Mathf.Lerp(normalBreathAmplitude, panicBreathAmplitude, panicFactor);
        }

        currentBreathTime += Time.deltaTime * currentSpeed;
        return Mathf.Sin(currentBreathTime) * currentAmplitude;
    }

    /// <summary>
    /// Applies the breathing effect. (Legacy, now using CalculateBreathingOffset)
    /// </summary>
    private void ApplyBreathingEffect()
    {
        // No longer used directly, but kept for compatibility if needed.
    }
    
    private void ApplyMouseTracking()
    {
        // 1. Verificación de seguridad básica
        if (!enableMouseTracking || isHitActive || combatManager == null) return;

        // 2. CORRECCIÓN: Usar .Length en lugar de .Count para el Array de fighters
        if (FighterIndex < 0 || FighterIndex >= combatManager.fighters.Length) return;

        var targetFighter = combatManager.fighters[FighterIndex];

        // 3. Verificar que el luchador y su pivot no sean nulos
        if (targetFighter == null || targetFighter.CameraPivot == null) return;

        // 4. Obtención de posición del mouse (-1 a 1)
        float mouseX = (Input.mousePosition.x / Screen.width) * 2f - 1f;
        float mouseY = (Input.mousePosition.y / Screen.height) * 2f - 1f;

        // 5. Cálculo de rotación relativa
        Quaternion mouseOffset = Quaternion.Euler(-mouseY * maxRotationAngle, mouseX * maxRotationAngle, 0f);
    
        // Usamos la rotación del pivot del luchador como base
        Quaternion baseRotation = targetFighter.CameraPivot.rotation;

        // 6. Aplicamos la rotación combinada de forma suave
        mainCamera.transform.rotation = Quaternion.Slerp(
            mainCamera.transform.rotation, 
            baseRotation * mouseOffset, 
            Time.deltaTime * trackingSmoothness
        );
    }
}
