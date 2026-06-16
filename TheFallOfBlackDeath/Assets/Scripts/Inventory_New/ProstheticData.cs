using UnityEngine;
using System.Collections.Generic;

namespace InventoryNew
{
    [CreateAssetMenu(fileName = "New Prosthetic", menuName = "Inventory_New/Prosthetic")]
    public class ProstheticData : NewEquipmentData
    {
        [Header("Prótesis")]
        public float prostheticMaxHealth = 80f;
        [Range(0f, 1f)] public float mobilityRestorePercent = 1.0f; // 1.0 = movilidad total, 0.5 = mitad
        public bool requiresDestroyedPart = true; // si true, solo se puede equipar si el slot está destruido
        
        private void OnValidate()
        {
            category = ItemCategory.Equipment;
        }
    }
}
