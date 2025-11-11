using UnityEngine;

public class DialogueInteractable : MonoBehaviour
{
    public Dialogue dialogue;
    private Transform playerTransform;
    private bool canTalk;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Charecter"))
        {
            Debug.Log("Presiona [E] para hablar");
            playerTransform = other.transform;
            canTalk = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Charecter"))
        {
            canTalk = false;
            playerTransform = null;
        }
    }

    private void Update()
    {
        if (canTalk && Input.GetKeyDown(KeyCode.E))
        {
            // 🔁 Que el NPC mire hacia el jugador
            if (playerTransform != null)
                LookAtPlayer();

            // 🗨️ Inicia el diálogo y le pasa este NPC al manager
            DialogueManager.Instance.StartDialogue(dialogue, gameObject);
        }
    }

    private void LookAtPlayer()
    {
        Vector3 direction = playerTransform.position - transform.position;
        direction.y = 0f; // evita que incline la cabeza
        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, 1f); // 1f = giro instantáneo
        }
    }
}
