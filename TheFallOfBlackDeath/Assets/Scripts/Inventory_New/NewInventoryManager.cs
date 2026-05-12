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
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void AddItem(NewItemData itemData, int amount = 1)
        {
            var existingItem = items.FirstOrDefault(i => i.data.id == itemData.id);

            if (existingItem != null)
            {
                existingItem.amount += amount;
            }
            else
            {
                items.Add(new InventoryItem { data = itemData, amount = amount });
            }

            OnInventoryChanged?.Invoke();
        }

        public void RemoveItem(string itemId, int amount = 1)
        {
            var existingItem = items.FirstOrDefault(i => i.data.id == itemId);

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
            return items.Where(i => i.data.category == category).ToList();
        }

        public List<NewEquipmentData> GetEquippableForSlot(EquipmentSlot slot)
        {
            return items
                .Where(i => i.data is NewEquipmentData)
                .Select(i => i.data as NewEquipmentData)
                .Where(e => e.slot == slot)
                .ToList();
        }

        public int GetItemCount(string itemId)
        {
            var item = items.FirstOrDefault(i => i.data.id == itemId);
            return item != null ? item.amount : 0;
        }

        public List<InventoryItem> GetAllItems() => new List<InventoryItem>(items);

        public NewItemData GetItemDataById(string id)
        {
            return masterCatalog.FirstOrDefault(i => i.id == id);
        }
    }

    [Serializable]
    public class InventoryItem
    {
        public NewItemData data;
        public int amount;
    }
}
