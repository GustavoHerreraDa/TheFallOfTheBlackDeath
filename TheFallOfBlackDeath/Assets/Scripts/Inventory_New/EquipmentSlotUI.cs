using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System;

namespace InventoryNew
{
    public class EquipmentSlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        public EquipmentSlot slot;
        public Image icon;
        public TMP_Text slotName;
        
        public event Action<EquipmentSlot> OnSlotClicked;
        public event Action<NewEquipmentData> OnSlotHover;
        public event Action OnSlotHoverExit;

        private NewEquipmentData currentItem;

        public void SetItem(NewEquipmentData item)
        {
            currentItem = item;
            if (item != null)
            {
                icon.sprite = item.icon;
                icon.enabled = true;
            }
            else
            {
                icon.enabled = false;
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            Debug.Log($"[EquipmentSlotUI] Click en slot: {slot}");
            OnSlotClicked?.Invoke(slot);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            OnSlotHover?.Invoke(currentItem);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            OnSlotHoverExit?.Invoke();
        }
    }
}
