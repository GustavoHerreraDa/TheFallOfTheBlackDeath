using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Cinemachine; // ADD THIS

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance;
    
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
    [SerializeField] private float selectionZoomFOV = 45f;
    [SerializeField] private float selectionZoomSpeed = 5f;
    private bool isHoveringEnemy = false;
    private bool isHitActive = false;

    private float defaultFOV;
    private Coroutine hitCoroutine;

    [SerializeField]
    private float cameraSpeed;

    [Header("Screen Shake (Cinemachine)")]
    [SerializeField] private CinemachineImpulseSource impulseSource; // ADD THIS

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        combatManager = FindObjectOfType<CombatManager>();
    }

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

    private void Update()
    {
        FighterIndex = combatManager.fighterIndex;

        if (currentCameraIndex != combatManager.fighterIndex)
        {
            currentCameraIndex = combatManager.fighterIndex;

            if (currentCameraIndex >= 0 && currentCameraIndex < combatManager.fighters.Length)
            {
                ChangeCameraPositionToCurrentFighter();
            }
        }
        
        // Lógica de Zoom (FOV) y Respiración
        if (!isHitActive && mainCamera != null)
        {
            if (enableBreathing)
            {
                ApplyBreathingEffect();
            }
            else
            {
                // Tu código original por si decides apagar la respiración
                float targetFOV = isHoveringEnemy ? selectionZoomFOV : defaultFOV;
                float newFOV = Mathf.Lerp(mainCamera.fieldOfView, targetFOV, Time.deltaTime * selectionZoomSpeed);
                UpdateFOV(newFOV);
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

    private void UpdateFOV(float fov)
    {
        if (mainCamera != null) mainCamera.fieldOfView = fov;
        if (shaderCamera != null) shaderCamera.fieldOfView = fov;
    }

    public void SetSelectionZoom(bool active)
    {
        isHoveringEnemy = active;
    }

    public void PlayHitCameraEffect(Transform attacker, Transform defender)
    {
        if (hitCoroutine != null)
            StopCoroutine(hitCoroutine);

        hitCoroutine = StartCoroutine(HitCameraEffect(attacker, defender));
    }

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

    private void ChangeCameraPositionToCurrentFighter()
    {
        
        var currentFighter = combatManager.fighters[FighterIndex];
        StartCoroutine(MoveCameraSmoothly(mainCamera.transform.position, currentFighter.CameraPivot.position, mainCamera.transform.rotation, currentFighter.CameraPivot.rotation, cameraSpeed));
        
    }

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
    
    public void TriggerDamageGlitch()
    {/*if (chromaticAberration == null) return;

        // Si ya hay un glitch ocurriendo, lo reiniciamos
        if (glitchCoroutine != null) StopCoroutine(glitchCoroutine);
        glitchCoroutine = StartCoroutine(GlitchRoutine());
        */
    }

    /*IEnumerator GlitchRoutine()
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
    */
    
// --- NUEVA LÓGICA DE SCREEN SHAKE CON CINEMACHINE ---
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
    public void TriggerHitStop(float duration = -1f)
    {
        if (duration < 0) duration = defaultHitStopDuration;
        StartCoroutine(HitStopRoutine(duration));
    }

    private IEnumerator HitStopRoutine(float duration)
    {
        // 1. Ralentizamos el tiempo al 5% (casi congelado)
        Time.timeScale = 0.05f; 
        
        // 2. Esperamos en TIEMPO REAL (independiente del timeScale)
        yield return new WaitForSecondsRealtime(duration); 
        
        // 3. Restauramos la velocidad normal del juego
        Time.timeScale = 1f;
    }
    
    private void ApplyBreathingEffect()
    {
        if (combatManager == null || combatManager.fighters == null || FighterIndex < 0 || FighterIndex >= combatManager.fighters.Length) return;

        var currentFighter = combatManager.fighters[FighterIndex];
        if (currentFighter == null || !currentFighter.isAlive) return;
        
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

        //Calculamos la respiración SOLO si no estamos apuntando al enemigo
        float breathOffset = 0f;
        if (!isHoveringEnemy)
        {
            
            currentBreathTime += Time.deltaTime * currentSpeed;
            
            breathOffset = Mathf.Sin(currentBreathTime) * currentAmplitude;
        }

        
        float targetFOV = (isHoveringEnemy ? selectionZoomFOV : defaultFOV) + breathOffset;

        
        float newFOV = Mathf.Lerp(mainCamera.fieldOfView, targetFOV, Time.deltaTime * selectionZoomSpeed);
        UpdateFOV(newFOV);
    }
}
