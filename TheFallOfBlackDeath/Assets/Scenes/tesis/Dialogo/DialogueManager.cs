using UnityEngine;

using InventoryNew;

/// <summary>
/// Controls dialogue progression, player input locking, branching choices, and gameplay events triggered from conversations.
/// </summary>
public class DialogueManager : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private DialogueUI ui;

    // --- NUEVO SISTEMA DE INPUT DESACOPLADO ---
    /// <summary>
    /// Entrada pública para avanzar en el diálogo. 
    /// Permite que cualquier script (PlayerInteraction, UI, etc.) dispare el avance.
    /// </summary>
    public void OnInteractInputPressed()
    {
        if (!IsDialogueActive) return;

        // Si está escribiendo, saltar el texto
        if (ui.IsTyping)
        {
            ui.SkipTyping();
            return;
        }

        // Si la línea tiene opciones
        if (currentLineHasChoices)
        {
            // Si las opciones aún no se muestran, las mostramos ahora
            if (!ui.IsShowingChoices)
            {
                DialogueLine line = currentDialogue.lines[currentLineIndex];
                ui.ShowChoices(line.choices);
                return;
            }
            // Si las opciones ya se muestran, no hacemos nada (el jugador debe elegir con el ratón/teclado)
            return;
        }

        // Avanzar normalmente si no hay opciones
        NextLine();
    }
    // ------------------------------------------

    public static DialogueManager Instance;
    private Dialogue currentDialogue;
    private int currentLineIndex;
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
        // Eliminamos FindObjectOfType para usar referencia serializada
        if (ui == null) ui = FindObjectOfType<DialogueUI>();
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

        // --- BLOQUEO SEGURO DEL JUGADOR ---
        cachedPlayerControl = ResolvePlayerControl();
        if (cachedPlayerControl != null)
        {
            // Usamos el nuevo método que maneja físicas y animaciones internamente
            cachedPlayerControl.ToggleDialogueState(true);
        }
        // ------------------------------------

        ShowLine();
    }



    /// <summary>
    /// Comprueba si el diálogo tiene alguna línea que pueda mostrarse según las condiciones actuales.
    /// Evalúa siempre todas las líneas para soportar misiones multi-visita (fetch quests).
    /// </summary>
    public bool HasAvailableContent(Dialogue dialogue)
    {
        if (dialogue == null || dialogue.lines == null) return false;

        foreach (var line in dialogue.lines)
            if (IsLineVisible(line)) return true;

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
            if (choice.requiredFlagsSO != null)
                foreach (var f in choice.requiredFlagsSO)
                    if (f != null && !GlobalState.Instance.HasFlag(f)) return false;

            if (choice.forbiddenFlagsSO != null)
                foreach (var f in choice.forbiddenFlagsSO)
                    if (f != null && GlobalState.Instance.HasFlag(f)) return false;
        }

        if (choice.requiredItemSO != null)
        {
            if (NewInventoryManager.Instance == null || !NewInventoryManager.Instance.HasItem(choice.requiredItemSO.id, choice.requiredItemAmount))
                return false;
        }

        if (choice.costItemSO != null)
        {
            bool hasCost = NewInventoryManager.Instance != null && NewInventoryManager.Instance.HasItem(choice.costItemSO.id, choice.costItemAmount);
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
            if (line.requiredFlagsSO != null)
                foreach (var f in line.requiredFlagsSO)
                    if (f != null && !GlobalState.Instance.HasFlag(f)) return false;

            if (line.forbiddenFlagsSO != null)
                foreach (var f in line.forbiddenFlagsSO)
                    if (f != null && GlobalState.Instance.HasFlag(f)) return false;
        }

        if (line.requiredItemSO != null)
        {
            if (NewInventoryManager.Instance == null || !NewInventoryManager.Instance.HasItem(line.requiredItemSO.id, line.requiredItemAmount))
                return false;
        }

        if (line.forbiddenItemSO != null)
        {
            if (NewInventoryManager.Instance != null && NewInventoryManager.Instance.HasItem(line.forbiddenItemSO.id, line.forbiddenItemAmount))
                return false;
        }

        return true;
    }

    /// <summary>
    /// Muestra la línea actual. Utiliza un bucle para saltar líneas que no cumplen condiciones,
    /// evitando recursividad profunda.
    /// </summary>
    private void ShowLine()
    {
        // Bucle iterativo para encontrar la siguiente línea válida
        while (currentLineIndex < currentDialogue.lines.Length)
        {
            DialogueLine line = currentDialogue.lines[currentLineIndex];

            if (IsLineVisible(line))
            {
                ui.HideChoices();
                currentLineHasChoices = line.hasChoices;
                ui.DisplayLine(line);

                // Ya no mostramos las opciones automáticamente al terminar de escribir
                // para que el jugador tenga tiempo de leer. Se mostrarán al pulsar 'E'.
                ui.onTypingFinished = null;
                
                return; // Encontramos una línea visible, salimos del método
            }

            currentLineIndex++;
        }

        // Si salimos del bucle sin encontrar líneas, terminamos el diálogo
        EndDialogue();
    }

    /// <summary>
    /// Avanza el índice de línea y muestra la siguiente.
    /// </summary>
    public void NextLine()
    {
        currentLineIndex++;
        ShowLine();
    }

    /// <summary>
    /// Método legacy para compatibilidad, ahora simplemente llama a NextLine.
    /// </summary>
    private void SkipLine()
    {
        NextLine();
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
            cachedPlayerControl.ToggleDialogueState(false);
       
        if (currentNPC != null)
        {
            DialogueEvent evt = currentNPC.GetComponent<DialogueEvent>();
            if (evt != null)
                evt.TriggerEvent(DialogueEvent.DialogueEndAction.None);
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
        if (choice.costItemSO != null)
        {
            if (NewInventoryManager.Instance != null && NewInventoryManager.Instance.HasItem(choice.costItemSO.id, choice.costItemAmount))
            {
                NewInventoryManager.Instance.RemoveItem(choice.costItemSO.id, choice.costItemAmount);
                Debug.Log($"[DialogueManager] Consumido: {choice.costItemSO.itemName} x{choice.costItemAmount}");
            }
            else
            {
                // Guard de seguridad: si llegó aquí sin el ítem, no procesar
                Debug.LogWarning($"[DialogueManager] SelectChoice: el jugador no tenía {choice.costItemSO.itemName} x{choice.costItemAmount} al confirmar. Abortando.");
                return;
            }
        }

        // Flags logic (SO only)
        if (choice.addFlagsSO != null)
            foreach (var f in choice.addFlagsSO) GlobalState.Instance.AddFlag(f);

        if (choice.removeFlagsSO != null)
            foreach (var f in choice.removeFlagsSO) GlobalState.Instance.RemoveFlag(f);

        // --- VALIDACIÓN DE CIERRE INMEDIATO ---
        if (choice.endDialogueAfterChoice)
        {
            if (choice.action != DialogueEvent.DialogueEndAction.None)
                EndDialogueWithAction(choice.action);
            else
                EndDialogue();

            return;
        }
        // --------------------------------------

        // --- LÓGICA DE DAR ITEM ---
        if (choice.action == DialogueEvent.DialogueEndAction.GiveItem)
        {
            if (choice.rewardItemSO != null && NewInventoryManager.Instance != null)
            {
                NewInventoryManager.Instance.AddItem(choice.rewardItemSO, choice.itemAmount);
                Debug.Log($"[DialogueManager] Recompensa entregada: {choice.rewardItemSO.itemName} x{choice.itemAmount}");
                OnGiveItem?.Invoke(choice.rewardItemSO.id, choice.itemAmount);
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
        if (cachedPlayerControl != null)
            cachedPlayerControl.ToggleDialogueState(false);

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
            evt?.TriggerEvent(action);
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
        // El input se gestiona ahora externamente via OnInteractInputPressed
    }
}