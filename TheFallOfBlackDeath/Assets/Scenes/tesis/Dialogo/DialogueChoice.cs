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
    public GlobalFlag requiredFlagSO;
    public GlobalFlag forbiddenFlagSO;

    public DialogueEvent.DialogueEndAction action = DialogueEvent.DialogueEndAction.None;
    public Dialogue nextDialogue;

    public string[] addFlags;
    public string[] removeFlags;
    public GlobalFlag[] addFlagsSO;
    public GlobalFlag[] removeFlagsSO;

    [Header("Condición de Ítem para la Opción")]
    [Tooltip("El jugador solo ve esta opción si tiene este ítem. Dejar vacío = sin condición.")]
    public string requiredItemId;
    [Tooltip("Cantidad mínima requerida.")]
    public int requiredItemAmount = 1;

    [Header("Costo de Ítem (entrega al elegir)")]
    [Tooltip("Si está definido, se consume este ítem al seleccionar la opción (simula 'entregar' un ítem al NPC).")]
    public string costItemId;
    public int costItemAmount = 1;
    [Tooltip("Si es true y el jugador no tiene el costItem, la opción se muestra bloqueada (grayed out) con el texto del motivo.")]
    public bool showIfMissingCost = false;
    [Tooltip("Texto que aparece junto a la opción si el jugador no tiene el ítem de costo (ej: 'Necesitas x2 Pieza de Repuesto').")]
    public string missingCostLabel;

    [Header("Recompensa (Solo si action es GiveItem)")]
    public string itemID; // Ahora es un string para el nuevo inventario
    public int itemAmount = 1;
}
