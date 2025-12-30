using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class EnemyButtonUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public Button button;
    public Text label;
    public int index;
    public Fighter target;

    public Material originalMaterial;
    public Material highlightMaterial;
    public GameObject effectPrfb;
    private Material[] originalMaterials;
    private Renderer cachedRenderer;
    private GameObject enemyCanvas;

    private void Awake()
    {
        if (button == null)
            button = GetComponent<Button>();
        if (label == null)
            label = GetComponentInChildren<Text>();
    }

    public void SetText(string text)
    {
        label.text = text;
    }

    public void SetTarget(Fighter fighter)
    {
        target = fighter;
    }

    public void Show() => button.gameObject.SetActive(true);
    public void Hide() => button.gameObject.SetActive(false);


    public void OnPointerEnter(PointerEventData eventData)
    {
        if (target == null) return;

        enemyCanvas = target.GetComponentInChildren<Canvas>(true)?.gameObject;
        if (enemyCanvas != null)
            enemyCanvas.SetActive(true);

        cachedRenderer = target.GetComponentInChildren<Renderer>();
        if (cachedRenderer != null)
        {
            Material[] mats = cachedRenderer.materials;

            
            if (originalMaterials == null)
            {
                originalMaterials = new Material[mats.Length];
                mats.CopyTo(originalMaterials, 0);
            }

            if (mats.Length > 0)
                mats[0] = highlightMaterial;

            if (mats.Length > 1)
                mats[1] = highlightMaterial;

            cachedRenderer.materials = mats;
        }

        effectPrfb.transform.position = target.transform.position;
        effectPrfb.SetActive(true);
    }


    public void OnPointerExit(PointerEventData eventData)
    {
        HideCanvasAndReset();
    }

    
    public void OnPointerClick(PointerEventData eventData)
    {
        HideCanvasAndReset();
    }

    private void HideCanvasAndReset()
    {
        if (enemyCanvas != null)
            enemyCanvas.SetActive(false);

        if (cachedRenderer != null && originalMaterials != null)
        {
            cachedRenderer.materials = originalMaterials;
        }

        effectPrfb.SetActive(false);
    }

}
