using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Selectable))]
public class UIButtonSound : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    public void OnPointerEnter(PointerEventData eventData)
    {
        // Solo suena si el botón es interactuable
        if (GetComponent<Selectable>().interactable && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.uiHoverSound, 0.5f, false);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (GetComponent<Selectable>().interactable && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.uiClickSound, 0.8f, false);
        }
    }
}