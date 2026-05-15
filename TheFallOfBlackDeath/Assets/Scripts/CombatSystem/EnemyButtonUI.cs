using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

/// <summary>
/// Supports the combat system by handling enemy button ui.
/// </summary>
public class EnemyButtonUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public Button button;
    public TextMeshProUGUI label;
    public int index;
    public Fighter target;

    public Material highlightMaterial;
    public GameObject effectPrfb;

    private List<Renderer> allRenderers = new List<Renderer>();
    private List<Material[]> originalMaterialsList = new List<Material[]>();
    
    private GameObject enemyCanvas;

    /// <summary>
    /// Initializes cached references and runtime state before the component starts running.
    /// </summary>
    private void Awake()
    {
        if (button == null) button = GetComponent<Button>();
        if (label == null) label = GetComponentInChildren<TextMeshProUGUI>();
    }

    public void SetText(string text) => label.text = text;
    public void SetTarget(Fighter fighter) => target = fighter;
    public void Show() => button.gameObject.SetActive(true);
    public void Hide() => button.gameObject.SetActive(false);

    /// <summary>
    /// Executes the on pointer enter workflow.
    /// </summary>
    /// <param name="eventData">The event data.</param>
    public void OnPointerEnter(PointerEventData eventData)
    {
        // === COMUNICACIÓN CON CAMERAMANAGER ===
        if (CameraManager.Instance != null)
            CameraManager.Instance.SetSelectionZoom(true, target);
        
        // === NUEVO: ACTIVAR BLOOM VERDE ===
        if (BloomManager.Instance != null)
            BloomManager.Instance.SetEnemyHighlight(true);
        // ==================================

        if (target == null) return;

        enemyCanvas = target.GetComponentInChildren<Canvas>(true)?.gameObject;
        if (enemyCanvas != null) enemyCanvas.SetActive(true);

        allRenderers.Clear();
        originalMaterialsList.Clear();
        allRenderers.AddRange(target.GetComponentsInChildren<Renderer>());

        foreach (Renderer rend in allRenderers)
        {
            Material[] mats = rend.materials;
            Material[] savedMats = new Material[mats.Length];
            mats.CopyTo(savedMats, 0);
            originalMaterialsList.Add(savedMats);

            Material[] highlightMats = new Material[mats.Length];
            for (int i = 0; i < highlightMats.Length; i++)
            {
                highlightMats[i] = highlightMaterial;
            }
            rend.materials = highlightMats;
        }

        if (effectPrfb != null)
        {
            effectPrfb.transform.position = target.transform.position;
            effectPrfb.SetActive(true);
        }
    }

    public void OnPointerExit(PointerEventData eventData) => HideCanvasAndReset();
    public void OnPointerClick(PointerEventData eventData) => HideCanvasAndReset();

    /// <summary>
    /// Hides the canvas and reset.
    /// </summary>
    private void HideCanvasAndReset()
    {
        // === RESETEAR ZOOM ===
        if (CameraManager.Instance != null)
            CameraManager.Instance.SetSelectionZoom(false);

        // === NUEVO: DESACTIVAR BLOOM VERDE ===
        if (BloomManager.Instance != null)
            BloomManager.Instance.SetEnemyHighlight(false);
        // =====================================

        if (enemyCanvas != null) enemyCanvas.SetActive(false);

        for (int i = 0; i < allRenderers.Count; i++)
        {
            if (allRenderers[i] != null && i < originalMaterialsList.Count)
            {
                allRenderers[i].materials = originalMaterialsList[i];
            }
        }

        if (effectPrfb != null) effectPrfb.SetActive(false);
    }
    
    /// <summary>
    /// Executes the reset highlight workflow.
    /// </summary>
    public void ResetHighlight()
    {
        if (CameraManager.Instance != null)
            CameraManager.Instance.SetSelectionZoom(false);

        // === NUEVO: ASEGURAR RESETEO DE BLOOM ===
        if (BloomManager.Instance != null)
            BloomManager.Instance.SetEnemyHighlight(false);
        // ========================================

        if (enemyCanvas != null) enemyCanvas.SetActive(false);
        for (int i = 0; i < allRenderers.Count; i++)
        {
            if (allRenderers[i] != null && i < originalMaterialsList.Count)
            {
                allRenderers[i].materials = originalMaterialsList[i];
            }
        }
        if (effectPrfb != null) effectPrfb.SetActive(false);
    }
    
    /// <summary>
    /// Executes the force reset workflow.
    /// </summary>
    public void ForceReset()
    {
        HideCanvasAndReset();
    }
}
