using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    [System.Serializable]
    public struct InventoryObjectID
    {
        public int id;
        public int amount;

        public InventoryObjectID(int id, int amount)
        {
            this.id = id;
            this.amount = amount;
        }


        public void Addamount(int amount)
        {
            this.amount += amount;
        }


    }
    
    public InventoryDateBase datebase;
    public List<InventoryObjectID> inventory;

    

    public void AddItem(int id, int amount)
    {
        for (int i = 0; i < inventory.Count; i++)
        {
            if (inventory[i].id == id)
            {
                inventory[i].Addamount(amount);
                return;
            }
        }
        
        inventory.Add(new InventoryObjectID(id, amount));
           
    }
    public void DestroyItem(int id, int amount)
    {
        for (int i = 0; i < inventory.Count; i++)
        {
            if (inventory[i].id == id)
            {
                inventory[i].Addamount(-amount);
                if (inventory[i].amount <= 0)
                {
                    inventory.Remove(inventory[i]);
                    return;
                }
                
            }
        }
    }

    
}
