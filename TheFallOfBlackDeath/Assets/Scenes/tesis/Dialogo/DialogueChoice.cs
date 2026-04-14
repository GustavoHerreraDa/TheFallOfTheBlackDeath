using UnityEngine;

[System.Serializable]
/// <summary>
/// Represents a selectable dialogue option and the gameplay effects it can trigger.
/// </summary>
public class DialogueChoice
{
    public string playerText;

    [Header("Condiciones de ApariciÃ³n")]
    public string requiredFlag; // La opciÃ³n solo aparece si el jugador tiene este flag
    public string forbiddenFlag; // La opciÃ³n desaparece si el jugador tiene este flag

    public DialogueEvent.DialogueEndAction action = DialogueEvent.DialogueEndAction.None;
    public Dialogue nextDialogue;

    public string[] addFlags;
    public string[] removeFlags;

    [Header("Recompensa (Solo si action es GiveItem)")]
    public int itemID;
    public int itemAmount = 1;
    public InventoryDateBase.Uso itemType; // Equipable, Usable, etc.
}
