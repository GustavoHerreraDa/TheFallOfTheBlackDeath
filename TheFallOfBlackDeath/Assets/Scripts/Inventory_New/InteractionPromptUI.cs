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

    /// <summary>
    /// Asigna el texto y activa el panel de mensaje.
    /// </summary>
    /// <param name="message">Mensaje a mostrar.</param>
    public void Show(string message)
    {
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
    /// Desactiva el panel de mensaje.
    /// </summary>
    public void Hide()
    {
        if (promptRoot != null)
        {
            promptRoot.SetActive(false);
        }
    }
}
