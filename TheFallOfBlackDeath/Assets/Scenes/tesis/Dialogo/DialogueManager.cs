using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;
    private Dialogue currentDialogue;
    private int currentLineIndex;
    private DialogueUI ui;

    [Header("Player")]
    public PlayerControl playerControl;

    private GameObject currentNPC;
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
                rb.velocity = Vector3.zero;
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

        ui.HideChoices();
        ui.DisplayLine(line);
        ui.onTypingFinished = () =>
        {
            if (line.hasChoices)
                ui.ShowChoices(line.choices);
        };
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

        if (choice.addFlags != null)
            foreach (var f in choice.addFlags) GlobalState.Instance.AddFlag(f);
        if (choice.removeFlags != null)
            foreach (var f in choice.removeFlags) GlobalState.Instance.RemoveFlag(f);
   
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

        if (playerControl != null)
            playerControl.enabled = true;

        if (currentNPC != null)
        {
            DialogueEvent evt = currentNPC.GetComponent<DialogueEvent>();
            if (evt != null)
                evt.onDialogueEnd = action;

            evt?.TriggerEvent();
        }

        currentNPC = null;
    }


}
