using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    public Dialogue dialogue;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Character"))
        {
            DialogueManager.Instance.StartDialogue(dialogue);
        }
    }
}
