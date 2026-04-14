using UnityEngine;

/// <summary>
/// Supports branching dialogue flow by handling dialogue trigger.
/// </summary>
public class DialogueTrigger : MonoBehaviour
{
    public Dialogue dialogue;

    /// <summary>
    /// Responds to the corresponding Unity trigger callback for this component.
    /// </summary>
    /// <param name="other">The other.</param>
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Charecter"))
        {
            DialogueManager.Instance.StartDialogue(dialogue);
        }
    }
}
