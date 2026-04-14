using UnityEngine;

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

    public bool hasChoices;
    public DialogueChoice[] choices = new DialogueChoice[2];
}
