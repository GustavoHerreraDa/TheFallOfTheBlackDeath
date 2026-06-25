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

    [Header("Typewriter")]
    [Tooltip("Si está activo, el texto se revela letra a letra antes de animar.")]
    [SerializeField] private bool useTypewriter = true;
    [Tooltip("Segundos entre cada letra revelada.")]
    [SerializeField] private float charInterval = 0.04f;
    [Tooltip("Multiplicador de charInterval tras coma o punto y coma.")]
    [SerializeField] private float commaPauseMult = 3f;
    [Tooltip("Multiplicador de charInterval tras punto, ! o ?")]
    [SerializeField] private float periodPauseMult = 6f;
    [Tooltip("Escala del pop visual por cada letra nueva revelada.")]
    [SerializeField] private float revealPopScale = 0.8f;
 
    [Header("Duración")]
    [SerializeField] private float activeDuration = 1.5f; 
    private float timer;
    private Vector3 startPos;
    private Transform mainCamera;
    private bool isCrit;

    private bool inRevealPhase;
    private float revealTimer;
    private int revealedChars;
    private int totalChars;
    private float nextCharInterval;
 
    /// <summary>
    /// Initializes cached references and runtime state before the component starts running.
    /// </summary>
    void Awake()
    {
        if (Camera.main != null) mainCamera = Camera.main.transform;
    }
 
    /// <summary>
    /// Configura el texto flotante con mensaje, color y si es crítico.
    /// </summary>
    public void Initialize(string message, Color color, bool isCritical, bool useRandomOffset = true, float duration = 0f)
    {
        if (duration > 0f) activeDuration = duration;
        textMesh.text = message;
        textMesh.color = color;
        timer = 0;
        isCrit = isCritical;

        // Variación de Posición (Random offset)
        if (useRandomOffset)
        {
            startPos = transform.position + new Vector3(
                Random.Range(-randomOffset.x, randomOffset.x),
                Random.Range(-randomOffset.y, randomOffset.y), 
                0
            );
        }
        else
        {
            startPos = transform.position;
        }
        
        transform.position = startPos;
        
        // Lo empezamos en escala 0 para que la curva lo haga "explotar" hacia afuera
        transform.localScale = Vector3.zero; 

        if (useTypewriter && textMesh.text.Length > 0)
        {
            inRevealPhase = true;
            revealTimer = 0f;
            revealedChars = 0;
            totalChars = textMesh.text.Length;
            nextCharInterval = charInterval;
            textMesh.maxVisibleCharacters = 0;
            timer = 0f;
        }
        else
        {
            inRevealPhase = false;
            textMesh.maxVisibleCharacters = int.MaxValue;
        }
    }
 
    /// <summary>
    /// Updates the component each frame while it is active.
    /// </summary>
    void Update()
    {
        // Billboard
        if (mainCamera != null)
            transform.rotation = Quaternion.LookRotation(mainCamera.forward);

        // ── FASE REVEAL ──────────────────────────────────────────────────────
        if (inRevealPhase)
        {
            revealTimer += Time.deltaTime;

            if (revealTimer >= nextCharInterval)
            {
                revealTimer -= nextCharInterval;
                revealedChars++;
                textMesh.maxVisibleCharacters = revealedChars;

                // Calcular el intervalo para la PRÓXIMA letra
                // basado en el caracter que ACABA de revelarse
                if (revealedChars < totalChars)
                {
                    char justRevealed = textMesh.text[revealedChars - 1];
                    if (justRevealed == '.' || justRevealed == '!' || justRevealed == '?')
                        nextCharInterval = charInterval * periodPauseMult;
                    else if (justRevealed == ',' || justRevealed == ';' || justRevealed == ':')
                        nextCharInterval = charInterval * commaPauseMult;
                    else
                        nextCharInterval = charInterval;
                }

                // Pop visual por cada letra nueva
                transform.localScale = Vector3.one * revealPopScale * (isCrit ? criticalSize : normalSize);

                // Reveal completo
                if (revealedChars >= totalChars)
                {
                    inRevealPhase = false;
                    textMesh.maxVisibleCharacters = int.MaxValue;
                    timer = 0f;
                }
            }

            // Mantener alpha visible durante el reveal
            textMesh.color = new Color(
                textMesh.color.r,
                textMesh.color.g,
                textMesh.color.b,
                1f);

            return;
        }

        // ── FASE ANIMATE ─────────────────────────────────────────────────────
        timer += Time.deltaTime;
        float percent = timer / activeDuration;

        // 1. Escala
        float currentScaleEval = scaleCurve.Evaluate(percent);
        float finalScale = currentScaleEval * (isCrit ? criticalSize : normalSize);
        transform.localScale = Vector3.one * finalScale;

        // 2. Movimiento + jitter
        float currentSpeed = isCrit ? criticalSpeed : normalSpeed;
        Vector3 currentPos = startPos + Vector3.up * (currentSpeed * timer);

        if (isCrit)
        {
            float jitterX = Mathf.Sin(Time.time * jitterSpeed) * jitterAmount;
            float jitterY = Mathf.Cos(Time.time * jitterSpeed * 1.2f) * jitterAmount;
            currentPos += new Vector3(jitterX, jitterY, 0);
        }

        transform.position = currentPos;

        // 3. Fade
        float currentAlpha = alphaCurve.Evaluate(percent);
        textMesh.color = new Color(
            textMesh.color.r,
            textMesh.color.g,
            textMesh.color.b,
            currentAlpha);

        // 4. Retorno al pool
        if (timer >= activeDuration)
            FloatingTextManager.Instance.ReturnToPool(this);
    }
}