using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "Date", menuName = "Inventory/List", order = 1)]
public class InventoryDateBase : ScriptableObject
{
    

    [System.Serializable]
    public struct Object
    {

        public string name;
        public Sprite sprite;
        public Uso uso;
        public string characteristic;
        public string funtion;
      
    }
    public enum Uso { equipable, usable, consumable }
    

    public Object[] DateBase;
}

