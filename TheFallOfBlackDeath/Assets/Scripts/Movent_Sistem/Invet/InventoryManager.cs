using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public enum ItemInventoryId
{
    Potion,
    Scimitar,
    Helmet,
    LeatherArmor,
    StealthBoot,
    Molotov,
    VoodooDoll,
    Key,
    StrengthPotion
}
public class InventoryManager : MonoBehaviour
{
    public static InventoryManager instance;
    public InventoryDateBase datebase;
    public List<InventoryObjectID> inventory;
    public InventoryUI prefab;
    public Transform equipmentUI;
    public Transform objetsUI;
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
        public InventoryDateBase.Uso uso;

        public InventoryObjectID(int id, int amount, InventoryDateBase.Uso uso)
        {
            this.id = id;
            this.amount = amount;
            this.uso = uso;
        }
    }
    public void AddItem(int id, int amount, InventoryDateBase.Uso uso)
    {
        for (int i = 0; i < inventory.Count; i++)
        {
            if (inventory[i].id == id)
            {
                inventory[i] = new InventoryObjectID(inventory[i].id, inventory[i].amount + amount, uso);
                updateUI(equipmentUI, InventoryDateBase.Uso.Equipable);
                updateUI(objetsUI, InventoryDateBase.Uso.Usable);

                updateUI(objetsUI, InventoryDateBase.Uso.SkillNeed);
                updateUI(objetsUI, InventoryDateBase.Uso.Consumable);
                return;
            }
        }
        inventory.Add(new InventoryObjectID(id, amount, uso));
        updateUI(equipmentUI, InventoryDateBase.Uso.Equipable);
        updateUI(objetsUI, InventoryDateBase.Uso.Usable);
        updateUI(objetsUI, InventoryDateBase.Uso.SkillNeed);
        updateUI(objetsUI, InventoryDateBase.Uso.Consumable);
    }
    public void DestroyItem(int id, int amount, InventoryDateBase.Uso uso)
    {
        for (int i = 0; i < inventory.Count; i++)
        {
            if (inventory[i].id == id)
            {
                inventory[i] = new InventoryObjectID(inventory[i].id, inventory[i].amount - amount, uso); ;
                if (inventory[i].amount <= 0)
                {
                    inventory.Remove(inventory[i]);
                    updateUI(equipmentUI, InventoryDateBase.Uso.Equipable);
                    updateUI(objetsUI, InventoryDateBase.Uso.Usable);
                    updateUI(objetsUI, InventoryDateBase.Uso.SkillNeed);
                    updateUI(objetsUI, InventoryDateBase.Uso.Consumable);
                    return;
                }
            }
        }
    }
    public void Start()
    {
        updateUI(equipmentUI, InventoryDateBase.Uso.Equipable);
        updateUI(objetsUI, InventoryDateBase.Uso.Usable);
        updateUI(objetsUI, InventoryDateBase.Uso.SkillNeed);
        updateUI(objetsUI, InventoryDateBase.Uso.Consumable);
    }
    public void updateUI(Transform _ui, InventoryDateBase.Uso uso)
    {
        //Debug.Log("updateinventory funciono");
        for (int i = 0; i < pool.Count; i++)
        {
            if (i < inventory.Count)
            {
                InventoryObjectID o = inventory[i];

                //if (datebase.DateBase[o.id].uso != uso)
                //    return;

                pool[i].sprite.sprite = datebase.DateBase[o.id].sprite;
                pool[i].amount.text = o.amount.ToString();
                pool[i].itemName.text = datebase.DateBase[o.id].name;
                pool[i].itemDescripcion.text = datebase.DateBase[o.id].characteristic;
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
                if (inventory[i].uso != uso)
                    return;

                InventoryUI oi = Instantiate(prefab, _ui);
                pool.Add(oi);

                oi.transform.position = Vector3.zero;
                oi.transform.localScale = Vector3.one;

                InventoryObjectID o = inventory[i];
                pool[i].sprite.sprite = datebase.DateBase[o.id].sprite;
                pool[i].itemName.text = datebase.DateBase[o.id].name;
                pool[i].itemDescripcion.text = datebase.DateBase[o.id].characteristic;
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

        foreach (var itemNeed in itemsNeeded)
        {
            var itemInventory = inventory.Where(x => x.id == itemNeed.id).FirstOrDefault();

            if (itemInventory.amount >= itemNeed.amount)
                hasItemInIventory = true;
        }

        return hasItemInIventory;
    }

    public bool HasItemInIventory(int _id, int _amount)
    {

        var hasItemInIventory = false;


        var itemInventory = inventory.Where(x => x.id == _id).FirstOrDefault();

        if (itemInventory.amount >= _amount)
            hasItemInIventory = true;


        return hasItemInIventory;
    }
}
