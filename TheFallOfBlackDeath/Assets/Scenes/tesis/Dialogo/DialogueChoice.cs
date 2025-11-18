using UnityEngine;

[System.Serializable]
public class DialogueChoice
{
    public string playerText;
    public DialogueEvent.DialogueEndAction action = DialogueEvent.DialogueEndAction.None;
    public Dialogue nextDialogue;


    public string[] addFlags;
    public string[] removeFlags;
}
