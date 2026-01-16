using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic; // Necesario para List

public class EnemyButtonUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public Button button;
    public TextMeshProUGUI label;
    public int index;
    public Fighter target;

    public Material highlightMaterial;
    public GameObject effectPrfb;

    // Cambiamos a listas para manejar múltiples piezas
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
        if (target == null) return;

        // Activar Canvas del enemigo
        enemyCanvas = target.GetComponentInChildren<Canvas>(true)?.gameObject;
        if (enemyCanvas != null) enemyCanvas.SetActive(true);

        // LIMPIEZA: Obtenemos TODOS los renderers de las piezas (Head, Torso, etc.)
        allRenderers.Clear();
        originalMaterialsList.Clear();
        allRenderers.AddRange(target.GetComponentsInChildren<Renderer>());

        foreach (Renderer rend in allRenderers)
        {
            // Guardamos los materiales originales de esta pieza específica
            Material[] mats = rend.materials;
            Material[] savedMats = new Material[mats.Length];
            mats.CopyTo(savedMats, 0);
            originalMaterialsList.Add(savedMats);

            // Aplicamos el highlight a todos los slots de material de esta pieza
            Material[] highlightMats = new Material[mats.Length];
            for (int i = 0; i < highlightMats.Length; i++)
            {
                highlightMats[i] = highlightMaterial;
            }
            rend.materials = highlightMats;
        }

        // Efecto visual
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
        if (enemyCanvas != null) enemyCanvas.SetActive(false);

        // Restauramos los materiales originales de cada pieza
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
        if (enemyCanvas != null) enemyCanvas.SetActive(false);

        // Restauramos los materiales originales de todas las piezas detectadas
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