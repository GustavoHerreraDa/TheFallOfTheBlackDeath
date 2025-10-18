using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class EnemyButtonUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Button button;
    public Text label;
    public int index;
    public Fighter target;

    public Material originalMaterial;
    public Material highlightMaterial;

    public EnemyButtonUI(GameObject buttonObject, int idx)
    {
        button = buttonObject.GetComponent<Button>();
        label = buttonObject.GetComponentInChildren<Text>();
        index = idx;
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

        Renderer rend = target.GetComponentInChildren<Renderer>();
        if (rend != null)
        {
            originalMaterial = rend.material;
            rend.material = highlightMaterial;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (target == null || originalMaterial == null) return;

        Renderer rend = target.GetComponentInChildren<Renderer>();
        if (rend != null)
        {
            rend.material = originalMaterial;
        }
    }
}
