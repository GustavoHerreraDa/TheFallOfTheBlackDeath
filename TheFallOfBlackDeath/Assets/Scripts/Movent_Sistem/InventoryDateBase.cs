using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "Date", menuName = "Inventory/List", order = 1)]
public class InventoryDateBase : ScriptableObject
{
    public Object[] DateBase;

    [System.Serializable]
    public struct Object
    {

        public string name;
        public Sprite sprite;

        public enum uso
        {
            usable, equippable, consumable
        }

        public string caractercharacteristics;
    }
}    
