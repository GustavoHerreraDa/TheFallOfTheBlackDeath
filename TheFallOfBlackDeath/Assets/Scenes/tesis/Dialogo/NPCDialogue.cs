using UnityEngine;

public class NPCDialogue : Interactable
{
    [Header("Diálogo del NPC")]
    public Dialogue dialogue;

    public override void Interact()
    {
        if (dialogue != null)
        {
            // Inicia el diálogo
            DialogueManager.Instance.StartDialogue(dialogue);
        }
        else
        {
            Debug.LogWarning("No hay diálogo asignado a este NPC.");
        }
    }
}
