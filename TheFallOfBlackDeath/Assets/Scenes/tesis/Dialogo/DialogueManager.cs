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
        currentLineIndex++;

        if (currentLineIndex < currentDialogue.lines.Length)
            ShowLine();
        else
            EndDialogue();
    }

    private void ShowLine()
    {
        DialogueLine line = currentDialogue.lines[currentLineIndex];
        ui.DisplayLine(line);
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
}
