using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using TMPro;

namespace InventoryNew
{
    public class NewEquipmentPanelUI : MonoBehaviour
    {
        [Header("Slots")]
        public List<EquipmentSlotUI> slotUIs;

        [Header("Item Selection")]
        public GameObject selectionPanel;
        public Transform selectionContent;
        public GameObject itemPrefab;

        [Header("Stats Preview")]
        public TMP_Text statsText;

        private PlayerFighter activeFighter;
        private EquipmentSlot pendingSlot;

        private void OnEnable()
        {
            if (activeFighter == null && GameManager.Instance != null)
            {
                Setup(GameManager.Instance.character1);
            }
            else if (activeFighter != null)
            {
                RefreshUI();
            }
        }

        public void Setup(PlayerFighter fighter)
        {
            if (fighter == null) return;
            
            // Limpiar suscripciones previas si las hubiera para evitar duplicados
            foreach (var slotUI in slotUIs)
            {
                if (slotUI == null) continue;
                slotUI.OnSlotClicked -= HandleSlotClick;
                slotUI.OnSlotHover -= ShowStats;
                slotUI.OnSlotHoverExit -= HideStats;
            }

            activeFighter = fighter;
            RefreshUI();

            foreach (var slotUI in slotUIs)
            {
                if (slotUI == null) continue;
                slotUI.OnSlotClicked += HandleSlotClick;
                slotUI.OnSlotHover += ShowStats;
                slotUI.OnSlotHoverExit += HideStats;
            }
        }

        public void RefreshUI()
        {
            if (activeFighter == null || activeFighter.equipmentHandler == null) return;

            foreach (var slotUI in slotUIs)
            {
                var item = activeFighter.equipmentHandler.GetEquippedItem(slotUI.slot);
                slotUI.SetItem(item);
            }
        }

        private void HandleSlotClick(EquipmentSlot slot)
        {
            Debug.Log($"[NewEquipmentPanelUI] HandleSlotClick llamado para: {slot}");
            pendingSlot = slot;
            ShowSelection(slot);
        }

        private void ShowSelection(EquipmentSlot slot)
        {
            Debug.Log($"[NewEquipmentPanelUI] ShowSelection para: {slot}");
            selectionPanel.SetActive(true);
            
            // Clear previous items
            foreach (Transform child in selectionContent)
            {
                Destroy(child.gameObject);
            }

            // Get compatible items from inventory
            var compatibleItems = NewInventoryManager.Instance.GetEquippableForSlot(slot);

            foreach (var item in compatibleItems)
            {
                var go = Instantiate(itemPrefab, selectionContent);
                var ui = go.GetComponent<NewInventoryItemUI>();
                ui.Setup(item);
                ui.OnClicked += () => EquipItem(item);
                ui.OnHover += ShowPreview;
                ui.OnHoverExit += HideStats;
            }
        }

        private void EquipItem(NewEquipmentData item)
        {
            activeFighter.equipmentHandler.Equip(item);
            selectionPanel.SetActive(false);
            RefreshUI();
            
            // Save state
            if (GameManager.Instance != null)
                GameManager.Instance.SavePlayerState(activeFighter);
        }

        private void ShowStats(NewEquipmentData item)
        {
            if (item == null)
            {
                statsText.text = "Slot vacío";
                return;
            }

            string text = $"{item.itemName}\n";
            foreach (var mod in item.modifiers)
            {
                string sign = mod.amount >= 0 ? "+" : "";
                text += $"{mod.stat}: {sign}{mod.amount}\n";
            }
            statsText.text = text;
        }

        private void ShowPreview(NewItemData itemData)
        {
            if (!(itemData is NewEquipmentData equipment)) return;

            var currentItem = activeFighter.equipmentHandler.GetEquippedItem(equipment.slot);
            
            string text = $"PREVIEW: {equipment.itemName}\n";
            
            // Simple preview logic: compare modifiers
            // In a more advanced version, we could show: Attack: 10 -> 15 (+5)
            foreach (StatType stat in System.Enum.GetValues(typeof(StatType)))
            {
                float currentMod = 0;
                if (currentItem != null)
                {
                    currentMod = currentItem.modifiers.FirstOrDefault(m => m.stat == stat).amount;
                }

                float newMod = equipment.modifiers.FirstOrDefault(m => m.stat == stat).amount;
                
                if (currentMod != 0 || newMod != 0)
                {
                    float diff = newMod - currentMod;
                    string sign = diff >= 0 ? "+" : "";
                    
                    text += $"{stat}: {sign}{diff}\n";
                }
            }
            
            statsText.text = text;
        }

        private void HideStats()
        {
            statsText.text = "";
        }
    }
}
