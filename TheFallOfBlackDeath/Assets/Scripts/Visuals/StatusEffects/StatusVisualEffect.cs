using System.Collections.Generic;
using UnityEngine;

// Simple component responsible for spawning and cleaning up a status visual for a single body part.
// It parents spawned VFX to the body's hit point and applies an optional tint to the relevant meshes.
/// <summary>
/// Owns the spawned visual effect attached to a fighter body part while a status remains active.
/// </summary>
public class StatusVisualEffect : MonoBehaviour
{
    private Fighter fighter;
    private BodyPart bodyPart;
    private StatusVisualDatabase.Entry entry;

    private Transform attachPoint;
    private GameObject spawnedVfx;

    // Cache of original colors to restore when clearing tints
    private readonly Dictionary<Renderer, Color> originalColors = new Dictionary<Renderer, Color>();
    private static readonly int ColorProp = Shader.PropertyToID("_Color");

    /// <summary>
    /// Initializes the ialize.
    /// </summary>
    /// <param name="f">The f.</param>
    /// <param name="part">The part.</param>
    /// <param name="dbEntry">The db entry.</param>
    public void Initialize(Fighter f, BodyPart part, StatusVisualDatabase.Entry dbEntry)
    {
        fighter = f;
        bodyPart = part;
        entry = dbEntry;

        if (fighter == null) return;

        attachPoint = fighter.GetHitPoint(part);
        if (attachPoint == null) attachPoint = fighter.transform;

        // Spawn VFX prefab if provided
        if (entry != null && entry.visualPrefab != null)
        {
            spawnedVfx = Instantiate(entry.visualPrefab, attachPoint.position, attachPoint.rotation, attachPoint);
        }

        // Apply tint (optional)
        if (entry != null && entry.useTint)
        {
            ApplyTint(entry.tint, entry.tintStrength);
        }
    }

    /// <summary>
    /// Executes the cleanup workflow.
    /// </summary>
    public void Cleanup()
    {
        // Destroy spawned VFX
        if (spawnedVfx != null)
        {
            Destroy(spawnedVfx);
            spawnedVfx = null;
        }

        // Remove tint
        ClearTint();
    }

    /// <summary>
    /// Executes the on destroy workflow.
    /// </summary>
    private void OnDestroy()
    {
        // Safety: ensure cleanup when destroyed externally
        Cleanup();
    }

    /// <summary>
    /// Applies the tint.
    /// </summary>
    /// <param name="tint">The tint.</param>
    /// <param name="strength">The strength.</param>
    private void ApplyTint(Color tint, float strength)
    {
        if (fighter == null) return;
        string partName = bodyPart.ToString();

        // Find renderers whose name matches or contains the body part name (same heuristic used in Fighter.HidePartMesh)
        Renderer[] allRenderers = fighter.GetComponentsInChildren<Renderer>(true);
        foreach (var r in allRenderers)
        {
            if (!r.enabled) continue; // skip hidden parts (destroyed)
            if (!(r.name.Equals(partName, System.StringComparison.OrdinalIgnoreCase) || r.name.Contains(partName)))
                continue;

            // Record original color once
            if (!originalColors.ContainsKey(r))
            {
                Color baseColor = Color.white;
                var sharedMat = r.sharedMaterial;
                if (sharedMat != null && sharedMat.HasProperty(ColorProp))
                    baseColor = sharedMat.color;
                originalColors[r] = baseColor;
            }

            // Blend original color with tint
            Color from = originalColors[r];
            Color to = Color.Lerp(from, tint, Mathf.Clamp01(strength));

            var mpb = new MaterialPropertyBlock();
            r.GetPropertyBlock(mpb);
            mpb.SetColor(ColorProp, to);
            r.SetPropertyBlock(mpb);
        }
    }

    /// <summary>
    /// Executes the clear tint workflow.
    /// </summary>
    private void ClearTint()
    {
        if (originalColors.Count == 0) return;

        foreach (var kvp in originalColors)
        {
            var r = kvp.Key;
            if (r == null) continue;

            // Restore original color using property block
            var mpb = new MaterialPropertyBlock();
            r.GetPropertyBlock(mpb);
            mpb.SetColor(ColorProp, kvp.Value);
            r.SetPropertyBlock(mpb);
        }

        originalColors.Clear();
    }
}
