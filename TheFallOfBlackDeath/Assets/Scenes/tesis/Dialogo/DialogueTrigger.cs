using UnityEngine;

/// <summary>
/// Supports branching dialogue flow by handling dialogue trigger.
/// </summary>
public class DialogueTrigger : MonoBehaviour
{
    public Dialogue dialogue;
    public bool oneShot = true;
    private bool triggered;

    /// <summary>
    /// Responds to the corresponding Unity trigger callback for this component.
    /// </summary>
    /// <param name="other">The other.</param>
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Charecter")) return;
        if (oneShot && triggered) return;
        triggered = true;
        DialogueManager.Instance.StartDialogue(dialogue);
    }
}
