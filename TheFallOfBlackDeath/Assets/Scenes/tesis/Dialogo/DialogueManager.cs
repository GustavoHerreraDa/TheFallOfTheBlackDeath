using UnityEngine;

using InventoryNew;

/// <summary>
/// Controls dialogue progression, player input locking, branching choices, and gameplay events triggered from conversations.
/// </summary>
public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;
    private Dialogue currentDialogue;
    private int currentLineIndex;
    private DialogueUI ui;
    public delegate void GiveItemHandler(string id, int amount);
    public static event GiveItemHandler OnGiveItem;
    

    // Cache del Rigidbody del jugador para restaurar su estado tras el diálogo
    private Rigidbody cachedPlayerRb;
    private bool cachedUseGravity;
    private RigidbodyConstraints cachedConstraints;

    private GameObject currentNPC;
    public delegate void RecruitEventHandler(GameObject npc, int fighterIndex);
    public static event RecruitEventHandler OnRecruitCharacter;
    public bool IsDialogueActive => currentDialogue != null;

    private bool currentLineHasChoices;
    private PlayerControl cachedPlayerControl;

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

        cachedPlayerControl = ResolvePlayerControl();
        if (cachedPlayerControl != null)
        {
            cachedPlayerControl.enabled = false;
            cachedPlayerControl.anim.SetFloat("Movent", 0f);
            // Intentamos obtener el rigidbody (en el root o en hijos) para congelar al jugador
            cachedPlayerRb = cachedPlayerControl.GetComponent<Rigidbody>();
            if (cachedPlayerRb == null)
                cachedPlayerRb = cachedPlayerControl.GetComponentInChildren<Rigidbody>();

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
    /// Comprueba si el diálogo tiene alguna línea que pueda mostrarse según las condiciones actuales.
    /// </summary>
    public bool HasAvailableContent(Dialogue dialogue)
    {
        if (dialogue == null || dialogue.lines == null) return false;

        // Si el diálogo ya fue leído por completo y no tiene flags que lo reinicien, se considera "sin contenido nuevo"
        if (GlobalState.Instance != null && GlobalState.Instance.HasFlag("Read_" + dialogue.Id))
        {
            // Podríamos añadir lógica aquí para ver si hay ramas no exploradas,
            // pero por simplicidad "pro", si el usuario no puso flags de condición
            // en las líneas, asumimos que es el mismo diálogo de siempre.
            return false;
        }

        foreach (var line in dialogue.lines)
        {
            if (IsLineVisible(line)) return true;
        }
        return false;
    }

    /// <summary>
    /// Evalúa si una opción específica es visible según flags e ítems.
    /// </summary>
    public bool IsChoiceVisible(DialogueChoice choice, out bool canPayCost)
    {
        canPayCost = true;

        if (GlobalState.Instance != null)
        {
            if (!string.IsNullOrEmpty(choice.requiredFlag) && !GlobalState.Instance.HasFlag(choice.requiredFlag))
                return false;
            if (choice.requiredFlagSO != null && !GlobalState.Instance.HasFlag(choice.requiredFlagSO))
                return false;
            if (!string.IsNullOrEmpty(choice.forbiddenFlag) && GlobalState.Instance.HasFlag(choice.forbiddenFlag))
                return false;
            if (choice.forbiddenFlagSO != null && GlobalState.Instance.HasFlag(choice.forbiddenFlagSO))
                return false;
        }

        if (!string.IsNullOrEmpty(choice.requiredItemId))
        {
            if (NewInventoryManager.Instance == null || !NewInventoryManager.Instance.HasItem(choice.requiredItemId, choice.requiredItemAmount))
                return false;
        }

        if (!string.IsNullOrEmpty(choice.costItemId))
        {
            bool hasCost = NewInventoryManager.Instance != null && NewInventoryManager.Instance.HasItem(choice.costItemId, choice.costItemAmount);
            if (!hasCost)
            {
                canPayCost = false;
                if (!choice.showIfMissingCost) return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Evalúa si una línea específica es visible según flags e ítems.
    /// </summary>
    private bool IsLineVisible(DialogueLine line)
    {
        if (GlobalState.Instance != null)
        {
            if (!string.IsNullOrEmpty(line.requiredFlag) && !GlobalState.Instance.HasFlag(line.requiredFlag))
                return false;
            if (line.requiredFlagSO != null && !GlobalState.Instance.HasFlag(line.requiredFlagSO))
                return false;
            if (!string.IsNullOrEmpty(line.forbiddenFlag) && GlobalState.Instance.HasFlag(line.forbiddenFlag))
                return false;
            if (line.forbiddenFlagSO != null && GlobalState.Instance.HasFlag(line.forbiddenFlagSO))
                return false;
        }

        if (!string.IsNullOrEmpty(line.requiredItemId))
        {
            if (NewInventoryManager.Instance == null || !NewInventoryManager.Instance.HasItem(line.requiredItemId, line.requiredItemAmount))
                return false;
        }

        return true;
    }

    /// <summary>
    /// Shows the line.
    /// </summary>
    private void ShowLine()
    {
        DialogueLine line = currentDialogue.lines[currentLineIndex];

        if (!IsLineVisible(line))
        {
            SkipLine();
            return;
        }

        ui.HideChoices();
        currentLineHasChoices = line.hasChoices;
        ui.DisplayLine(line);
        ui.onTypingFinished = () =>
        {
            if (line.hasChoices && line.choices != null && line.choices.Count > 0)
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
        // Marcar diálogo como leído (Pro)
        if (currentDialogue != null && GlobalState.Instance != null)
        {
            GlobalState.Instance.AddFlag("Read_" + currentDialogue.Id);
        }

        ui.ShowUI(false);
        currentDialogue = null;

        if (cachedPlayerControl != null)
            cachedPlayerControl.enabled = true;
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

        // Consumir ítem de costo si aplica
        if (!string.IsNullOrEmpty(choice.costItemId))
        {
            if (NewInventoryManager.Instance != null && NewInventoryManager.Instance.HasItem(choice.costItemId, choice.costItemAmount))
            {
                NewInventoryManager.Instance.RemoveItem(choice.costItemId, choice.costItemAmount);
                Debug.Log($"[DialogueManager] Consumido: {choice.costItemId} x{choice.costItemAmount}");
            }
            else
            {
                // Guard de seguridad: si llegó aquí sin el ítem, no procesar
                Debug.LogWarning($"[DialogueManager] SelectChoice: el jugador no tenía {choice.costItemId} x{choice.costItemAmount} al confirmar. Abortando.");
                return;
            }
        }

        // Flags logic...
        if (choice.addFlags != null)
            foreach (var f in choice.addFlags) GlobalState.Instance.AddFlag(f);
        if (choice.addFlagsSO != null)
            foreach (var f in choice.addFlagsSO) GlobalState.Instance.AddFlag(f);
            
        if (choice.removeFlags != null)
            foreach (var f in choice.removeFlags) GlobalState.Instance.RemoveFlag(f);
        if (choice.removeFlagsSO != null)
            foreach (var f in choice.removeFlagsSO) GlobalState.Instance.RemoveFlag(f);

        // --- LÃ“GICA DE DAR ITEM ---
        if (choice.action == DialogueEvent.DialogueEndAction.GiveItem)
        {
            // Disparamos el evento con los datos del ScriptableObject
            Debug.Log($"Dialogo: Regalando item ID {choice.itemID}");
            OnGiveItem?.Invoke(choice.itemID, choice.itemAmount);

            if (NewInventoryManager.Instance != null)
            {
                var itemData = NewInventoryManager.Instance.GetItemDataById(choice.itemID);
                if (itemData != null)
                {
                    NewInventoryManager.Instance.AddItem(itemData, choice.itemAmount);
                }
            }
            
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
        // Marcar diálogo como leído (Pro)
        if (currentDialogue != null && GlobalState.Instance != null)
        {
            GlobalState.Instance.AddFlag("Read_" + currentDialogue.Id);
        }

        ui.ShowUI(false);
        // Marcar diálogo como finalizado para permitir hablar con otros NPC
        currentDialogue = null;
        if (cachedPlayerControl != null) cachedPlayerControl.enabled = true;
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

    private PlayerControl ResolvePlayerControl()
    {
        // Primero intentar desde el líder del party
        if (GameManager.Instance != null)
        {
            var leader = GameManager.Instance.GetLeader();
            if (leader != null)
            {
                var pc = leader.GetComponent<PlayerControl>();
                if (pc != null) return pc;
            }
            // Fallback: character raíz del GameManager
            if (GameManager.Instance.character != null)
                return GameManager.Instance.character.GetComponent<PlayerControl>();
        }
        return null;
    }

    private void Update()
    {
        if (IsDialogueActive && Input.GetKeyDown(KeyCode.E))
        {
            // Si está escribiendo, siempre permitimos saltar el texto (incluso con opciones)
            if (ui.IsTyping)
            {
                NextLine();
            }
            // Si no hay opciones, permitimos pasar a la siguiente línea
            else if (!currentLineHasChoices)
            {
                NextLine();
            }
        }
    }
}
