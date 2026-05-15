using UnityEngine;

/// <summary>
/// Controls dialogue progression, player input locking, branching choices, and gameplay events triggered from conversations.
/// </summary>
public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;
    private Dialogue currentDialogue;
    private int currentLineIndex;
    private DialogueUI ui;
    public delegate void GiveItemHandler(int id, int amount, InventoryDateBase.Uso type);
    public static event GiveItemHandler OnGiveItem;
    
    [Header("Player")]
    public PlayerControl playerControl;

    // Cache del Rigidbody del jugador para restaurar su estado tras el diálogo
    private Rigidbody cachedPlayerRb;
    private bool cachedUseGravity;
    private RigidbodyConstraints cachedConstraints;

    private GameObject currentNPC;
    public delegate void RecruitEventHandler(GameObject npc, int fighterIndex);
    public static event RecruitEventHandler OnRecruitCharacter;
    public bool IsDialogueActive => currentDialogue != null;
    /// <summary>
    /// Initializes cached references and runtime state before the component starts running.
    /// </summary>
    private void Awake()
    {
        if (Instance == null) Instance = this;
        ui = FindObjectOfType<DialogueUI>();
    }

    /// <summary>
    /// Starts a dialogue sequence, shows the dialogue UI, and temporarily disables player control.
    /// </summary>
    /// <param name="dialogue">The dialogue.</param>
    /// <param name="npc">The npc.</param>
    public void StartDialogue(Dialogue dialogue, GameObject npc = null)
    {
        currentDialogue = dialogue;
        currentNPC = npc;
        currentLineIndex = 0;

        ui.ShowUI(true);

        if (playerControl != null)
        {
            playerControl.enabled = false;
            playerControl.anim.SetFloat("Movent", 0f);
            // Intentamos obtener el rigidbody (en el root o en hijos) para congelar al jugador
            cachedPlayerRb = playerControl.GetComponent<Rigidbody>();
            if (cachedPlayerRb == null)
                cachedPlayerRb = playerControl.GetComponentInChildren<Rigidbody>();

            if (cachedPlayerRb != null)
            {
                // Guardamos estado previo para restaurarlo al finalizar
                cachedUseGravity = cachedPlayerRb.useGravity;
                cachedConstraints = cachedPlayerRb.constraints;

                cachedPlayerRb.linearVelocity = Vector3.zero;
                cachedPlayerRb.angularVelocity = Vector3.zero;
                cachedPlayerRb.useGravity = false;
                cachedPlayerRb.constraints = RigidbodyConstraints.FreezeAll; 
            }


        }

        ShowLine();
    }

    /// <summary>
    /// Advances the current dialogue, or skips the typing effect if the active line is still animating.
    /// </summary>
    public void NextLine()
    {
        Debug.Log("NEXT LINE CALLED");

        if (ui.IsTyping)
        {
            Debug.Log("Skipping typing");
            ui.SkipTyping();
            return;
        }

        currentLineIndex++;
        Debug.Log("Current index now: " + currentLineIndex);

        if (currentLineIndex < currentDialogue.lines.Length)
            ShowLine();
        else
            EndDialogue();
    }


    /// <summary>
    /// Shows the line.
    /// </summary>
    private void ShowLine()
    {
        DialogueLine line = currentDialogue.lines[currentLineIndex];

        // VERIFICACIÓN DE FLAGS PARA LA LÍNEA
        if (!string.IsNullOrEmpty(line.requiredFlag) && !GlobalState.Instance.HasFlag(line.requiredFlag))
        {
            SkipLine();
            return;
        }

        if (!string.IsNullOrEmpty(line.forbiddenFlag) && GlobalState.Instance.HasFlag(line.forbiddenFlag))
        {
            SkipLine();
            return;
        }

        ui.HideChoices();
        ui.DisplayLine(line);
        ui.onTypingFinished = () =>
        {
            if (line.hasChoices)
                ui.ShowChoices(line.choices);
        };
    }

    /// <summary>
    /// Executes the skip line workflow.
    /// </summary>
    private void SkipLine()
    {
        currentLineIndex++;
        if (currentLineIndex < currentDialogue.lines.Length)
            ShowLine();
        else
            EndDialogue();
    }


    /// <summary>
    /// Ends the dialogue.
    /// </summary>
    private void EndDialogue()
    {
        ui.ShowUI(false);
        currentDialogue = null;

        if (playerControl != null)
            playerControl.enabled = true;
        RestorePlayerRigidbody();
       
        if (currentNPC != null)
        {
            DialogueEvent evt = currentNPC.GetComponent<DialogueEvent>();
            if (evt != null)
                evt.TriggerEvent();
        }

        currentNPC = null;
    }


    /// <summary>
    /// Resolves the selected dialogue choice, including flags, items, recruitment, and dialogue branching.
    /// </summary>
    /// <param name="choice">The choice.</param>
    public void SelectChoice(DialogueChoice choice)
    {
        ui.HideChoices();

        // Flags logic...
        if (choice.addFlags != null)
            foreach (var f in choice.addFlags) GlobalState.Instance.AddFlag(f);
        if (choice.removeFlags != null)
            foreach (var f in choice.removeFlags) GlobalState.Instance.RemoveFlag(f);

        // --- LÓGICA DE DAR ITEM ---
        if (choice.action == DialogueEvent.DialogueEndAction.GiveItem)
        {
            // Disparamos el evento con los datos del ScriptableObject
            Debug.Log($"Dialogo: Regalando item ID {choice.itemID}");
            OnGiveItem?.Invoke(choice.itemID, choice.itemAmount, choice.itemType);
            
            // Si hay un diálogo siguiente, vamos a él en lugar de cerrar
            if (choice.nextDialogue != null)
            {
                StartDialogue(choice.nextDialogue, currentNPC);
                return;
            }

            // Si no hay siguiente diálogo, cerramos
            EndDialogueWithAction(choice.action);
            return;
        }
        // ---------------------------

        if (choice.action != DialogueEvent.DialogueEndAction.None)
        {
            EndDialogueWithAction(choice.action);
            return;
        }

        if (choice.nextDialogue != null)
        {
            StartDialogue(choice.nextDialogue, currentNPC);
            return;
        }

        NextLine();
    }

    /// <summary>
    /// Closes the dialogue UI and executes the gameplay action associated with the final dialogue choice.
    /// </summary>
    /// <param name="action">The action.</param>
    private void EndDialogueWithAction(DialogueEvent.DialogueEndAction action)
    {
        ui.ShowUI(false);
        // Marcar diálogo como finalizado para permitir hablar con otros NPC
        currentDialogue = null;
        if (playerControl != null) playerControl.enabled = true;
        RestorePlayerRigidbody();

        if (currentNPC != null)
        {
            if (action == DialogueEvent.DialogueEndAction.RecruitCharacter)
            {
                // BUSCAMOS EL ID ANTES DE DISPARAR
                PlayerFighter npcData = currentNPC.GetComponent<PlayerFighter>();
            
                if (npcData != null)
                {
                    // Disparamos el evento pasando el objeto Y su índice de base de datos
                    OnRecruitCharacter?.Invoke(currentNPC, npcData.figherIndex);
                }
                else
                {
                    Debug.LogError($"El NPC {currentNPC.name} no tiene componente PlayerFighter. No se puede obtener su ID.");
                }
            }

            DialogueEvent evt = currentNPC.GetComponent<DialogueEvent>();
            evt?.TriggerEvent();
        }
        currentNPC = null;
    }

    /// <summary>
    /// Restaura el estado del rigidbody del jugador si fue modificado al iniciar el diálogo.
    /// </summary>
    private void RestorePlayerRigidbody()
    {
        if (cachedPlayerRb != null)
        {
            cachedPlayerRb.useGravity = cachedUseGravity;
            cachedPlayerRb.constraints = cachedConstraints;
            // Limpieza de caché
            cachedPlayerRb = null;
        }
    }


}
