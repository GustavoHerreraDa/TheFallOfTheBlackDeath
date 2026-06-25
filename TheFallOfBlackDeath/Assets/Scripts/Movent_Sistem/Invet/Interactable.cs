using System.Collections;
using UnityEngine;

/// <summary>
/// Clase base para objetos interactuables que maneja la lógica de triggers y la UI de prompts.
/// </summary>
public abstract class Interactable : MonoBehaviour, Assets.Scripts.Movent_Sistem.Invet.IInteractable
{
    [Header("Configuración Base de Interacción")]
    [SerializeField] protected string interactionPrompt = "[ E ] Interactuar";
    
    /// <summary>
    /// Propiedad de la interfaz IInteractable.
    /// </summary>
    public virtual string InteractionPrompt => interactionPrompt;

    /// <summary>
    /// Ejecuta la acción de interacción. Debe ser implementado por clases hijas.
    /// </summary>
    public abstract void Interact();

    /// <summary>
    /// Detecta cuando el jugador entra en el área de interacción.
    /// </summary>
    protected virtual void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Charecter"))
        {
            InteractionPromptUI.Instance?.Show(InteractionPrompt);
        }
    }

    /// <summary>
    /// Detecta cuando el jugador sale del área de interacción.
    /// </summary>
    protected virtual void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Charecter"))
        {
            InteractionPromptUI.Instance?.Hide();
        }
    }
}