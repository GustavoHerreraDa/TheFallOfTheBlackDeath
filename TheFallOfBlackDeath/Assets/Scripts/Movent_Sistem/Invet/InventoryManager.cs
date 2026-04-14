using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
//chumba
/// <summary>
/// Defines the named values used by item inventory id.
/// </summary>
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
/// <summary>
/// Maintains the runtime inventory, equipped items, and inventory-driven UI refreshes for the current play session.
/// </summary>
public class InventoryManager : MonoBehaviour
{
    // Observer events for decoupled UI updates
    public static event Action OnInventoryChanged;
    public static event Action<PlayerFighter> OnCharacterChanged;
    /// <summary>
    /// Executes the notify character changed workflow.
    /// </summary>
    /// <param name="fighter">The fighter.</param>
    public static void NotifyCharacterChanged(PlayerFighter fighter)
    {
        OnCharacterChanged?.Invoke(fighter);
    }
    public static InventoryManager instance;
    public InventoryDateBase datebase;
    public List<InventoryObjectID> inventory;
    public InventoryUI prefab;
    public Transform equipmentUI;
    public Transform objetsUI;
    public List<InventoryUI> pool = new List<InventoryUI>();
    public Dictionary<PlayerFighter, List<InventoryObjectID>> playerEquipped;

    // Cache for item data to avoid repeated array indexing and string allocations
    private Dictionary<int, InventoryDateBase.Object> _itemCache;

    
    /// <summary>
    /// Registers runtime listeners when the component becomes active.
    /// </summary>
    void OnEnable()
    {
        DialogueManager.OnGiveItem += HandleGiveItem;
    }

    /// <summary>
    /// Unregisters runtime listeners when the component becomes inactive.
    /// </summary>
    void OnDisable()
    {
        DialogueManager.OnGiveItem -= HandleGiveItem;
    }
    
    /// <summary>
    /// Initializes cached references and runtime state before the component starts running.
    /// </summary>
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
        }


        _itemCache = new Dictionary<int, InventoryDateBase.Object>();
        if (datebase != null && datebase.DateBase != null)
        {
            for (int i = 0; i < datebase.DateBase.Length; i++)
            {
                _itemCache[i] = datebase.DateBase[i];
            }
        }
    }


    [System.Serializable]
    /// <summary>
    /// Represents an inventory entry that tracks an item identifier, amount, and usage category.
    /// </summary>
    public struct InventoryObjectID
    {
        public int id;
        public int amount;
        public InventoryDateBase.Uso uso;

        /// <summary>
        /// Initializes a new instance of the <see cref="InventoryObjectID"/> class.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="amount">The amount.</param>
        /// <param name="uso">The uso.</param>
        public InventoryObjectID(int id, int amount, InventoryDateBase.Uso uso)
        {
            this.id = id;
            this.amount = amount;
            this.uso = uso;
        }
    }
    /// <summary>
    /// Adds the item.
    /// </summary>
    /// <param name="id">The id.</param>
    /// <param name="amount">The amount.</param>
    /// <param name="uso">The uso.</param>
    public void AddItem(int id, int amount, InventoryDateBase.Uso uso)
    {
        Debug.Log("Add Item");
        if (inventory == null)
            inventory = new List<InventoryObjectID>();

        for (int i = 0; i < inventory.Count; i++)
        {
            if (inventory[i].id == id)
            {
                inventory[i] = new InventoryObjectID(inventory[i].id, inventory[i].amount + amount, uso);
                updateUI(equipmentUI, InventoryDateBase.Uso.Equipable);
                updateUI(objetsUI, InventoryDateBase.Uso.Usable);
                updateUI(objetsUI, InventoryDateBase.Uso.SkillNeed);
                updateUI(objetsUI, InventoryDateBase.Uso.Consumable);
                OnInventoryChanged?.Invoke();
                return;
            }
        }
        inventory.Add(new InventoryObjectID(id, amount, uso));
        updateUI(equipmentUI, InventoryDateBase.Uso.Equipable);
        updateUI(objetsUI, InventoryDateBase.Uso.Usable);
        updateUI(objetsUI, InventoryDateBase.Uso.SkillNeed);
        updateUI(objetsUI, InventoryDateBase.Uso.Consumable);
        OnInventoryChanged?.Invoke();
    }
    /// <summary>
    /// Executes the destroy item workflow.
    /// </summary>
    /// <param name="id">The id.</param>
    /// <param name="amount">The amount.</param>
    /// <param name="uso">The uso.</param>
    public void DestroyItem(int id, int amount, InventoryDateBase.Uso uso)
    {
        if (inventory == null) return;
        for (int i = 0; i < inventory.Count; i++)
        {
            if (inventory[i].id == id)
            {
                inventory[i] = new InventoryObjectID(inventory[i].id, inventory[i].amount - amount, uso);
                if (inventory[i].amount <= 0)
                {
                    inventory.Remove(inventory[i]);
                }
                updateUI(equipmentUI, InventoryDateBase.Uso.Equipable);
                updateUI(objetsUI, InventoryDateBase.Uso.Usable);
                updateUI(objetsUI, InventoryDateBase.Uso.SkillNeed);
                updateUI(objetsUI, InventoryDateBase.Uso.Consumable);
                OnInventoryChanged?.Invoke();
                return;
            }
        }
    }
    /// <summary>
    /// Initializes the component once the scene dependencies are ready.
    /// </summary>
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

        //AgregarEquipoEquipado(fighters[0], inventory[0]);
        //AgregarEquipoEquipado(fighters[1], inventory[1]);

    }

    /// <summary>
    /// Updates the ui.
    /// </summary>
    /// <param name="_ui">The ui.</param>
    /// <param name="uso">The uso.</param>
    public void updateUI(Transform _ui, InventoryDateBase.Uso uso)
    {
        if (_ui == null || prefab == null || datebase == null)
            return;

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

    /// <summary>
    /// Creates the ui.
    /// </summary>
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
    /// <summary>
    /// Determines whether the component has item in iventory.
    /// </summary>
    /// <param name="itemsNeeded">The items needed.</param>
    /// <returns>True when the requested condition is met; otherwise, false.</returns>
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

    /// <summary>
    /// Determines whether the component has item in iventory.
    /// </summary>
    /// <param name="_id">The id.</param>
    /// <param name="_amount">The amount.</param>
    /// <returns>True when the requested condition is met; otherwise, false.</returns>
    public bool HasItemInIventory(int _id, int _amount)
    {

        var hasItemInIventory = false;


        var itemInventory = inventory.Where(x => x.id == _id).FirstOrDefault();

        if (itemInventory.amount >= _amount)
            hasItemInIventory = true;


        return hasItemInIventory;
    }


    /// <summary>
    /// Executes the obtener equipo equipado workflow.
    /// </summary>
    /// <param name="jugador">The jugador.</param>
    /// <returns>The resulting collection.</returns>
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

    /// <summary>
    /// Executes the agregar equipo equipado workflow.
    /// </summary>
    /// <param name="jugador">The jugador.</param>
    /// <param name="objeto">The objeto.</param>
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

    /// <summary>
    /// Attempts to get the item data.
    /// </summary>
    /// <param name="id">The id.</param>
    /// <param name="data">The data.</param>
    /// <returns>True when the requested condition is met; otherwise, false.</returns>
    public bool TryGetItemData(int id, out InventoryDateBase.Object data)
    {
        data = default;
        if (_itemCache != null && _itemCache.TryGetValue(id, out data))
            return true;
        if (datebase != null && datebase.DateBase != null && id >= 0 && id < datebase.DateBase.Length)
        {
            data = datebase.DateBase[id];
            return true;
        }
        return false;
    }

    /// <summary>
    /// Gets the item information.
    /// </summary>
    /// <param name="_id">The id.</param>
    /// <returns>The resulting value.</returns>
    public InventoryDateBase.Object GetItemInformation(int _id)
    {
        if (TryGetItemData(_id, out var data))
            return data;
        Debug.LogWarning($"GetItemInformation: invalid id {_id}");
        return default;
    }
    
    /// <summary>
    /// Handles the give item.
    /// </summary>
    /// <param name="id">The id.</param>
    /// <param name="amount">The amount.</param>
    /// <param name="uso">The uso.</param>
    private void HandleGiveItem(int id, int amount, InventoryDateBase.Uso uso)
    {
        Debug.Log($"InventoryManager: Recibido item {id} x{amount}");
        
        // Llamamos a tu método existente AddItem
        
        AddItem(id, amount, uso);
        
        // Opcional: Mostrar feedback visual en pantalla tipo "¡Has conseguido una Poción!"
    }
    
}
