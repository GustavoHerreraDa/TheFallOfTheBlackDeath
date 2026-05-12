using UnityEngine;
using InventoryNew;

namespace InventoryNew
{
    [CreateAssetMenu(fileName = "New Item", menuName = "Inventory_New/Item")]
    public class NewItemData : ScriptableObject
    {
        public string id; // Unique ID
        public string itemName;
        [TextArea]
        public string description;
        public Sprite icon;
        public ItemCategory category;
        public int maxStack = 99;
    }
}
