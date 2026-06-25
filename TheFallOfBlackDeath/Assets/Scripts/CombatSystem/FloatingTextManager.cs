using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Supports the combat system by handling floating text manager.
/// </summary>
public class FloatingTextManager : MonoBehaviour
{
    public static FloatingTextManager Instance;
    
    [Header("Configuración")]
    public GameObject floatingTextPrefab;
    public Transform container;
    public int initialPoolSize = 20;
    
    private Stack<FloatingText> textPool = new Stack<FloatingText>();

    /// <summary>
    /// Initializes cached references and runtime state before the component starts running.
    /// </summary>
    void Awake()
    {
        // Guard contra duplicados: si ya existe una instancia, nos destruimos.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        InitializePool();
    }

    /// <summary>
    /// Pre-instancia el pool inicial de FloatingTexts.
    /// </summary>
    void InitializePool()
    {
        for (int i = 0; i < initialPoolSize; i++)
        {
            // Instanciamos y pusheamos al pool directamente.
            textPool.Push(InstantiateText());
        }
    }

    /// <summary>
    /// Instancia un FloatingText desactivado. No lo agrega al pool —
    /// eso es responsabilidad del llamador.
    /// </summary>
    private FloatingText InstantiateText()
    {
        GameObject obj = Instantiate(floatingTextPrefab, container);
        obj.SetActive(false);
        return obj.GetComponent<FloatingText>();
    }

    /// <summary>
    /// Muestra un texto flotante en la posición indicada.
    /// Si el pool está vacío, instancia uno nuevo on-demand.
    /// </summary>
    /// <param name="message">Texto a mostrar.</param>
    /// <param name="position">Posición world-space donde aparece.</param>
    /// <param name="color">Color del texto.</param>
    /// <param name="isCritical">Si es true aplica escala y jitter de crítico.</param>
    /// <param name="randomizePosition">Si es true aplica un offset aleatorio.</param>
    /// <param name="duration">Duración del texto en pantalla (0 usa el default).</param>
    public void ShowText(string message, Vector3 position, Color color, bool isCritical = false, bool randomizePosition = true, float duration = 0f)
    {
        // Si el pool está vacío, creamos uno nuevo on-demand sin pushearlo
        // (ReturnToPool se encargará de devolverlo al pool al terminar).
        FloatingText txt = textPool.Count > 0
            ? textPool.Pop()
            : InstantiateText();

        txt.gameObject.SetActive(true);
        txt.transform.position = position;
        txt.Initialize(message, color, isCritical, randomizePosition, duration);
    }

    /// <summary>
    /// Devuelve un FloatingText al pool para su reutilización.
    /// </summary>
    public void ReturnToPool(FloatingText txt)
    {
        txt.gameObject.SetActive(false);
        textPool.Push(txt);
    }
}