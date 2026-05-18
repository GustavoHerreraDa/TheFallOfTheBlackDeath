using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace InventoryNew
{
    public class NewInventoryManager : MonoBehaviour
    {
        public static NewInventoryManager Instance { get; private set; }

        public event Action OnInventoryChanged;

        [Header("Settings")]
        public List<NewItemData> masterCatalog = new List<NewItemData>();

        [SerializeField]
        private List<InventoryItem> items = new List<InventoryItem>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                ValidateCatalog();
            }
            else
            {
                // Combinar catálogos antes de destruir el duplicado
                MergeCatalogs(Instance, this);
                Destroy(gameObject);
            }
        }

        private void ValidateCatalog()
        {
            var seenIds = new HashSet<string>();
            foreach (var item in masterCatalog)
            {
                if (item == null) continue;
                if (string.IsNullOrEmpty(item.id))
                {
                    Debug.LogWarning($"[NewInventoryManager] Item en catálogo tiene ID vacío: {item.itemName}");
                    continue;
                }
                if (seenIds.Contains(item.id))
                {
                    Debug.LogWarning($"[NewInventoryManager] ID duplicado en catálogo: {item.id} ({item.itemName})");
                }
                seenIds.Add(item.id);
            }
        }

        private void MergeCatalogs(NewInventoryManager original, NewInventoryManager duplicate)
        {
            if (duplicate.masterCatalog == null || duplicate.masterCatalog.Count == 0) return;

            int addedCount = 0;
            foreach (var item in duplicate.masterCatalog)
            {
                if (item == null) continue;
                if (!original.masterCatalog.Any(i => i != null && i.id == item.id))
                {
                    original.masterCatalog.Add(item);
                    addedCount++;
                }
            }
            if (addedCount > 0)
            {
                Debug.Log($"[NewInventoryManager] Se han fusionado {addedCount} ítems nuevos al catálogo maestro desde un duplicado.");
            }
        }

        public bool TryGetItemDataById(string id, out NewItemData item)
        {
            item = GetItemDataById(id);
            return item != null;
        }

        public bool HasItem(string itemId, int amount = 1)
        {
            return GetItemCount(itemId) >= amount;
        }

        public bool TryRemoveItem(string itemId, int amount = 1)
        {
            if (HasItem(itemId, amount))
            {
                RemoveItem(itemId, amount);
                return true;
            }
            return false;
        }

        public void AddItem(NewItemData itemData, int amount = 1)
        {
            if (itemData == null) return;
            
            var existingItem = items.FirstOrDefault(i => i != null && i.data != null && i.data.id == itemData.id);

            if (existingItem != null)
            {
                existingItem.amount += amount;
            }
            else
            {
                items.Add(new InventoryItem { data = itemData, amount = amount });
            }

            OnInventoryChanged?.Invoke();
            
            // Opcional: Sonido de recogida si el AudioManager existe
            if (AudioManager.Instance != null && AudioManager.Instance.uiClickSound != null)
            {
                AudioManager.Instance.PlaySFX(AudioManager.Instance.uiClickSound, 0.7f);
            }
        }

        public void RemoveItem(string itemId, int amount = 1)
        {
            var existingItem = items.FirstOrDefault(i => i != null && i.data != null && i.data.id == itemId);

            if (existingItem != null)
            {
                existingItem.amount -= amount;
                if (existingItem.amount <= 0)
                {
                    items.Remove(existingItem);
                }
                OnInventoryChanged?.Invoke();
            }
        }

        public List<InventoryItem> GetItemsByCategory(ItemCategory category)
        {
            return items.Where(i => i != null && i.data != null && i.data.category == category).ToList();
        }

        public List<NewEquipmentData> GetEquippableForSlot(EquipmentSlot slot)
        {
            var list = items
                .Where(i => i != null && i.data is NewEquipmentData)
                .Select(i => i.data as NewEquipmentData)
                .Where(e => e.slot == slot)
                .ToList();
            
            Debug.Log($"[NewInventoryManager] Buscando ítems para slot {slot}. Encontrados: {list.Count}");
            foreach(var item in list) Debug.Log($"- Item: {item.itemName} (ID: {item.id})");
            
            return list;
        }

        public int GetItemCount(string itemId)
        {
            var item = items.FirstOrDefault(i => i != null && i.data != null && i.data.id == itemId);
            return item != null ? item.amount : 0;
        }

        public List<InventoryItem> GetAllItems() => new List<InventoryItem>(items);

        [Serializable]
        public struct InventorySaveData
        {
            public List<ItemSaveEntry> items;
        }

        [Serializable]
        public struct ItemSaveEntry
        {
            public string id;
            public int amount;
        }

        public InventorySaveData GetSaveData()
        {
            var data = new InventorySaveData
            {
                items = items
                    .Where(i => i != null && i.data != null && !string.IsNullOrEmpty(i.data.id))
                    .Select(i => new ItemSaveEntry { id = i.data.id, amount = i.amount })
                    .ToList()
            };
            return data;
        }

        public void LoadSaveData(InventorySaveData saveData)
        {
            items.Clear();
            if (saveData.items == null) return;

            foreach (var entry in saveData.items)
            {
                if (entry.amount <= 0) continue;

                if (TryGetItemDataById(entry.id, out var itemData))
                {
                    items.Add(new InventoryItem { data = itemData, amount = entry.amount });
                }
                else
                {
                    Debug.LogWarning($"[NewInventoryManager] No se pudo cargar ítem con ID: {entry.id}. No existe en el catálogo.");
                }
            }
            OnInventoryChanged?.Invoke();
        }

        public NewItemData GetItemDataById(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            return masterCatalog.FirstOrDefault(i => i != null && i.id == id);
        }
    }

    [Serializable]
    public class InventoryItem
    {
        public NewItemData data;
        public int amount;
    }
    
    
}
