using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Item que al usarse muestra un panel para elegir qué parte del cuerpo curar.
/// Adjuntalo al mismo GameObject que InventoryUI o invocalo desde ahí.
/// </summary>
public class BodyPartHealItem : MonoBehaviour
{
    [Tooltip("Cuánta salud restaura en la parte seleccionada")]
    public float healAmount = 50f;

    // El panel que muestra los botones de partes del cuerpo
    public BodyPartHealPanel healPanel;

    /// <summary>
    /// Llamado desde InventoryUI cuando el jugador hace click en "Usar".
    /// </summary>
    public void Use(PlayerFighter target, int itemId)
    {
        if (healPanel == null)
        {
            Debug.LogWarning("[BodyPartHealItem] healPanel no asignado");
            return;
        }

        healPanel.Show(target, healAmount, onPartSelected: (part) =>
        {
            // Curar la parte seleccionada
            target.ModifyBodyPartHealth(part, healAmount);

            // Consumir el item
            InventoryManager.instance.DestroyItem(
                itemId, 1, InventoryDateBase.Uso.BodyPartHeal);

            Debug.Log($"[BodyPartHealItem] Curado {part} en {target.idName} por {healAmount}");
        });
    }
}