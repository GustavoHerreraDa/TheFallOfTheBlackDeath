using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System;

namespace InventoryNew
{
    public class NewInventoryItemUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        public Image icon;
        public TMP_Text nameText;
        public TMP_Text amountText;

        public event Action OnClicked;
        public event Action<NewItemData> OnHover;
        public event Action OnHoverExit;

        private NewItemData data;

        public void Setup(NewItemData itemData, int amount = -1)
        {
            data = itemData;
            if (itemData != null)
            {
                if (icon != null) icon.sprite = itemData.icon;
                if (nameText != null) nameText.text = itemData.itemName;
            }
            
            if (amountText != null)
            {
                if (amount >= 0)
                {
                    amountText.text = $"x{amount}";
                    amountText.gameObject.SetActive(true);
                }
                else
                {
                    amountText.gameObject.SetActive(false);
                }
            }
        }

        public void Setup(InventoryItem item)
        {
            Setup(item.data, item.amount);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            OnClicked?.Invoke();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            OnHover?.Invoke(data);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            OnHoverExit?.Invoke();
        }
    }
}
