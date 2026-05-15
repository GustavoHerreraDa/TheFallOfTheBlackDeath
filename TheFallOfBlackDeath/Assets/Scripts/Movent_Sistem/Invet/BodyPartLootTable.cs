using System.Collections.Generic;
using UnityEngine;
using InventoryNew;

// Attach this ScriptableObject to EnemyFighter prefabs via the Inspector.
// Each entry says: "give item X if these body parts are NOT destroyed."

[CreateAssetMenu(fileName = "LootTable", menuName = "Combat/Body Part Loot Table", order = 2)]
public class BodyPartLootTable : ScriptableObject
{
    [System.Serializable]
    public class LootEntry
    {
        [Header("Legacy Item (Old System)")]
        [System.Obsolete("Legacy inventory is no longer supported. Use 'newItemData'.")]
        [HideInInspector]
        public int itemId;
        [System.Obsolete("Legacy inventory is no longer supported. Use 'newItemData'.")]
        [HideInInspector]
        public InventoryDateBase.Uso uso = InventoryDateBase.Uso.Equipable;

        [Header("New Item (Fear & Hunger System)")]
        [Tooltip("Assign the new NewItemData asset here")]
        public NewItemData newItemData;

        [Header("Common Settings")]
        public int amount = 1;

        [Header("Display Override (Used if newItemData is null)")]
        public string itemDisplayName = "Item";
        public Sprite itemSprite;

        [Header("Condition — Parts that must be INTACT to drop this item")]
        [Tooltip("If ANY of these parts is destroyed, this item is NOT dropped.")]
        public List<BodyPart> requiredIntactParts = new List<BodyPart>();

        [Header("Condition — Parts that must be DESTROYED to drop this item (optional)")]
        [Tooltip("If set, ALL of these parts must be destroyed for the item to drop.")]
        public List<BodyPart> requiredDestroyedParts = new List<BodyPart>();

        [TextArea(2, 4)]
        [Tooltip("Shown in the loot panel so the player understands how to farm this item.")]
        public string hint = "Keep the legs intact to loot the boots.";
    }

    public List<LootEntry> entries = new List<LootEntry>();

    /// <summary>
    /// Evaluates which entries drop given the enemy's current body part states.
    /// </summary>
    public List<LootEntry> Evaluate(Fighter enemy)
    {
        var result = new List<LootEntry>();

        foreach (var entry in entries)
        {
            bool passes = true;

            // Check that required-intact parts are NOT destroyed
            foreach (BodyPart part in entry.requiredIntactParts)
            {
                var partData = enemy.GetBodyPart(part);
                if (partData != null && partData.IsDestroyed)
                {
                    passes = false;
                    break;
                }
            }

            if (!passes) continue;

            // Check that required-destroyed parts ARE destroyed
            foreach (BodyPart part in entry.requiredDestroyedParts)
            {
                var partData = enemy.GetBodyPart(part);
                if (partData == null || !partData.IsDestroyed)
                {
                    passes = false;
                    break;
                }
            }

            if (passes)
                result.Add(entry);
        }

        return result;
    }
}
