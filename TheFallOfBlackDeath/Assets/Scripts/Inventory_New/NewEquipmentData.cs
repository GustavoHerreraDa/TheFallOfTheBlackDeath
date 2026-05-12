using UnityEngine;
using System.Collections.Generic;

namespace InventoryNew
{
    [CreateAssetMenu(fileName = "New Equipment", menuName = "Inventory_New/Equipment")]
    public class NewEquipmentData : NewItemData
    {
        public EquipmentSlot slot;
        public List<StatModifier> modifiers;

        private void OnValidate()
        {
            category = ItemCategory.Equipment;
        }
    }

    [System.Serializable]
    public struct StatModifier
    {
        public StatType stat;
        public float amount;
    }
}
