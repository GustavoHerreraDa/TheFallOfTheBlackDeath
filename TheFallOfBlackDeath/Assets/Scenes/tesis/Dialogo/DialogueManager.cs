using UnityEngine;

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

    private GameObject currentNPC;
    public delegate void RecruitEventHandler(GameObject npc, int fighterIndex);
    public static event RecruitEventHandler OnRecruitCharacter;
    public bool IsDialogueActive => currentDialogue != null;
    private void Awake()
    {
        if (Instance == null) Instance = this;
        ui = FindObjectOfType<DialogueUI>();
    }

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
            Rigidbody rb = playerControl.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.useGravity = false;
                rb.constraints = RigidbodyConstraints.FreezeAll; 
            }


        }

        ShowLine();
    }

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

    private void SkipLine()
    {
        currentLineIndex++;
        if (currentLineIndex < currentDialogue.lines.Length)
            ShowLine();
        else
            EndDialogue();
    }


    private void EndDialogue()
    {
        ui.ShowUI(false);
        currentDialogue = null;

        if (playerControl != null)
            playerControl.enabled = true;
       
        if (currentNPC != null)
        {
            DialogueEvent evt = currentNPC.GetComponent<DialogueEvent>();
            if (evt != null)
                evt.TriggerEvent();
        }

        currentNPC = null;
    }


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

    private void EndDialogueWithAction(DialogueEvent.DialogueEndAction action)
    {
        ui.ShowUI(false);
        if (playerControl != null) playerControl.enabled = true;

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


}
