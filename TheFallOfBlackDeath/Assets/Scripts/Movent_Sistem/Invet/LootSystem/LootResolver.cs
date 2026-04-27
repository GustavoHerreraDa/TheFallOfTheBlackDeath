using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Attach this to the same GameObject as EnemyFighter.
/// Assign a BodyPartLootTable ScriptableObject in the Inspector.
/// CombatManager will call Resolve() on each dead enemy after victory.
/// </summary>
public class LootResolver : MonoBehaviour
{
    [Tooltip("The loot table for this specific enemy.")]
    public BodyPartLootTable lootTable;

    /// <summary>
    /// Returns the loot entries that passed body-part conditions.
    /// </summary>
    public List<BodyPartLootTable.LootEntry> Resolve(Fighter enemy)
    {
        if (lootTable == null)
            return new List<BodyPartLootTable.LootEntry>();

        return lootTable.Evaluate(enemy);
    }
}
