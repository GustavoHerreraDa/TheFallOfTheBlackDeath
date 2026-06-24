using UnityEngine;
using InventoryNew;

[System.Serializable]
/// <summary>
/// Represents a selectable dialogue option and the gameplay effects it can trigger.
/// </summary>
public class DialogueChoice
{
    public string playerText;

    [Header("Condiciones de Aparición")]
    [Tooltip("La opción aparece solo si el jugador tiene TODOS estos flags.")]
    public GlobalFlag[] requiredFlagsSO;
    [Tooltip("La opción desaparece si el jugador tiene CUALQUIERA de estos flags.")]
    public GlobalFlag[] forbiddenFlagsSO;

    public DialogueEvent.DialogueEndAction action = DialogueEvent.DialogueEndAction.None;
    public Dialogue nextDialogue;

    public GlobalFlag[] addFlagsSO;
    public GlobalFlag[] removeFlagsSO;

    [Header("Condición de Ítem para la Opción")]
    [Tooltip("El jugador solo ve esta opción si tiene este ítem. Dejar vacío = sin condición.")]
    public NewItemData requiredItemSO;
    [Tooltip("Cantidad mínima requerida.")]
    public int requiredItemAmount = 1;

    [Header("Costo de Ítem (entrega al elegir)")]
    [Tooltip("Si está definido, se consume este ítem al seleccionar la opción.")]
    public NewItemData costItemSO;
    public int costItemAmount = 1;
    [Tooltip("Si es true y el jugador no tiene el costItem, la opción se muestra bloqueada con el texto del motivo.")]
    public bool showIfMissingCost = false;
    [Tooltip("Texto que aparece si el jugador no tiene el ítem de costo.")]
    public string missingCostLabel;

    [Header("Recompensa (Solo si action es GiveItem)")]
    public NewItemData rewardItemSO;
    public int itemAmount = 1;
}
