using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
//TP2 AUGUSTO NANINI/GUSTAVO HERRERA
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
    public List<InventoryUI> pool = new List<InventoryUI>();
    public Dictionary<PlayerFighter, List<InventoryObjectID>> playerEquipped;

    void Awake()
    {

        if (InventoryManager.instance == null)
        {
            InventoryManager.instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            InventoryManager.instance.pool = new List<InventoryUI>();
            //CreateUI();
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
        Debug.Log("Add Item");
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
        Debug.Log("Start Item Manager");
        pool = new List<InventoryUI>();
        playerEquipped = new Dictionary<PlayerFighter, List<InventoryObjectID>>();

        updateUI(equipmentUI, InventoryDateBase.Uso.Equipable);
        updateUI(objetsUI, InventoryDateBase.Uso.Usable);
        updateUI(objetsUI, InventoryDateBase.Uso.SkillNeed);
        updateUI(objetsUI, InventoryDateBase.Uso.Consumable);

        var fighters = GameObject.FindObjectsOfType<PlayerFighter>();

        for (int i = 0; i < fighters.Length; i++)
        {
            playerEquipped.Add(fighters[i], new List<InventoryObjectID>());
        }

        AgregarEquipoEquipado(fighters[0], inventory[0]);
        AgregarEquipoEquipado(fighters[1], inventory[1]);
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

                //Tambien le paso las referencias de statAffected y amountAffected.
                pool[i].statAffected = datebase.DateBase[o.id].statsAffected.ToString();
                pool[i].amountAffected = datebase.DateBase[o.id].amountAffected;
                if (pool[i].gameObject != null)
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

    public void CreateUI()
    {
        var _ui = equipmentUI;

        if (inventory.Count > pool.Count)
        {
            for (int i = pool.Count; i < inventory.Count; i++)
            {
                switch (inventory[i].uso)
                {
                    case InventoryDateBase.Uso.Equipable:
                        _ui = equipmentUI;
                        break;
                    case InventoryDateBase.Uso.Usable:
                    case InventoryDateBase.Uso.SkillNeed:
                    case InventoryDateBase.Uso.Consumable:
                        _ui = objetsUI;
                        break;
                }

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


    public List<InventoryObjectID> ObtenerEquipoEquipado(PlayerFighter jugador)
    {
        if (playerEquipped.TryGetValue(jugador, out List<InventoryObjectID> equipo))
        {
            return equipo;
        }
        else
        {
            // El jugador no tiene equipo equipado
            return new List<InventoryObjectID>();
        }
    }

    public void AgregarEquipoEquipado(PlayerFighter jugador, InventoryObjectID objeto)
    {
        if (playerEquipped.ContainsKey(jugador))
        {
            playerEquipped[jugador].Add(objeto);
        }
        else
        {
            playerEquipped[jugador] = new List<InventoryObjectID> { objeto };
        }
    }

    public InventoryDateBase.Object GetItemInformation(int _id)
    {
        return datebase.DateBase[_id];
    }
}
