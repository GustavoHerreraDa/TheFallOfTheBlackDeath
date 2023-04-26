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

    public InventoryUI prefab;
    public Transform inventoryUI;

    List<InventoryUI> pool = new List<InventoryUI>();

    public void updateinventory()
    {
        for (int i = 0; i < pool.Count; i++)
        {
            if (i < inventory.Count)
            {
                InventoryObjectID o = inventory[i];
                pool[i].sprite.sprite = datebase.DateBase[o.id].sprite;
                pool[i].amount.text = o.amount.ToString();
                pool[i].gameObject.SetActive(true);
            }
            else
            {
                pool[i].gameObject.SetActive(false);
            }
            
            

        }
        if (inventory.Count > pool.Count)
        {
            for (int i = pool.Count; i < inventory.Count; i++)
            {
                InventoryUI oi = Instantiate(prefab, inventoryUI);
                pool.Add(oi);

                oi.transform.position = Vector3.zero;
                oi.transform.localScale = Vector3.one;

                InventoryObjectID o = inventory[i];
                pool[i].sprite.sprite = datebase.DateBase[o.id].sprite;
                pool[i].amount.text = o.amount.ToString();

                pool[i].gameObject.SetActive(true);
            }
        }
    }

}
