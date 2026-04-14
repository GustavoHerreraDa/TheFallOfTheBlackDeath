using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles fading object for the current project workflow.
/// </summary>
public class FadingObject : MonoBehaviour, IEquatable<FadingObject>
{
    public List<Renderer> Renderers = new List<Renderer>();
    public Vector3 Position;
    public List<Material> Materials = new List<Material>();
    [HideInInspector]
    public float InitialAlpha;

    /// <summary>
    /// Initializes cached references and runtime state before the component starts running.
    /// </summary>
    private void Awake()
    {
        Position = transform.position;

        if (Renderers.Count == 0)
        {
            Renderers.AddRange(GetComponentsInChildren<Renderer>());
        }
        foreach(Renderer renderer in Renderers)
        {
            Materials.AddRange(renderer.materials);
        }

        InitialAlpha = Materials[0].color.a;
    }

    /// <summary>
    /// Executes the equals workflow.
    /// </summary>
    /// <param name="other">The other.</param>
    /// <returns>True when the requested condition is met; otherwise, false.</returns>
    public bool Equals(FadingObject other)
    {
        return Position.Equals(other.Position);
    }

    /// <summary>
    /// Gets the hash code.
    /// </summary>
    /// <returns>The resulting value.</returns>
    public override int GetHashCode()
    {
        return Position.GetHashCode();
    }
}
