using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
/// <summary>
/// Represents a single dialogue line, including speaker data, branching metadata, and conditional flags.
/// </summary>
public class DialogueLine
{
    public string speakerName;
    [TextArea(2, 5)]
    public string sentence;

    [Header("Condiciones de ApariciÃ³n")]
    public string requiredFlag; // El diÃ¡logo solo aparece si el jugador tiene este flag
    public string forbiddenFlag; // El diÃ¡logo desaparece si el jugador tiene este flag
    public GlobalFlag requiredFlagSO;
    public GlobalFlag forbiddenFlagSO;

    [Header("Condición de Ítem (opcional)")]
    [Tooltip("El ID del ítem que el jugador debe tener para que esta línea aparezca. Dejar vacío = sin condición.")]
    public string requiredItemId;
    [Tooltip("Cantidad mínima requerida del ítem anterior.")]
    public int requiredItemAmount = 1;

    public bool hasChoices;
    public List<DialogueChoice> choices = new List<DialogueChoice>();
}
