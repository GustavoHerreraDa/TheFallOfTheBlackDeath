using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Supports inventory and interaction flow by handling portal.
/// </summary>
public class Portal : MonoBehaviour, Assets.Scripts.Movent_Sistem.Invet.IInteractable
{
    [SerializeField] private string interactionPrompt = "[ E ] Usar portal";
    /// <summary>
    /// Mensaje de interacción para la interfaz IInteractable.
    /// </summary>
    public string InteractionPrompt => interactionPrompt;

    /// <summary>
    /// Ejecuta la acción de usar el portal.
    /// </summary>
    public void Interact()
    {
        Debug.Log("Usando portal hacia otro mundo: " + gotoWorld);
        // Aquí iría la lógica para cambiar de escena o teletransportar
    }

    // Start is called before the first frame update
    public bool gotoWorld;
    /// <summary>
    /// Initializes the component once the scene dependencies are ready.
    /// </summary>
    void Start()
    {
        
    }

    // Update is called once per frame
    /// <summary>
    /// Updates the component each frame while it is active.
    /// </summary>
    void Update()
    {
        
    }
}
