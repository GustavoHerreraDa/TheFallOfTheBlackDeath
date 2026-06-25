using UnityEngine;

/// <summary>
/// A collection of anchor points for different types of floating text.
/// Allows for precise, scene-visible control over where text spawns on a character.
/// </summary>
public class FloatingTextAnchorSet : MonoBehaviour
{
    [Header("Floating Text Anchors")]
    [SerializeField] private Transform damageAnchor;
    [SerializeField] private Transform critAnchor;
    [SerializeField] private Transform statModAnchor;
    [SerializeField] private Transform limbDestroyAnchor;

    /// <summary>
    /// Gets the world position for damage or critical hit text.
    /// Falls back to damageAnchor if isCritical is true but critAnchor is unassigned.
    /// Falls back to transform.position + Vector3.up * 1f if both are unassigned.
    /// </summary>
    public Vector3 GetDamagePosition(bool isCritical)
    {
        if (isCritical && critAnchor != null)
            return critAnchor.position;

        if (damageAnchor != null)
            return damageAnchor.position;

        return transform.position + Vector3.up * 1f;
    }

    /// <summary>
    /// Gets the world position for stat modification (buff/debuff) text.
    /// Falls back to transform.position + Vector3.up * 2f if unassigned.
    /// </summary>
    public Vector3 GetStatModPosition()
    {
        if (statModAnchor != null)
            return statModAnchor.position;

        return transform.position + Vector3.up * 2f;
    }

    /// <summary>
    /// Gets the world position for limb destruction events.
    /// Falls back to GetDamagePosition(false) if unassigned.
    /// </summary>
    public Vector3 GetLimbDestroyPosition()
    {
        if (limbDestroyAnchor != null)
            return limbDestroyAnchor.position;

        return GetDamagePosition(false);
    }
}
