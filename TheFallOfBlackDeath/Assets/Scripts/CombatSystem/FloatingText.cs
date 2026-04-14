using UnityEngine;
using TMPro;

/// <summary>
/// Supports the combat system by handling floating text.
/// </summary>
public class FloatingText : MonoBehaviour
{
    public TextMeshProUGUI textMesh;
    
    [Header("Movimiento")]
    public float normalSpeed = 1.5f;
    public float criticalSpeed = 0.5f; // Que suba más lento en crítico le da más "peso"
    public Vector3 randomOffset = new Vector3(0.5f, 0, 0);

    [Header("Juiciness (Animación)")]
    [Tooltip("Curva de tamaño. Eje X es el tiempo (0 a 1), Eje Y es la escala.")]
    public AnimationCurve scaleCurve; 
    [Tooltip("Curva de desvanecimiento (Alpha).")]
    public AnimationCurve alphaCurve;

    [Header("Escala Base")]
    public float normalSize = 1f;
    public float criticalSize = 1.8f; // Críticos mucho más grandes

    [Header("Temblor Crítico (Jitter)")]
    public float jitterAmount = 0.05f; // Cuánto tiembla
    public float jitterSpeed = 35f;    // Qué tan rápido vibra

    private float timer;
    private float activeDuration = 1.5f; 
    private Vector3 startPos;
    private Transform mainCamera;
    private bool isCrit;

    /// <summary>
    /// Initializes cached references and runtime state before the component starts running.
    /// </summary>
    void Awake()
    {
        if (Camera.main != null) mainCamera = Camera.main.transform;
    }

    /// <summary>
    /// Initializes the ialize.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="color">The color.</param>
    /// <param name="isCritical">The is critical.</param>
    public void Initialize(string message, Color color, bool isCritical)
    {
        textMesh.text = message;
        textMesh.color = color;
        timer = 0;
        isCrit = isCritical;

        // Variación de Posición (Random offset)
        startPos = transform.position + new Vector3(
            Random.Range(-randomOffset.x, randomOffset.x),
            Random.Range(-randomOffset.y, randomOffset.y), 
            0
        );
        transform.position = startPos;
        
        // Lo empezamos en escala 0 para que la curva lo haga "explotar" hacia afuera
        transform.localScale = Vector3.zero; 
    }

    /// <summary>
    /// Updates the component each frame while it is active.
    /// </summary>
    void Update()
    {
        // Mirar a cámara (Billboard)
        if (mainCamera != null)
            transform.rotation = Quaternion.LookRotation(transform.position - mainCamera.position);

        timer += Time.deltaTime;
        float percent = timer / activeDuration; // Va de 0.0 a 1.0

        // 1. EVALUAR CURVA DE ESCALA (El efecto "Pop" y rebote)
        float currentScaleEval = scaleCurve.Evaluate(percent);
        float finalScale = currentScaleEval * (isCrit ? criticalSize : normalSize);
        transform.localScale = Vector3.one * finalScale;

        // 2. MOVIMIENTO HACIA ARRIBA + TEMBLOR
        float currentSpeed = isCrit ? criticalSpeed : normalSpeed;
        Vector3 currentPos = startPos + Vector3.up * (currentSpeed * timer);

        // Si es crítico, le sumamos un temblor matemático violento
        if (isCrit)
        {
            float jitterX = Mathf.Sin(Time.time * jitterSpeed) * jitterAmount;
            float jitterY = Mathf.Cos(Time.time * jitterSpeed * 1.2f) * jitterAmount;
            currentPos += new Vector3(jitterX, jitterY, 0);
        }

        transform.position = currentPos;

        // 3. FADE OUT (Usando la curva de Alpha)
        float currentAlpha = alphaCurve.Evaluate(percent);
        textMesh.color = new Color(textMesh.color.r, textMesh.color.g, textMesh.color.b, currentAlpha);

        // 4. RETORNO AL POOL
        if (timer >= activeDuration)
        {
            FloatingTextManager.Instance.ReturnToPool(this);
        }
    }
}
