using UnityEngine;
using System.Collections.Generic;

namespace InventoryNew
{
    [CreateAssetMenu(fileName = "New Equipment", menuName = "Inventory_New/Equipment")]
    public class NewEquipmentData : NewItemData
    {
        public EquipmentSlot slot;
        public List<StatModifier> modifiers = new List<StatModifier>(); // MODIFICADO: evita null si el equipo solo otorga skills.

        [Header("Granted Skills")]
        public List<GameObject> grantedSkillPrefabs = new List<GameObject>(); // NUEVO: prefabs de Skill que este equipo otorga al equiparse.

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
