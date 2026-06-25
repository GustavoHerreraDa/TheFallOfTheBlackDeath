using System;

namespace Assets.Scripts.Movent_Sistem.Invet
{
    /// <summary>
    /// Interfaz para objetos que pueden ser interactuados por el jugador.
    /// Define el contrato para ejecutar la interacción y obtener el mensaje de la UI.
    /// </summary>
    public interface IInteractable
    {
        /// <summary>
        /// Mensaje que se mostrará en la UI cuando el jugador esté cerca.
        /// </summary>
        string InteractionPrompt { get; }

        /// <summary>
        /// Ejecuta la lógica de interacción.
        /// </summary>
        void Interact();
    }
}
