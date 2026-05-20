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
                Setup(GameManager.Instance.GetLeader() ?? GameManager.Instance.character1);
            }
            else if (activeFighter != null)
            {
                RefreshUI();
            }

            // Nos suscribimos al cambio de inventario para enterarnos si recogió un ítem nuevo
            if (NewInventoryManager.Instance != null)
            {
                NewInventoryManager.Instance.OnInventoryChanged += RefreshUI;
            }
        }

        // FALTABA ESTE MÉTODO PARA EVITAR ERRORES AL CERRAR EL PANEL
        private void OnDisable()
        {
            if (NewInventoryManager.Instance != null)
            {
                NewInventoryManager.Instance.OnInventoryChanged -= RefreshUI;
            }
        }

        public void SetNextFighter()
        {
            if (GameManager.Instance == null) return;
            var party = GameManager.Instance.GetPartyMembers();
            if (party.Count <= 1) return;

            int currentIndex = party.IndexOf(activeFighter);
            if (currentIndex < 0) currentIndex = 0;
            int nextIndex = (currentIndex + 1) % party.Count;
            Setup(party[nextIndex]);
        }

        public void SetPreviousFighter()
        {
            if (GameManager.Instance == null) return;
            var party = GameManager.Instance.GetPartyMembers();
            if (party.Count <= 1) return;

            int currentIndex = party.IndexOf(activeFighter);
            if (currentIndex < 0) currentIndex = 0;
            int prevIndex = (currentIndex - 1 + party.Count) % party.Count;
            Setup(party[prevIndex]);
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

            // 1. Mostrar los ítems equipados actualmente
            foreach (var slotUI in slotUIs)
            {
                if (slotUI == null) continue;
                var item = activeFighter.equipmentHandler.GetEquippedItem(slotUI.slot);
                slotUI.SetItem(item);
            }

            // 2. Controlar el feedback visual (pulso violeta) de ítems nuevos
            foreach (var slotUI in slotUIs)
            {
                if (slotUI == null) continue;

                if (NewInventoryManager.Instance != null && NewInventoryManager.Instance.HasNewItemForSlot(slotUI.slot))
                {
                    // Cambiamos el color de highlight a violeta dinámicamente y activamos el pulso
                    slotUI.highlightColor = new Color(0.6f, 0.2f, 0.8f, 1f); 
                    slotUI.SetHighlight(true);
                }
                else
                {
                    // Si no tiene ítems nuevos y no es el slot que está abierto en la selección, apagamos
                    if (pendingSlot != slotUI.slot)
                    {
                        slotUI.SetHighlight(false);
                    }
                }
            }
        }

        // NOMBRE CORREGIDO: HandleSlotClick
        private void HandleSlotClick(EquipmentSlot slot) 
        {
            pendingSlot = slot;

            // Al hacer clic en el slot del cuerpo, limpiamos el estado de "nuevo"
            if (NewInventoryManager.Instance != null)
            {
                NewInventoryManager.Instance.ClearNewStatusForSlot(slot);
            }
            
            Debug.Log($"[NewEquipmentPanelUI] HandleSlotClick llamado para: {slot}");
            ShowSelection(slot);
        }

        private void ShowSelection(EquipmentSlot slot)
        {
            Debug.Log($"[NewEquipmentPanelUI] ShowSelection para: {slot}");
            if (NewInventoryManager.Instance == null)
            {
                Debug.LogWarning("[NewEquipmentPanelUI] No hay NewInventoryManager disponible.");
                return;
            }

            selectionPanel.SetActive(true);
            
            // Highlight the compatible slot
            foreach (var slotUI in slotUIs)
            {
                if (slotUI != null)
                {
                    slotUI.SetHighlight(slotUI.slot == slot);
                }
            }
            
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
            
            // Clear highlights
            foreach (var slotUI in slotUIs)
            {
                if (slotUI != null) slotUI.SetHighlight(false);
            }

            RefreshUI();
            
            // Save state
            if (GameManager.Instance != null)
                GameManager.Instance.SavePlayerState(activeFighter);
            
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(AudioManager.Instance.equipSound);
            }
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

        public void CloseSelection()
        {
            selectionPanel.SetActive(false);
            foreach (var slotUI in slotUIs)
            {
                if (slotUI != null) slotUI.SetHighlight(false);
            }
        }
    }
}