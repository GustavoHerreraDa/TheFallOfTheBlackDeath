using UnityEngine;
using Assets.Scripts.Movent_Sistem.Invet;

/// <summary>
/// Gestiona la detección de objetos interactuables y dispara la interacción.
/// Vive en el objeto del jugador.
/// </summary>
public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] private float detectionRadius = 2f;
    [SerializeField] private LayerMask interactableLayer;

    private IInteractable currentInteractable;

    /// <summary>
    /// Entrada pública para invocar la interacción.
    /// Puede ser llamada por un sistema de inputs (ej. OnInteractButtonPressed).
    /// </summary>
    public void OnInteractButtonPressed()
    {
        if (DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueActive)
        {
            DialogueManager.Instance.OnInteractInputPressed();
            return;
        }

        if (currentInteractable != null)
        {
            // Ocultar el prompt inmediatamente al interactuar
            InteractionPromptUI.Instance?.Hide();
            currentInteractable.Interact();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        IInteractable interactable = other.GetComponent<IInteractable>();
        if (interactable != null)
        {
            currentInteractable = interactable;
            InteractionPromptUI.Instance?.Show(currentInteractable.InteractionPrompt);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        IInteractable interactable = other.GetComponent<IInteractable>();
        if (interactable != null && currentInteractable == interactable)
        {
            currentInteractable = null;
            InteractionPromptUI.Instance?.Hide();
        }
    }
}
