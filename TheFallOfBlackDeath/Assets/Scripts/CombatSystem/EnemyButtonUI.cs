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

        Renderer rend = target.GetComponentInChildren<Renderer>();
        if (rend != null)
        {
            originalMaterial = rend.material;
            rend.material = highlightMaterial;
        }
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

        if (target == null || originalMaterial == null) return;

        Renderer rend = target.GetComponentInChildren<Renderer>();
        if (rend != null)
            rend.material = originalMaterial;
    }
}
