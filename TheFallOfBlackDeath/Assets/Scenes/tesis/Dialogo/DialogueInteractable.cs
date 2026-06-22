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

    [Header("Configuración de Interacción")]
    [Tooltip("Si es verdadero, el componente se desactivará y no se podrá volver a hablar con el NPC.")]
    public bool disableAfterTalking = true;
    [Tooltip("Si es verdadero, el NPC solo hablará si tiene algo nuevo que decir (basado en condiciones de líneas).")]
    public bool onlyTalkIfNewContent = true;
    [Tooltip("Si no tiene nada nuevo que decir, se mostrará este mensaje rápido (opcional).")]
    public string noContentMessage = "No tengo nada más que decirte por ahora.";

    /// <summary>
    /// Responds to the corresponding Unity trigger callback for this component.
    /// </summary>
    /// <param name="other">The other.</param>
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Charecter") || !this.enabled) return;

        // Solo el líder del party activa la interacción
        bool isLeader = GameManager.Instance == null ||
                        GameManager.Instance.GetLeader() == null ||
                        other.GetComponentInParent<PlayerFighter>() == GameManager.Instance.GetLeader();

        if (!isLeader) return;

        playerTransform = other.transform;
        canTalk = true;
        Debug.Log($"[DialogueInteractable] Presiona [E] para hablar con {gameObject.name}");
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
    /// Inicia la interacción de diálogo.
    /// </summary>
    public void Interact()
    {
        // ---> BLOQUEO DE SEGURIDAD <---
        // Previene que otros scripts (como PlayerControl) fuercen la interacción si este script ya está desactivado
        if (!this.enabled) return; 

        if (DialogueManager.Instance.IsDialogueActive)
            return;

        // VERIFICACIÓN DE CONTENIDO NUEVO (PRO)
        if (onlyTalkIfNewContent && !DialogueManager.Instance.HasAvailableContent(dialogue))
        {
            if (!string.IsNullOrEmpty(noContentMessage))
            {
                Debug.Log($"[DialogueInteractable] {gameObject.name}: {noContentMessage}");
                // Aquí podrías mostrar un pequeño popup flotante en lugar de un diálogo completo
                InteractionPromptUI.Instance?.Show(noContentMessage, 2f);
            }
            return;
        }

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

        // ---> DESACTIVACIÓN TOTAL <---
        if (disableAfterTalking)
        {
            canTalk = false;
            this.enabled = false; 
            
            // Opcional: Apagamos el Collider (si actúa como Trigger) para que el jugador ni siquiera choque o detecte al NPC al acercarse
            Collider col = GetComponent<Collider>();
            if (col != null && col.isTrigger) 
            {
                col.enabled = false;
            }
        }
    }

    /// <summary>
    /// Updates the component each frame while it is active.
    /// </summary>
    private void Update()
    {
        if (canTalk && Input.GetKeyDown(KeyCode.E))
        {
            Interact();
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