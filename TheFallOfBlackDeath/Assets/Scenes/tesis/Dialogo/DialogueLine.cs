using UnityEngine;
using System.Collections.Generic;
using InventoryNew;

[System.Serializable]
/// <summary>
/// Represents a single dialogue line, including speaker data, branching metadata, and conditional flags.
/// </summary>
public class DialogueLine
{
    public string speakerName;
    [TextArea(2, 5)]
    public string sentence;

    [Header("Condiciones de Aparición")]
    [Tooltip("La línea aparece solo si el jugador tiene TODOS estos flags.")]
    public GlobalFlag[] requiredFlagsSO;
    [Tooltip("La línea desaparece si el jugador tiene CUALQUIERA de estos flags.")]
    public GlobalFlag[] forbiddenFlagsSO;

    [Header("Condición de Ítem (opcional)")]
    [Tooltip("El ítem que el jugador debe tener para que esta línea aparezca. Dejar vacío = sin condición.")]
    public NewItemData requiredItemSO;
    [Tooltip("Cantidad mínima requerida.")]
    public int requiredItemAmount = 1;
    [Tooltip("Si el jugador TIENE este ítem, esta línea NO aparece.")]
    public NewItemData forbiddenItemSO;
    [Tooltip("Cantidad mínima para que se considere forbidden.")]
    public int forbiddenItemAmount = 1;

    public bool hasChoices;
    public List<DialogueChoice> choices = new List<DialogueChoice>();
}
