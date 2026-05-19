using UnityEngine;
using System;
using System.Collections.Generic;

namespace InventoryNew
{
    public class EquipmentHandler : MonoBehaviour
    {
        // Event for UI to react to
        public event Action OnEquipChanged;

        // The 8 anatomical slots
        private Dictionary<EquipmentSlot, NewEquipmentData> equippedItems = new Dictionary<EquipmentSlot, NewEquipmentData>();

        // Current totals for quick access
        private Dictionary<StatType, float> totalModifiers = new Dictionary<StatType, float>();

        private void Awake()
        {
            EnsureInitialized();
        }

        private void EnsureInitialized()
        {
            if (equippedItems == null || equippedItems.Count == 0)
            {
                InitializeSlots();
            }

            if (totalModifiers == null || totalModifiers.Count == 0)
            {
                InitializeStats();
            }
        }

        private void InitializeSlots()
        {
            if (equippedItems == null) equippedItems = new Dictionary<EquipmentSlot, NewEquipmentData>();
            foreach (EquipmentSlot slot in Enum.GetValues(typeof(EquipmentSlot)))
            {
                equippedItems[slot] = null;
            }
        }

        private void InitializeStats()
        {
            if (totalModifiers == null) totalModifiers = new Dictionary<StatType, float>();
            foreach (StatType stat in Enum.GetValues(typeof(StatType)))
            {
                totalModifiers[stat] = 0;
            }
        }

        public void Equip(NewEquipmentData equipment)
        {
            EnsureInitialized();
            if (equipment == null) return;

            var currentItem = equippedItems[equipment.slot];

            if (NewInventoryManager.Instance != null)
            {
                if (!NewInventoryManager.Instance.TryRemoveItem(equipment.id, 1))
                {
                    Debug.LogWarning($"[EquipmentHandler] No se pudo equipar {equipment.itemName} porque no hay suficientes unidades en el inventario.");
                    return;
                }

                if (currentItem != null)
                {
                    NewInventoryManager.Instance.AddItem(currentItem, 1);
                }
            }

            equippedItems[equipment.slot] = equipment;
            RecalculateStats();
            OnEquipChanged?.Invoke();
            
           /* if (AudioManager.Instance != null && AudioManager.Instance.equipSound != null)
            {
                AudioManager.Instance.PlaySFX(AudioManager.Instance.equipSound);
            }*/
        }

        public void Unequip(EquipmentSlot slot)
        {
            EnsureInitialized();
            if (equippedItems.ContainsKey(slot) && equippedItems[slot] != null)
            {
                var item = equippedItems[slot];
                equippedItems[slot] = null;

                // Devolver al inventario
                if (NewInventoryManager.Instance != null)
                {
                    NewInventoryManager.Instance.AddItem(item, 1);
                }

                RecalculateStats();
                OnEquipChanged?.Invoke();
            }
        }

        private void RecalculateStats()
        {
            InitializeStats();

            foreach (var item in equippedItems.Values)
            {
                if (item == null) continue;

                foreach (var mod in item.modifiers)
                {
                    totalModifiers[mod.stat] += mod.amount;
                }
            }
        }

        public float GetTotalModifier(StatType stat)
        {
            EnsureInitialized();
            if (totalModifiers.ContainsKey(stat))
            {
                return totalModifiers[stat];
            }
            return 0;
        }

        public float GetModifierForSlot(EquipmentSlot slot, StatType stat)
        {
            EnsureInitialized();
            if (equippedItems.ContainsKey(slot) && equippedItems[slot] != null)
            {
                float total = 0;
                foreach (var mod in equippedItems[slot].modifiers)
                {
                    if (mod.stat == stat) total += mod.amount;
                }
                return total;
            }
            return 0;
        }

        public NewEquipmentData GetEquippedItem(EquipmentSlot slot)
        {
            EnsureInitialized();
            if (equippedItems.TryGetValue(slot, out var item))
            {
                return item;
            }
            return null;
        }

        public Dictionary<EquipmentSlot, NewEquipmentData> GetAllEquipped()
        {
            EnsureInitialized();
            return new Dictionary<EquipmentSlot, NewEquipmentData>(equippedItems);
        }

        public void ClearAllEquipped()
        {
            EnsureInitialized();
            InitializeSlots();
            RecalculateStats();
            OnEquipChanged?.Invoke();
        }

        /// <summary>
        /// Equips an item without consuming it from inventory and without unequipping/returning the previous one to inventory.
        /// Useful for loading saved states.
        /// </summary>
        public void EquipForce(NewEquipmentData equipment)
        {
            EnsureInitialized();
            if (equipment == null) return;
            equippedItems[equipment.slot] = equipment;
            RecalculateStats();
            OnEquipChanged?.Invoke();
        }
    }
}
