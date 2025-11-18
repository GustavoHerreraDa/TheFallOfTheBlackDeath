using UnityEngine;

[System.Serializable]
public class DialogueLine
{
    public string speakerName;
    [TextArea(2, 5)]
    public string sentence;

    public bool hasChoices;
    public DialogueChoice[] choices = new DialogueChoice[2];
}