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
            InitializeSlots();
            InitializeStats();
        }

        private void InitializeSlots()
        {
            foreach (EquipmentSlot slot in Enum.GetValues(typeof(EquipmentSlot)))
            {
                equippedItems[slot] = null;
            }
        }

        private void InitializeStats()
        {
            foreach (StatType stat in Enum.GetValues(typeof(StatType)))
            {
                totalModifiers[stat] = 0;
            }
        }

        public void Equip(NewEquipmentData equipment)
        {
            if (equipment == null) return;

            // Unequip current item in that slot
            Unequip(equipment.slot);

            // Equip new item
            equippedItems[equipment.slot] = equipment;
            
            RecalculateStats();
            OnEquipChanged?.Invoke();
        }

        public void Unequip(EquipmentSlot slot)
        {
            if (equippedItems[slot] != null)
            {
                equippedItems[slot] = null;
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
            if (totalModifiers.ContainsKey(stat))
            {
                return totalModifiers[stat];
            }
            return 0;
        }

        public NewEquipmentData GetEquippedItem(EquipmentSlot slot)
        {
            return equippedItems[slot];
        }

        public Dictionary<EquipmentSlot, NewEquipmentData> GetAllEquipped()
        {
            return new Dictionary<EquipmentSlot, NewEquipmentData>(equippedItems);
        }
    }
}
