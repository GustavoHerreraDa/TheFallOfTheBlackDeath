using UnityEngine;

/// <summary>
/// Clase que vive en el jugador y se encarga de delegar la interacción al objeto detectado.
/// Hereda de Interactable para aprovechar la detección de triggers y la UI centralizada.
/// </summary>
public class PlayerInteraction : Interactable
{
    /// <summary>
    /// Implementación de la acción de interactuar.
    /// Busca en el objeto detectado (objCollider) componentes específicos que sepan interactuar.
    /// </summary>
    public override void Interact()
    {
        if (objCollider == null) return;

        // Intentar delegar al objeto específico según su tipo
        var dialogue = objCollider.GetComponent<DialogueInteractable>();
        if (dialogue != null) { dialogue.Interact(); return; }

        var portal = objCollider.GetComponent<Portal>();
        if (portal != null) { portal.Interact(); return; }

        // Agregar más tipos acá si se necesitan en el futuro
    }
}
