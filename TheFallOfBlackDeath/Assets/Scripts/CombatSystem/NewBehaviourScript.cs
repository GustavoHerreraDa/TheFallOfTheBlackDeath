using UnityEngine;
using UnityEngine.EventSystems;

public class PartHoverHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    private Renderer targetRenderer;
    private Material[] originalMaterials; // Guardamos el array completo
    private Material highlightMaterial;

    public void Init(Renderer rend, Material highMat)
    {
        targetRenderer = rend;
        highlightMaterial = highMat;
        
        if (rend != null)
        {
            // Guardamos una copia de todos los materiales originales (base + outline)
            originalMaterials = rend.materials;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (targetRenderer != null && highlightMaterial != null)
        {
            // Creamos un array del mismo tamaño que el original lleno con el material de brillo
            Material[] highlightSheet = new Material[targetRenderer.materials.Length];
            for (int i = 0; i < highlightSheet.Length; i++)
            {
                highlightSheet[i] = highlightMaterial;
            }
            
            // Aplicamos a todos los slots (esto quita el blanco del outline)
            targetRenderer.materials = highlightSheet;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ResetToOriginal();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // Al hacer clic para atacar, reseteamos el color antes de que el panel se cierre
        ResetToOriginal();
    }

    private void ResetToOriginal()
    {
        if (targetRenderer != null && originalMaterials != null)
        {
            targetRenderer.materials = originalMaterials;
        }
    }

    // Por seguridad, si el botón se destruye o el panel se oculta, reseteamos
    private void OnDisable()
    {
        ResetToOriginal();
    }
}