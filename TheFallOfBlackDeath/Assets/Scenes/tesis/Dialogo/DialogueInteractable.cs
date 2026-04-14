using UnityEngine;

/// <summary>
/// Supports branching dialogue flow by handling dialogue interactable.
/// </summary>
public class DialogueInteractable : MonoBehaviour
{
    public Dialogue dialogue;
    private Transform playerTransform;
    private bool canTalk;
    [SerializeField]
    private bool _canMove;
    /// <summary>
    /// Responds to the corresponding Unity trigger callback for this component.
    /// </summary>
    /// <param name="other">The other.</param>
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Charecter"))
        {
            Debug.Log("Presiona [E] para hablar");
            playerTransform = other.transform;
            canTalk = true;
        }
    }

    /// <summary>
    /// Responds to the corresponding Unity trigger callback for this component.
    /// </summary>
    /// <param name="other">The other.</param>
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Charecter"))
        {
            canTalk = false;
            playerTransform = null;
        }
    }

    /// <summary>
    /// Updates the component each frame while it is active.
    /// </summary>
    private void Update()
    {
        if (canTalk && Input.GetKeyDown(KeyCode.E))
        {
           
            if (DialogueManager.Instance.IsDialogueActive)
                return;

            if (_canMove == false)
            {
                DialogueManager.Instance.StartDialogue(dialogue, gameObject);
            }
            else
            {
                if (playerTransform != null)
                    LookAtPlayer();

                DialogueManager.Instance.StartDialogue(dialogue, gameObject);
            }
        }
    }


    /// <summary>
    /// Executes the look at player workflow.
    /// </summary>
    private void LookAtPlayer()
    {
        Vector3 direction = playerTransform.position - transform.position;
        direction.y = 0f; // evita que incline la cabeza
        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, 1f);
        }
    }
}
