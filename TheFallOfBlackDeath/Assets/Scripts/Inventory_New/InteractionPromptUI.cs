using UnityEngine;
using TMPro;

/// <summary>
/// Singleton que gestiona la interfaz de usuario para los mensajes de interacción.
/// </summary>
public class InteractionPromptUI : MonoBehaviour
{
    public static InteractionPromptUI Instance { get; private set; }

    [SerializeField] private GameObject promptRoot; // El panel raíz a activar/desactivar
    [SerializeField] private TMP_Text promptText;    // El texto del mensaje

    private void Awake()
    {
        // Patrón singleton estándar
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Ocultar al inicializar
        Hide();
    }

    private Coroutine hideCoroutine;

    /// <summary>
    /// Asigna el texto y activa el panel de mensaje.
    /// </summary>
    /// <param name="message">Mensaje a mostrar.</param>
    public void Show(string message)
    {
        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
            hideCoroutine = null;
        }

        if (promptText != null)
        {
            promptText.text = message;
        }
        
        if (promptRoot != null)
        {
            promptRoot.SetActive(true);
        }
    }

    /// <summary>
    /// Muestra un mensaje y lo oculta automáticamente tras un tiempo.
    /// </summary>
    public void Show(string message, float duration)
    {
        Show(message);
        hideCoroutine = StartCoroutine(HideAfterDelay(duration));
    }

    private System.Collections.IEnumerator HideAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Hide();
        hideCoroutine = null;
    }

    /// <summary>
    /// Desactiva el panel de mensaje.
    /// </summary>
    public void Hide()
    {
        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
            hideCoroutine = null;
        }

        if (promptRoot != null)
        {
            promptRoot.SetActive(false);
        }
    }
}
