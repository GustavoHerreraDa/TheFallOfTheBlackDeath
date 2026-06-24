using TMPro;
using UnityEngine;

/// <summary>
/// Controla una columna individual del efecto Matrix.
///
/// LÓGICA DE VERTEX COLORS EN TMP:
///   TextMeshPro expone el mesh mediante textInfo.meshInfo[0].colors32.
///   Cada carácter ocupa 4 vértices (quad BL/TL/TR/BR), por lo que el
///   índice de vértice para el carácter i es: vertexIndex = i * 4.
///   Manipulando colors32 directamente y llamando UpdateVertexData(Colors32)
///   mantenemos 1 draw call por columna sin materiales extra.
///
/// OPTIMIZACIÓN DE UPDATE:
///   La caída (anchoredPosition) ocurre cada frame → movimiento fluido.
///   El rebuild de texto (ForceMeshUpdate) ocurre solo cada textUpdateInterval
///   segundos, y solo cuando algún char cambió → drástica reducción de costo.
///   El staggering (offset inicial aleatorio) distribuye los rebuilds entre
///   columnas para evitar spikes en un mismo frame.
/// </summary>
[RequireComponent(typeof(TextMeshProUGUI))]
public class DigitalRainColumn : MonoBehaviour
{
    // ── Referencia TMP ────────────────────────────────────────────────────────
    private TextMeshProUGUI _tmp;

    // ── Config seteada por DigitalRainManager ─────────────────────────────────
    [HideInInspector] public string characterSet;
    [HideInInspector] public float  fallSpeed;
    [HideInInspector] public float  mutationRate;
    [HideInInspector] public int    columnLength;
    [HideInInspector] public Color  headColor;
    [HideInInspector] public Color  tailColor;
    [HideInInspector] public float  maxAlpha;
    [HideInInspector] public float  screenHeight;
    [HideInInspector] public float  fontSize;

    /// <summary>
    /// Intervalo en segundos entre rebuilds de texto. Seteado por el Manager
    /// con un valor aleatorio por columna para distribuir el costo (staggering).
    /// </summary>
    [HideInInspector] public float textUpdateInterval = 0.05f; // ~20 fps de mutación

    // ── Estado interno ────────────────────────────────────────────────────────
    private char[]  _chars;
    private float   _currentY;
    private float   _mutationTimer;
    private float   _textUpdateTimer;
    private float   _headGlitchTimer;
    private bool    _active;
    private bool    _charsChanged;  // flag: ¿hubo cambio de chars en este tick?

    // La cabeza glitchea ~25 veces por segundo independientemente del textUpdateInterval
    private const float HEAD_GLITCH_RATE = 0.04f;

    // StringBuilder reutilizable para evitar allocations en BuildColumnString
    private System.Text.StringBuilder _sb;

    // ─────────────────────────────────────────────────────────────────────────
    private void Awake()
    {
        _tmp = GetComponent<TextMeshProUGUI>();
        _tmp.enableVertexGradient = false;
        _sb = new System.Text.StringBuilder(80);
    }

    /// <summary>
    /// Reinicia y activa la columna. Llamado desde el pool en DigitalRainManager.
    /// </summary>
    public void Activate()
    {
        // Inicializa chars
        if (_chars == null || _chars.Length != columnLength)
            _chars = new char[columnLength];

        for (int i = 0; i < columnLength; i++)
            _chars[i] = RandomChar();

        // Posición inicial: fuera de pantalla por arriba
        // (pivot top-center → Y positivo = arriba)
        _currentY = (screenHeight * 0.5f) + (columnLength * fontSize);

        var rt = transform as RectTransform;
        if (rt != null)
            rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, _currentY);

        _tmp.fontSize = fontSize;

        // ── Staggering: offset inicial aleatorio para distribuir rebuilds ─────
        // Sin esto, todas las columnas activadas en el mismo frame hacen
        // ForceMeshUpdate el mismo frame → spike de CPU.
        _textUpdateTimer = Random.Range(0f, textUpdateInterval);
        _mutationTimer   = mutationRate;
        _headGlitchTimer = HEAD_GLITCH_RATE;
        _charsChanged    = true;  // fuerza rebuild en el primer tick
        _active          = true;

        gameObject.SetActive(true);

        // Primer build del mesh antes de empezar a actualizar colores
        _tmp.SetText(BuildColumnString());
        _tmp.ForceMeshUpdate();
        UpdateColors();
    }

    // ─────────────────────────────────────────────────────────────────────────
    private void Update()
    {
        if (!_active) return;

        float dt = Time.deltaTime;

        // ── 1. CAÍDA: cada frame para movimiento suave ────────────────────────
        _currentY -= fallSpeed * dt;
        var rt = transform as RectTransform;
        if (rt != null)
            rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, _currentY);

        // ── 2. TICK DE TEXTO: solo cada textUpdateInterval ────────────────────
        _textUpdateTimer -= dt;
        if (_textUpdateTimer <= 0f)
        {
            _textUpdateTimer = textUpdateInterval;
            _charsChanged    = false;

            // Glitch de la cabeza (índice columnLength-1 = último char = cabeza)
            _headGlitchTimer -= textUpdateInterval;
            if (_headGlitchTimer <= 0f)
            {
                _chars[columnLength - 1] = RandomChar();
                _headGlitchTimer = HEAD_GLITCH_RATE;
                _charsChanged = true;
            }

            // Mutación aleatoria de la cola
            _mutationTimer -= textUpdateInterval;
            if (_mutationTimer <= 0f)
            {
                int idx = Random.Range(0, columnLength - 1);
                _chars[idx] = RandomChar();
                _mutationTimer = mutationRate + Random.Range(-mutationRate * 0.3f, mutationRate * 0.3f);
                _charsChanged = true;
            }

            // ForceMeshUpdate solo si los chars cambiaron (rebuild de geometría)
            // UpdateVertexData(Colors32) siempre (el gradiente no cambia los chars)
            if (_charsChanged)
            {
                _tmp.SetText(BuildColumnString()); // SetText es más barato que .text =
                _tmp.ForceMeshUpdate();
            }

            UpdateColors();
        }

        // ── 3. RECICLADO: columna salió por debajo de pantalla ────────────────
        if (_currentY < -(screenHeight * 0.5f) - fontSize)
        {
            gameObject.SetActive(false);
            _active = false;
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    /// <summary>
    /// Aplica el gradiente cola→cabeza directamente sobre colors32 del mesh TMP.
    ///
    /// MATEMÁTICA DEL GRADIENTE:
    ///   Para el carácter i (0 = cola superior, columnLength-1 = cabeza):
    ///     t     = i / (columnLength - 1)           → [0..1] normalizado
    ///     color = Lerp(tailColor, headColor, t)     → verde en cola, blanco en cabeza
    ///     alpha = SmoothStep(0, maxAlpha, t)        → transparente en cola, opaco en cabeza
    ///
    ///   SmoothStep(a,b,t) = a + (b-a) * t²(3-2t) → curva S, derivada=0 en extremos,
    ///   perceptualmente más suave que Lerp lineal.
    ///
    ///   Cada char = 4 vértices → vertexIndex = charIndex * 4
    ///   Solo actualizamos Colors32, no geometría → llamada mínima y barata.
    /// </summary>
    private void UpdateColors()
    {
        TMP_TextInfo textInfo = _tmp.textInfo;
        if (textInfo == null || textInfo.characterCount == 0) return;

        Color32[] colors   = textInfo.meshInfo[0].colors32;
        int       charCount = Mathf.Min(textInfo.characterCount, columnLength);

        for (int i = 0; i < charCount; i++)
        {
            float t = (columnLength > 1) ? (float)i / (columnLength - 1) : 1f;

            Color c = Color.Lerp(tailColor, headColor, t);
            c.a = Mathf.SmoothStep(0f, maxAlpha, t);

            Color32 c32  = c;
            int     vIdx = i * 4;
            if (vIdx + 3 >= colors.Length) break;

            colors[vIdx]     = c32; // BL
            colors[vIdx + 1] = c32; // TL
            colors[vIdx + 2] = c32; // TR
            colors[vIdx + 3] = c32; // BR
        }

        // Solo actualiza el buffer de colores, sin tocar geometría ni layout
        _tmp.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
    }

    // ─────────────────────────────────────────────────────────────────────────
    /// <summary>
    /// Construye el string multilinea de la columna reutilizando el StringBuilder
    /// para evitar allocations de GC por frame.
    /// _chars[0]              = cola superior (transparente)
    /// _chars[columnLength-1] = cabeza (blanco brillante)
    /// </summary>
    private string BuildColumnString()
    {
        _sb.Clear();
        for (int i = 0; i < columnLength; i++)
        {
            _sb.Append(_chars[i]);
            if (i < columnLength - 1) _sb.Append('\n');
        }
        return _sb.ToString();
    }

    private char RandomChar()
    {
        if (string.IsNullOrEmpty(characterSet)) return '?';
        return characterSet[Random.Range(0, characterSet.Length)];
    }
}