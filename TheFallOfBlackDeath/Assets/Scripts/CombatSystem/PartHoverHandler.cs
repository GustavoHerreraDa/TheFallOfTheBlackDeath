using UnityEngine;
using UnityEngine.EventSystems;

public class PartHoverHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    private Renderer targetRenderer;
    private Material highlightMaterial;
    [SerializeField]
    private Material[] originalMaterials; 

    public void Init(Renderer rend, Material highMat)
    {
        targetRenderer = rend;
        highlightMaterial = highMat;

        if (rend != null)
        {
            Material[] mats = rend.materials;
            originalMaterials = new Material[mats.Length];
            mats.CopyTo(originalMaterials, 0);
        }
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (targetRenderer == null || highlightMaterial == null) return;

        // Reemplazamos TODOS los slots de materiales por el shader de escaneo
        // Esto elimina el outline blanco mientras está seleccionado
        Material[] highlightSheet = new Material[targetRenderer.materials.Length];
        for (int i = 0; i < highlightSheet.Length; i++) {
            highlightSheet[i] = highlightMaterial;
        }
        targetRenderer.materials = highlightSheet;
    }

    public void OnPointerExit(PointerEventData eventData) => ResetToOriginal();

    public void OnPointerClick(PointerEventData eventData) => ResetToOriginal();

    public void ResetToOriginal()
    {
        if (targetRenderer != null && originalMaterials != null) {
            targetRenderer.materials = originalMaterials;
        }
    }

    private void OnDisable() => ResetToOriginal();
}