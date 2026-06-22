using UnityEngine;

[CreateAssetMenu(fileName = "NewDialogue", menuName = "Dialogue/Conversation")]
/// <summary>
/// Stores the ordered dialogue lines that define a conversation asset.
/// </summary>
public class Dialogue : ScriptableObject
{
    public DialogueLine[] lines;
    [Tooltip("El ID único para este diálogo. Se usa para rastrear si ya ha sido leído.")]
    public string dialogueId;

    public string Id => string.IsNullOrEmpty(dialogueId) ? name : dialogueId;
}
