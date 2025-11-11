using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    public Dialogue dialogue;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Charecter"))
        {
            DialogueManager.Instance.StartDialogue(dialogue);
        }
    }
}
