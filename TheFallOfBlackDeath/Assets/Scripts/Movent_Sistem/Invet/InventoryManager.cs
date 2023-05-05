using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager instance;
    public InventoryDateBase datebase;
    public List<InventoryObjectID> inventory;
    public InventoryUI prefab;
    public Transform inventoryUI;
    List<InventoryUI> pool = new List<InventoryUI>();

    void Awake()
    {
        if (InventoryManager.instance == null)
        {
            InventoryManager.instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }



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
    }
    public void AddItem(int id, int amount)
    {
        for (int i = 0; i < inventory.Count; i++)
        {
            if (inventory[i].id == id)
            {
                inventory[i] = new InventoryObjectID(inventory[i].id, inventory[i].amount + amount);
                updateinventory();
                return;
            }
        }
        inventory.Add(new InventoryObjectID(id, amount));
        updateinventory();
    }
    public void DestroyItem(int id, int amount)
    {
        for (int i = 0; i < inventory.Count; i++)
        {
            if (inventory[i].id == id)
            {
                inventory[i] = new InventoryObjectID(inventory[i].id, inventory[i].amount - amount); ;
                if (inventory[i].amount <= 0)
                {
                    inventory.Remove(inventory[i]);
                    updateinventory();
                    return;
                }
            }
        }
    }
    public void Start()
    {
        updateinventory();
    }
    public void updateinventory()
    {
        Debug.Log("updateinventory funciono");
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
    public bool HasItemInIventory(List<InventoryObjectID> itemsNeeded)
    {

        if (itemsNeeded.Count == 0)
            return true;

        var hasItemInIventory = false;

        foreach (var itemNeed in itemsNeeded )
        {
            var itemInventory = inventory.Where(x => x.id == itemNeed.id).FirstOrDefault();

            if (itemInventory.amount >= itemNeed.amount)
                hasItemInIventory = true;
        }

        return hasItemInIventory;
    }

}
