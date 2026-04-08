using UnityEngine;

[System.Serializable]
public class DialogueLine
{
    public string speakerName;
    [TextArea(2, 5)]
    public string sentence;

    [Header("Condiciones de Aparición")]
    public string requiredFlag; // El diálogo solo aparece si el jugador tiene este flag
    public string forbiddenFlag; // El diálogo desaparece si el jugador tiene este flag

    public bool hasChoices;
    public DialogueChoice[] choices = new DialogueChoice[2];
}