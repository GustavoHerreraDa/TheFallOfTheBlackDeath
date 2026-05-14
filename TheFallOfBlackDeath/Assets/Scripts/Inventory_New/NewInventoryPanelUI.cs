using UnityEngine;
using System.Collections.Generic;
using TMPro;

namespace InventoryNew
{
    public class NewInventoryPanelUI : MonoBehaviour
    {
        [Header("References")]
        public Transform contentParent;
        public GameObject itemPrefab;
        public TMP_Text descriptionText;
        public BodyPartHealPanel bodyPartHealPanel;

        [Header("Category Selection")]
        public ItemCategory currentCategory = ItemCategory.Consumable;

        private void OnEnable()
        {
            RefreshUI();
            if (NewInventoryManager.Instance != null)
            {
                NewInventoryManager.Instance.OnInventoryChanged += RefreshUI;
            }
        }

        private void OnDisable()
        {
            if (NewInventoryManager.Instance != null)
            {
                NewInventoryManager.Instance.OnInventoryChanged -= RefreshUI;
            }
        }

        public void SetCategory(int categoryIndex)
        {
            currentCategory = (ItemCategory)categoryIndex;
            RefreshUI();
        }

        public void RefreshUI()
        {
            if (contentParent == null || itemPrefab == null) return;

            // Limpiar lista actual
            foreach (Transform child in contentParent)
            {
                Destroy(child.gameObject);
            }

            // Obtener items filtrados
            var items = NewInventoryManager.Instance.GetItemsByCategory(currentCategory);

            foreach (var invItem in items)
            {
                var go = Instantiate(itemPrefab, contentParent);
                var ui = go.GetComponent<NewInventoryItemUI>();
                
                ui.Setup(invItem);
                
                ui.OnClicked += () => HandleItemClick(invItem);
                ui.OnHover += ShowDescription;
                ui.OnHoverExit += ClearDescription;
            }
        }

        private void HandleItemClick(InventoryItem item)
        {
            if (item.data.category == ItemCategory.Consumable)
            {
                Debug.Log($"[NewInventoryPanelUI] Intentando usar consumible: {item.data.itemName}");
                
                // Si es un ítem de salud, abrimos el panel de partes del cuerpo
                if (item.data.isHealingItem)
                {
                    if (bodyPartHealPanel == null)
                    {
                        // Intento de búsqueda automática si no está asignado
                        bodyPartHealPanel = FindObjectOfType<BodyPartHealPanel>();
                    }

                    if (bodyPartHealPanel != null)
                    {
                        var target = GameManager.Instance.character1; // O el personaje activo
                        float healAmount = item.data.healAmount;
                        
                        bodyPartHealPanel.Show(target, healAmount, onPartSelected: (part) => 
                        {
                            target.ModifyBodyPartHealth(part, healAmount);
                            NewInventoryManager.Instance.RemoveItem(item.data.id, 1);
                            RefreshUI();
                            Debug.Log($"[NewInventoryPanelUI] {part} curado en {target.idName} con {healAmount} HP");
                        });
                    }
                    else
                    {
                        Debug.LogError("[NewInventoryPanelUI] No se encontró BodyPartHealPanel en la escena.");
                    }
                }
            }
            else if (item.data.category == ItemCategory.Equipment)
            {
                Debug.Log($"[NewInventoryPanelUI] El equipo se gestiona mejor desde el panel anatómico.");
            }
        }

        private void ShowDescription(NewItemData data)
        {
            if (descriptionText != null)
            {
                descriptionText.text = $"<b>{data.itemName}</b>\n{data.description}";
            }
        }

        private void ClearDescription()
        {
            if (descriptionText != null)
            {
                descriptionText.text = "";
            }
        }
    }
}
