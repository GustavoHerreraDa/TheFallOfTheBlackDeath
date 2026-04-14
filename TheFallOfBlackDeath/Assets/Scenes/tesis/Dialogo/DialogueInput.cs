using UnityEngine;

/// <summary>
/// Supports branching dialogue flow by handling dialogue input.
/// </summary>
public class DialogueInput : MonoBehaviour
{
    /// <summary>
    /// Updates the component each frame while it is active.
    /// </summary>
    void Update()
    {
        if (DialogueManager.Instance != null && Input.GetKeyDown(KeyCode.E))
        {
            DialogueManager.Instance.NextLine();
        }
    }
}
