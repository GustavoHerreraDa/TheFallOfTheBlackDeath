using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

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

    private void Awake()
    {
        if (button == null) button = GetComponent<Button>();
        if (label == null) label = GetComponentInChildren<TextMeshProUGUI>();
    }

    public void SetText(string text) => label.text = text;
    public void SetTarget(Fighter fighter) => target = fighter;
    public void Show() => button.gameObject.SetActive(true);
    public void Hide() => button.gameObject.SetActive(false);

    public void OnPointerEnter(PointerEventData eventData)
    {
        // === COMUNICACIÓN CON CAMERAMANAGER ===
        if (CameraManager.Instance != null)
            CameraManager.Instance.SetSelectionZoom(true);
        
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
    
    public void ForceReset()
    {
        HideCanvasAndReset();
    }
}