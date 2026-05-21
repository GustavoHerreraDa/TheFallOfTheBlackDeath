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

        private PlayerFighter activeTarget;

        public void SetActiveTarget(PlayerFighter target)
        {
            activeTarget = target;
            Debug.Log($"[NewInventoryPanelUI] Objetivo activo cambiado a: {(target != null ? target.idName : "Ninguno")}");
        }

        private void OnEnable()
        {
            // ... tu lógica actual de target ...
            StartCoroutine(EnsureManagerReady());
        }

        private System.Collections.IEnumerator EnsureManagerReady()
        {
            // Esperar hasta que el manager esté listo
            while (NewInventoryManager.Instance == null)
            {
                yield return null; // Espera un frame
            }

            // Una vez que sale del bucle, el manager es seguro
            NewInventoryManager.Instance.OnInventoryChanged += RefreshUI;
            RefreshUI();
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
            if (NewInventoryManager.Instance == null)
            {
                Debug.LogWarning("[NewInventoryPanelUI] NewInventoryManager.Instance es null. Esperando...");
                return;
            }

            // Limpiar lista actual
            foreach (Transform child in contentParent)
            {
                if (child != null) Destroy(child.gameObject);
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
            if (item == null || item.data == null) return;

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
                        if (GameManager.Instance == null)
                        {
                            Debug.LogError("[NewInventoryPanelUI] No se encontro GameManager para elegir objetivo.");
                            return;
                        }

                        var target = activeTarget != null ? activeTarget : (GameManager.Instance.GetLeader() ?? GameManager.Instance.character1);
                        if (target == null)
                        {
                            Debug.LogError("[NewInventoryPanelUI] No hay personaje activo para curar.");
                            return;
                        }

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
