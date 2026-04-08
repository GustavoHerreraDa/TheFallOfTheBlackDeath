using UnityEngine;

[System.Serializable]
public class DialogueChoice
{
    public string playerText;

    [Header("Condiciones de Aparición")]
    public string requiredFlag; // La opción solo aparece si el jugador tiene este flag
    public string forbiddenFlag; // La opción desaparece si el jugador tiene este flag

    public DialogueEvent.DialogueEndAction action = DialogueEvent.DialogueEndAction.None;
    public Dialogue nextDialogue;

    public string[] addFlags;
    public string[] removeFlags;

    [Header("Recompensa (Solo si action es GiveItem)")]
    public int itemID;
    public int itemAmount = 1;
    public InventoryDateBase.Uso itemType; // Equipable, Usable, etc.
}