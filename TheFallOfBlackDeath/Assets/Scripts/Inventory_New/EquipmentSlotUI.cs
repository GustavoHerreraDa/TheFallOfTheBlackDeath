﻿using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System;
using System.Collections;

namespace InventoryNew
{
    public class EquipmentSlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        public EquipmentSlot slot;
        public Image icon;
        public TMP_Text slotName;
        
        [Header("Highlight Settings")]
        public Color highlightColor = Color.yellow;
        public float pulseScale = 1.1f;
        public float pulseSpeed = 2.0f;

        public event Action<EquipmentSlot> OnSlotClicked;
        public event Action<EquipmentSlot> OnSlotRightClicked;
        public event Action<NewEquipmentData> OnSlotHover;
        public event Action OnSlotHoverExit;

        private NewEquipmentData currentItem;
        private Color originalColor;
        private Coroutine pulseCoroutine;
        private Image slotImage;

        private void Awake()
        {
            slotImage = GetComponent<Image>();
            if (slotImage != null)
            {
                originalColor = slotImage.color;
            }
        }

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

        public void SetHighlight(bool active)
        {
            if (active)
            {
                if (slotImage != null) slotImage.color = highlightColor;
                if (pulseCoroutine == null)
                {
                    pulseCoroutine = StartCoroutine(PulseRoutine());
                }
            }
            else
            {
                if (pulseCoroutine != null)
                {
                    StopCoroutine(pulseCoroutine);
                    pulseCoroutine = null;
                }
                transform.localScale = Vector3.one;
                if (slotImage != null) slotImage.color = originalColor;
            }
        }

        private IEnumerator PulseRoutine()
        {
            while (true)
            {
                float t = (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f;
                float currentScale = Mathf.Lerp(1.0f, pulseScale, t);
                transform.localScale = new Vector3(currentScale, currentScale, 1.0f);
                yield return null;
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                Debug.Log($"[EquipmentSlotUI] Click izquierdo en slot: {slot}");
                OnSlotClicked?.Invoke(slot);
            }
            else if (eventData.button == PointerEventData.InputButton.Right)
            {
                Debug.Log($"[EquipmentSlotUI] Click derecho en slot: {slot} → desequipar");
                OnSlotRightClicked?.Invoke(slot);
            }
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