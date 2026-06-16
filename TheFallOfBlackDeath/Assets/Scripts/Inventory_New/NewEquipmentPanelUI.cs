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

        [Header("Party Selection")]
        public PartyMemberSelectorUI memberSelector;

        private PlayerFighter activeFighter;
        private EquipmentSlot pendingSlot;

        private void OnEnable()
        {
            if (memberSelector != null)
            {
                memberSelector.OnMemberSelected += Setup;

                // Sincronizar inmediatamente con el personaje seleccionado actualmente
                if (memberSelector.CurrentSelected != null)
                {
                    Setup(memberSelector.CurrentSelected);
                }
            }

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
            if (memberSelector != null)
            {
                memberSelector.OnMemberSelected -= Setup;
            }

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
            
            // Actualizar selector visual si existe
            if (memberSelector != null)
            {
                memberSelector.SelectMember(party[nextIndex]);
            }
            else
            {
                Setup(party[nextIndex]);
            }
        }

        public void SetPreviousFighter()
        {
            if (GameManager.Instance == null) return;
            var party = GameManager.Instance.GetPartyMembers();
            if (party.Count <= 1) return;

            int currentIndex = party.IndexOf(activeFighter);
            if (currentIndex < 0) currentIndex = 0;
            int prevIndex = (currentIndex - 1 + party.Count) % party.Count;
            
            // Actualizar selector visual si existe
            if (memberSelector != null)
            {
                memberSelector.SelectMember(party[prevIndex]);
            }
            else
            {
                Setup(party[prevIndex]);
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
                slotUI.OnSlotRightClicked -= HandleSlotRightClick;
                slotUI.OnSlotHover -= ShowStats;
                slotUI.OnSlotHoverExit -= HideStats;
            }

            activeFighter = fighter;
            RefreshUI();

            foreach (var slotUI in slotUIs)
            {
                if (slotUI == null) continue;
                slotUI.OnSlotClicked += HandleSlotClick;
                slotUI.OnSlotRightClicked += HandleSlotRightClick;
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
            if (NewInventoryManager.Instance == null) return;

            selectionPanel.SetActive(true);
            foreach (var slotUI in slotUIs)
                if (slotUI != null) slotUI.SetHighlight(slotUI.slot == slot);

            foreach (Transform child in selectionContent)
                Destroy(child.gameObject);

            // Determinar si el body part está destruido
            BodyPart bodyPart = Fighter.EquipmentSlotToBodyPart(slot);
            Fighter.BodyPartData partData = activeFighter?.GetBodyPart(bodyPart);
            bool partIsDestroyed = partData != null && partData.IsDestroyed;

            var allCompatible = NewInventoryManager.Instance.GetEquippableForSlot(slot);

            bool anyItemShown = false;
            foreach (var item in allCompatible)
            {
                bool isProsthetic = item is ProstheticData pd && pd.requiresDestroyedPart;
                // Prótesis: solo si la parte está destruida
                // Item normal: solo si la parte NO está destruida
                bool shouldShow = isProsthetic ? partIsDestroyed : !partIsDestroyed;

                if (!shouldShow) continue;

                var go = Instantiate(itemPrefab, selectionContent);
                var ui = go.GetComponent<NewInventoryItemUI>();
                ui.Setup(item);
                ui.OnClicked += () => EquipItem(item);
                ui.OnHover += ShowPreview;
                ui.OnHoverExit += HideStats;
                anyItemShown = true;
            }

            // Mensaje si no hay items disponibles
            if (!anyItemShown && statsText != null)
            {
                statsText.text = partIsDestroyed
                    ? "Este slot necesita una prótesis.\nNo tienes ninguna disponible."
                    : "No tienes equipamiento para este slot.";
            }
        }

        private void HandleSlotRightClick(EquipmentSlot slot)
        {
            if (activeFighter?.equipmentHandler == null) return;

            var equipped = activeFighter.equipmentHandler.GetEquippedItem(slot);
            if (equipped == null) return;

            // No permitir desequipar una prótesis que esté en una parte destruida a menos que se reemplace
            if (equipped is ProstheticData)
            {
                BodyPart bodyPart = Fighter.EquipmentSlotToBodyPart(slot);
                var partData = activeFighter.GetBodyPart(bodyPart);
                if (partData != null && partData.IsDestroyed)
                {
                    // Opcional: mostrar un aviso en lugar de bloquear silenciosamente
                    if (statsText != null)
                        statsText.text = "No puedes quitar una prótesis sin reemplazarla primero.";
                    Debug.LogWarning($"[NewEquipmentPanelUI] Intento de quitar prótesis de parte destruida: {slot}");
                    return;
                }
            }

            activeFighter.equipmentHandler.Unequip(slot);
            RefreshUI();

            if (GameManager.Instance != null)
                GameManager.Instance.SavePlayerState(activeFighter);
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