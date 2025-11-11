using UnityEngine;

[CreateAssetMenu(fileName = "NewDialogue", menuName = "Dialogue/Conversation")]
public class Dialogue : ScriptableObject
{
    public DialogueLine[] lines;
}