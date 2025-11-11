using UnityEngine;

public class DialogueInput : MonoBehaviour
{
    void Update()
    {
        if (DialogueManager.Instance != null && Input.GetKeyDown(KeyCode.E))
        {
            DialogueManager.Instance.NextLine();
        }
    }
}