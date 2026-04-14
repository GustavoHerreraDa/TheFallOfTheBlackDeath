using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NarrativeLogDatabase", menuName = "Narrative/Narrative Log Database", order = 1)]
/// <summary>
/// Stores the combat narration entries that can be resolved by enemy identifier.
/// </summary>
public class NarrativeLogDatabase : ScriptableObject
{
    [Tooltip("List of narrative entries for enemies. Assign one per enemy type.")]
    public List<EnemyNarrativeEntry> enemies = new List<EnemyNarrativeEntry>();

    private Dictionary<string, EnemyNarrativeEntry> _map;

    /// <summary>
    /// Gets the by id.
    /// </summary>
    /// <param name="enemyId">The enemy id.</param>
    /// <returns>The resulting value.</returns>
    public EnemyNarrativeEntry GetById(string enemyId)
    {
        if (string.IsNullOrEmpty(enemyId)) return null;
        if (_map == null)
        {
            _map = new Dictionary<string, EnemyNarrativeEntry>();
            foreach (var e in enemies)
            {
                if (e == null || string.IsNullOrEmpty(e.enemyId)) continue;
                if (!_map.ContainsKey(e.enemyId))
                    _map.Add(e.enemyId, e);
            }
        }
        _map.TryGetValue(enemyId, out var entry);
        return entry;
    }
}
