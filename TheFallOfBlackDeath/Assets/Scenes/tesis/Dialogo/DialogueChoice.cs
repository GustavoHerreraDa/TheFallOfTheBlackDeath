using UnityEngine;

[System.Serializable]
public class DialogueChoice
{
    public string playerText;
    public DialogueEvent.DialogueEndAction action = DialogueEvent.DialogueEndAction.None;
    public Dialogue nextDialogue;

    public string[] addFlags;
    public string[] removeFlags;

    [Header("Recompensa (Solo si action es GiveItem)")]
    public int itemID;
    public int itemAmount = 1;
    public InventoryDateBase.Uso itemType; // Equipable, Usable, etc.
}