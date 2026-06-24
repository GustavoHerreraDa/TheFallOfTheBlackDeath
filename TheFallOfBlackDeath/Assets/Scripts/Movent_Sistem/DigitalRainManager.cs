using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Gestor principal del efecto "Lluvia Digital".
///
/// ARQUITECTURA DEL POOL:
///   poolSize columnas se instancian en Awake y se reutilizan indefinidamente.
///   Cuando una columna sale por abajo, se desactiva sola. El Manager la detecta
///   en Update y la recoloca + reactiva → cero Instantiate/Destroy en runtime.
///
/// FALSO PARALAJE (3 ejes):
///   Cada columna recibe una capa de profundidad aleatoria [0..depthLayers-1].
///   A menor layer (fondo): menor escala, menor velocidad, menor alpha.
///   A mayor layer (frente): mayor escala, mayor velocidad, mayor alpha.
///   SetSiblingIndex respeta el z-order para que el fondo se pinte debajo.
///
/// STAGGERING (anti-spike):
///   textUpdateInterval se asigna con un valor aleatorio por columna para que
///   los ForceMeshUpdate no coincidan en el mismo frame.
/// </summary>
public class DigitalRainManager : MonoBehaviour
{
    // ── Referencias ───────────────────────────────────────────────────────────
    [Header("Referencias")]
    [Tooltip("Prefab con RectTransform + TextMeshProUGUI + DigitalRainColumn")]
    public GameObject columnPrefab;

    [Tooltip("RectTransform del panel que contiene las columnas (DigitalRainRoot)")]
    public RectTransform canvasRect;

    // ── Caracteres ────────────────────────────────────────────────────────────
    [Header("Caracteres")]
    [Tooltip("Kana de medio ancho para look Matrix / hex para look industrial / ASCII para 1-bit")]
    public string characterSet = "ｦｧｨｩｪｫｬｭｮｯｰｱｲｳｴｵｶｷｸｹｺｻｼｽｾｿﾀﾁﾂﾃﾄﾅﾆﾇﾈﾉﾊﾋﾌﾍﾎﾏﾐﾑﾒﾓﾔﾕﾖﾗﾘﾙﾚﾛﾜﾝ0123456789ABCDEF";

    // ── Pool / Densidad ───────────────────────────────────────────────────────
    [Header("Pool / Densidad")]
    [Tooltip("Columnas totales en el pool. 25-35 es suficiente con staggering.")]
    public int poolSize = 30;

    [Tooltip("Separación horizontal en px entre columnas.")]
    public float columnSpacing = 20f;

    // ── Apariencia ────────────────────────────────────────────────────────────
    [Header("Apariencia")]
    [Tooltip("Color de la cabeza (carácter más bajo). Blanco puro para look clásico.")]
    public Color headColor = Color.white;

    [Tooltip("Color base de la cola. Verde fósforo = (0,1,0.3,1) / Ámbar = (1,0.6,0.1,1)")]
    public Color tailColor = new Color(0f, 1f, 0.3f, 1f);

    [Tooltip("Tamaño de fuente base en px.")]
    public float baseFontSize = 16f;

    [Tooltip("Cantidad de caracteres visibles por columna.")]
    [Range(5, 40)]
    public int columnLength = 20;

    // ── Movimiento ────────────────────────────────────────────────────────────
    [Header("Movimiento")]
    [Tooltip("Velocidad de caída mínima en px/seg (columnas del fondo).")]
    public float minFallSpeed = 80f;

    [Tooltip("Velocidad de caída máxima en px/seg (columnas del frente).")]
    public float maxFallSpeed = 220f;

    [Tooltip("Segundos entre mutaciones de cola (base). Cada columna recibe variación aleatoria.")]
    public float mutationRate = 0.1f;

    // ── Optimización ──────────────────────────────────────────────────────────
    [Header("Optimización")]
    [Tooltip("Rango mínimo del intervalo de rebuild de texto (segundos). Mayor = menos costo.")]
    public float minTextUpdateInterval = 0.04f;

    [Tooltip("Rango máximo del intervalo de rebuild de texto (segundos).")]
    public float maxTextUpdateInterval = 0.08f;

    // ── Paralaje ──────────────────────────────────────────────────────────────
    [Header("Paralaje (Profundidad)")]
    [Range(1, 5)]
    [Tooltip("Número de capas de profundidad. 3 da un buen equilibrio visual/costo.")]
    public int depthLayers = 3;

    [Tooltip("Escala mínima (columnas del fondo, se ven más pequeñas/lejanas).")]
    public float minScale = 0.5f;

    [Tooltip("Escala máxima (columnas del frente).")]
    public float maxScale = 1.0f;

    [Range(0f, 1f)]
    [Tooltip("Alpha máximo de columnas del fondo.")]
    public float minMaxAlpha = 0.3f;

    [Range(0f, 1f)]
    [Tooltip("Alpha máximo de columnas del frente.")]
    public float maxMaxAlpha = 0.9f;

    // ── Estado interno ────────────────────────────────────────────────────────
    private List<DigitalRainColumn> _pool = new List<DigitalRainColumn>();
    private float _screenW;
    private float _screenH;

    // ─────────────────────────────────────────────────────────────────────────
    private void Awake()
    {
        _screenW = canvasRect.rect.width;
        _screenH = canvasRect.rect.height;
        BuildPool();
    }

    private void Start()
    {
        // Activación inicial con Y aleatoria para que el efecto no arranque vacío
        foreach (var col in _pool)
            ActivateColumn(col, randomStartY: true);
    }

    private void Update()
    {
        // Recicla columnas inactivas
        // Nota: no usamos foreach con índice para evitar allocations de enumerador
        for (int i = 0; i < _pool.Count; i++)
        {
            if (!_pool[i].gameObject.activeSelf)
                ActivateColumn(_pool[i], randomStartY: false);
        }
    }

    // ── Pool ──────────────────────────────────────────────────────────────────
    private void BuildPool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject go = Instantiate(columnPrefab, canvasRect);
            go.SetActive(false);
            _pool.Add(go.GetComponent<DigitalRainColumn>());
        }
    }

    /// <summary>
    /// Configura y activa una columna del pool.
    /// randomStartY=true: distribución inicial dentro de pantalla (arranque).
    /// randomStartY=false: siempre empieza desde arriba (reciclado normal).
    /// </summary>
    private void ActivateColumn(DigitalRainColumn col, bool randomStartY)
    {
        // ── 1. Capa de profundidad ────────────────────────────────────────────
        int   layer    = Random.Range(0, depthLayers);
        float depthT   = (depthLayers > 1) ? (float)layer / (depthLayers - 1) : 1f;

        float scale    = Mathf.Lerp(minScale, maxScale, depthT);
        float speed    = Mathf.Lerp(minFallSpeed, maxFallSpeed, depthT);
        float maxAlpha = Mathf.Lerp(minMaxAlpha, maxMaxAlpha, depthT);

        // Variación intra-capa para que no se vean clonadas
        speed    *= Random.Range(0.85f, 1.15f);
        maxAlpha  = Mathf.Clamp01(maxAlpha * Random.Range(0.85f, 1.05f));

        // ── 2. Posición X ─────────────────────────────────────────────────────
        int   slots = Mathf.Max(1, Mathf.FloorToInt(_screenW / columnSpacing));
        int   slot  = Random.Range(0, slots);
        float xPos  = -_screenW * 0.5f
                    + slot * columnSpacing
                    + Random.Range(-columnSpacing * 0.3f, columnSpacing * 0.3f);

        // ── 3. Posición Y ─────────────────────────────────────────────────────
        var   rt   = col.transform as RectTransform;
        float yPos = randomStartY
            ? Random.Range(-_screenH * 0.5f, _screenH * 0.5f)
            : _screenH * 0.5f + columnLength * baseFontSize * scale;

        if (rt != null)
        {
            rt.anchoredPosition = new Vector2(xPos, yPos);
            rt.localScale       = new Vector3(scale, scale, 1f);
        }

        // ── 4. Parámetros de la columna ───────────────────────────────────────
        col.characterSet      = characterSet;
        col.fallSpeed         = speed;
        col.mutationRate      = mutationRate * Random.Range(0.7f, 1.5f);
        col.columnLength      = columnLength + Random.Range(-3, 4);
        col.headColor         = headColor;
        col.tailColor         = tailColor;
        col.maxAlpha          = maxAlpha;
        col.screenHeight      = _screenH;
        col.fontSize          = baseFontSize;

        // ── 5. Staggering: intervalo de rebuild aleatorio por columna ─────────
        // Distribuye los ForceMeshUpdate en el tiempo para evitar spikes.
        col.textUpdateInterval = Random.Range(minTextUpdateInterval, maxTextUpdateInterval);

        col.Activate();

        // ── 6. Z-order: fondo atrás, frente adelante ──────────────────────────
        int siblingIdx = Mathf.Clamp(
            layer * Mathf.Max(1, canvasRect.childCount / Mathf.Max(1, depthLayers)),
            0,
            canvasRect.childCount - 1
        );
        col.transform.SetSiblingIndex(siblingIdx);
    }
}