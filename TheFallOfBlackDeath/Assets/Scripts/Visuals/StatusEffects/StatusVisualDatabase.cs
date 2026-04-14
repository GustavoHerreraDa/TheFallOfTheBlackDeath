using System.Collections.Generic;
using UnityEngine;

// Simple, inspector-driven mapping from PartStatus -> visual prefab/tint
// Create an asset via: Create > Status Visuals > Status Visual Database
[CreateAssetMenu(menuName = "Status Visuals/Status Visual Database", fileName = "StatusVisualDatabase")]
/// <summary>
/// Stores the visual mappings used to convert gameplay status values into spawned effects.
/// </summary>
public class StatusVisualDatabase : ScriptableObject
{
    [System.Serializable]
    /// <summary>
    /// Supports status-effect presentation by handling entry.
    /// </summary>
    public class Entry
    {
        [Tooltip("Status this visual represents")] public PartStatus status = PartStatus.None;
        [Tooltip("Optional particle or VFX prefab to spawn on the body part's hit point")] public GameObject visualPrefab;
        [Header("Tint (Optional)")]
        [Tooltip("Apply a simple material color tint to the meshes for the affected body part")] public bool useTint = true;
        [Tooltip("Tint color to apply when status is active (e.g., Corroded -> green, Burning -> orange)")] public Color tint = Color.white;
        [Range(0f, 1f)] [Tooltip("How strongly to blend the tint color with the original material color")] public float tintStrength = 1f;
    }

    [SerializeField]
    public List<Entry> entries = new List<Entry>();

    private Dictionary<PartStatus, Entry> _map;

    /// <summary>
    /// Gets the entry.
    /// </summary>
    /// <param name="status">The status.</param>
    /// <returns>The resulting value.</returns>
    public Entry GetEntry(PartStatus status)
    {
        if (status == PartStatus.None) return null;
        if (_map == null)
        {
            _map = new Dictionary<PartStatus, Entry>();
            foreach (var e in entries)
            {
                if (e != null)
                {
                    _map[e.status] = e; // last wins, easy to override
                }
            }
        }

        _map.TryGetValue(status, out var entry);
        return entry;
    }
}
