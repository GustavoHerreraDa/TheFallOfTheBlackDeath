using UnityEngine;

/// <summary>
/// Controlador centralizado para gestionar los materiales de una parte del cuerpo,
/// evitando conflictos entre el sistema de escaneo y el resaltado (hover).
/// </summary>
[RequireComponent(typeof(Renderer))]
public class BodyPartMaterialController : MonoBehaviour
{
    public enum VisualState
    {
        Normal,
        Scanned,
        Hovered
    }

    private Renderer targetRenderer;
    private Material[] originalMaterials;
    private Material scannerMaterial;
    private Material hoverMaterial;

    private bool isScanned;
    private bool isHovered;
    private VisualState lastAppliedState = VisualState.Normal;

    private void Awake()
    {
        targetRenderer = GetComponent<Renderer>();
        // Guardamos los materiales originales de fábrica de forma segura
        originalMaterials = targetRenderer.sharedMaterials;
    }

    /// <summary>
    /// Define si el estado de escaneo está activo y qué material usar.
    /// </summary>
    public void SetScannerState(bool active, Material scanMat)
    {
        isScanned = active;
        scannerMaterial = scanMat;
        ApplyStates();
    }

    /// <summary>
    /// Define si el estado de hover está activo y qué material usar.
    /// </summary>
    public void SetHoverState(bool active, Material hovMat)
    {
        isHovered = active;
        hoverMaterial = hovMat;
        ApplyStates();
    }

    private void ApplyStates()
    {
        if (targetRenderer == null) return;

        // Determinamos el estado actual basado en la Regla de Prioridad:
        // Hovered > Scanned > Normal
        VisualState currentState = VisualState.Normal;
        if (isHovered) currentState = VisualState.Hovered;
        else if (isScanned) currentState = VisualState.Scanned;

        // Optimización Crítica: Si el estado no ha cambiado, evitamos asignaciones costosas a .materials
        // que generarían basura para el GC (instanciación de arrays de materiales).
        if (currentState == lastAppliedState) return;

        switch (currentState)
        {
            case VisualState.Hovered:
                ApplyMaterialOverride(hoverMaterial);
                break;
            case VisualState.Scanned:
                ApplyMaterialOverride(scannerMaterial);
                break;
            case VisualState.Normal:
                targetRenderer.materials = originalMaterials;
                break;
        }

        lastAppliedState = currentState;
    }

    private void ApplyMaterialOverride(Material overrideMat)
    {
        if (overrideMat == null) return;

        // Creamos un array temporal para aplicar el material a todos los slots del renderer
        Material[] newMats = new Material[originalMaterials.Length];
        for (int i = 0; i < newMats.Length; i++)
        {
            newMats[i] = overrideMat;
        }
        targetRenderer.materials = newMats;
    }
}
