using UnityEngine;

[CreateAssetMenu(fileName = "NewDialogue", menuName = "Dialogue/Conversation")]
/// <summary>
/// Stores the ordered dialogue lines that define a conversation asset.
/// </summary>
public class Dialogue : ScriptableObject
{
    public DialogueLine[] lines;
}
