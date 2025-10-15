using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class EnemyButtonUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public Button button;
    public Text label;
    public int index;
    public Fighter target;

    private Material originalMaterial;
    public Material highlightMaterial;
    public Material selectedMaterial;

    private static EnemyButtonUI currentlySelected; // 🔹 mantiene referencia al botón actualmente seleccionado

    private bool isSelected = false;

    public void SetText(string text) => label.text = text;
    public void SetTarget(Fighter fighter) => target = fighter;
    public void Show() => button.gameObject.SetActive(true);
    public void Hide() => button.gameObject.SetActive(false);

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (target == null || isSelected) return;

        Renderer rend = target.GetComponentInChildren<Renderer>();
        if (rend != null)
        {
            if (originalMaterial == null)
                originalMaterial = rend.material;

            rend.material = highlightMaterial;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (target == null || isSelected) return;

        Renderer rend = target.GetComponentInChildren<Renderer>();
        if (rend != null)
        {
            rend.material = originalMaterial;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (target == null) return;

        // 🔹 Deselecciona al anterior si existe
        if (currentlySelected != null && currentlySelected != this)
            currentlySelected.Deselect();

        // 🔹 Marca este como seleccionado
        Select();
        currentlySelected = this;

        // 🔹 Acá podés notificar a tu sistema de combate
        // por ejemplo:
        // CombatManager.Instance.SelectEnemy(target);
    }

    private void Select()
    {
        if (target == null) return;

        Renderer rend = target.GetComponentInChildren<Renderer>();
        if (rend != null)
        {
            if (originalMaterial == null)
                originalMaterial = rend.material;

            rend.material = selectedMaterial;
        }

        isSelected = true;
    }

    public void Deselect()
    {
        if (target == null || originalMaterial == null) return;

        Renderer rend = target.GetComponentInChildren<Renderer>();
        if (rend != null)
        {
            rend.material = originalMaterial;
        }

        isSelected = false;
    }
}
