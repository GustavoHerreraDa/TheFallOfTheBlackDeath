using System;
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
    // ── Eventos ──────────────────────────────────────────────────────────────
    public static event Action OnInventoryChanged;
    public static event Action<PlayerFighter> OnCharacterChanged;

    public static void NotifyCharacterChanged(PlayerFighter fighter)
    {
        OnCharacterChanged?.Invoke(fighter);
    }

    // ── Singleton ─────────────────────────────────────────────────────────────
    public static InventoryManager instance;

    // ── Datos ─────────────────────────────────────────────────────────────────
    public InventoryDateBase datebase;
    public List<InventoryObjectID> inventory = new List<InventoryObjectID>();

    // ── UI ────────────────────────────────────────────────────────────────────
    public InventoryUI prefab;
    public Transform equipmentUI;
    public Transform objetsUI;

    // Pool separado por tipo para no mezclar equipables con usables
    private List<InventoryUI> equipmentPool = new List<InventoryUI>();
    private List<InventoryUI> objectsPool   = new List<InventoryUI>();

    // ── Equipamiento persistente ──────────────────────────────────────────────
    // Se ha movido a cada PlayerFighter (equippedItems).
    // Este manager puede seguir ofreciendo helpers para consultar el estado global.

    public int activeCharacterIndex = 0;

    public void SetActiveCharacter(int index)
    {
        activeCharacterIndex = index;
        PlayerFighter target = (index == 0) ? GameManager.Instance?.character1 : GameManager.Instance?.character2;
        NotifyCharacterChanged(target);
        RefreshAllUI();
    }

    public void SetActiveCharacter0() => SetActiveCharacter(0);
    public void SetActiveCharacter1() => SetActiveCharacter(1);

    public void ToggleInventoryPanel(GameObject panel)
    {
        if (panel != null)
        {
            panel.SetActive(!panel.activeSelf);
            if (panel.activeSelf) RefreshAllUI();
        }
    }

    public bool IsEquippedByCharacter(int itemId, int characterIndex)
    {
        PlayerFighter target = characterIndex == 0 ? GameManager.Instance?.character1 : GameManager.Instance?.character2;
        return target != null && target.equippedItems.ContainsValue(itemId);
    }

    // ── Cache de datos ────────────────────────────────────────────────────────
    private Dictionary<int, InventoryDateBase.Object> _itemCache;

    // ─────────────────────────────────────────────────────────────────────────
    // Unity lifecycle
    // ─────────────────────────────────────────────────────────────────────────

    void OnEnable()
    {
        DialogueManager.OnGiveItem += HandleGiveItem;
    }

    void OnDisable()
    {
        DialogueManager.OnGiveItem -= HandleGiveItem;
    }

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // Ya existe una instancia persistente: limpiar pools porque los
            // GameObjects de UI de la escena anterior ya no existen.
            instance.equipmentPool.Clear();
            instance.objectsPool.Clear();
            
            // Intentar re-vincular UIs si este nuevo manager tiene referencias
            if (instance.equipmentUI == null) instance.equipmentUI = equipmentUI;
            if (instance.objetsUI == null) instance.objetsUI = objetsUI;

            Destroy(gameObject);
            return;
        }

        // Auto-asignar database si falta
        if (datebase == null)
        {
            datebase = Resources.Load<InventoryDateBase>("InventoryDatabase");
            // Si no está en Resources, se tendrá que asignar manualmente, pero esto ayuda.
        }

        BuildItemCache();
    }

    void Start()
    {
        Debug.Log("[InventoryManager] Start");
        RefreshAllUI();
    }

    /// <summary>
    /// Equipa el item al personaje indicado.
    /// </summary>
    public void Equip(int itemId, int characterIndex)
    {
        PlayerFighter char1 = GameManager.Instance?.character1;
        PlayerFighter char2 = GameManager.Instance?.character2;

        // Desequipar de cualquier otro personaje si lo tiene puesto (ítem único en este sentido)
        // Nota: Si el diseño permite que varios tengan el mismo ítem (instancias), esto podría sobrar, 
        // pero suele ser la norma en RPGs simples si el ID representa la instancia única en el inventario.
        // Como el inventario maneja "amount", si amount > 1, no deberíamos quitarlo del otro.
        
        InventoryObjectID invItem = inventory.Find(x => x.id == itemId);
        bool isShared = invItem.amount > 1;

        if (!isShared)
        {
            int otherIndex = characterIndex == 0 ? 1 : 0;
            PlayerFighter other = otherIndex == 0 ? char1 : char2;
            
            if (other != null)
            {
                InventoryDateBase.EquipmentSlot? slotFound = null;
                foreach(var kvp in other.equippedItems)
                {
                    if(kvp.Value == itemId)
                    {
                        slotFound = kvp.Key;
                        break;
                    }
                }
                if(slotFound.HasValue) other.UnequipItem(slotFound.Value);
            }
        }

        // Equipar al actual
        PlayerFighter target = characterIndex == 0 ? char1 : char2;
        target?.EquipItem(itemId);

        OnInventoryChanged?.Invoke();
    }

    /// <summary>
    /// Desequipa el item del personaje indicado.
    /// </summary>
    public void Unequip(int itemId, int characterIndex)
    {
        PlayerFighter char1 = GameManager.Instance?.character1;
        PlayerFighter char2 = GameManager.Instance?.character2;
        PlayerFighter target = characterIndex == 0 ? char1 : char2;

        if (target != null)
        {
            InventoryDateBase.EquipmentSlot? slotFound = null;
            foreach(var kvp in target.equippedItems)
            {
                if(kvp.Value == itemId)
                {
                    slotFound = kvp.Key;
                    break;
                }
            }
            if(slotFound.HasValue) target.UnequipItem(slotFound.Value);
        }

        OnInventoryChanged?.Invoke();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Inventario
    // ─────────────────────────────────────────────────────────────────────────

    public void AddItem(int id, int amount, InventoryDateBase.Uso uso)
    {
        Debug.Log($"[InventoryManager] AddItem id={id} amount={amount}");
        if (inventory == null)
            inventory = new List<InventoryObjectID>();

        for (int i = 0; i < inventory.Count; i++)
        {
            if (inventory[i].id == id)
            {
                inventory[i] = new InventoryObjectID(inventory[i].id, inventory[i].amount + amount, uso);
                RefreshAllUI();
                OnInventoryChanged?.Invoke();
                return;
            }
        }

        inventory.Add(new InventoryObjectID(id, amount, uso));
        RefreshAllUI();
        OnInventoryChanged?.Invoke();
    }

    public void DestroyItem(int id, int amount, InventoryDateBase.Uso uso)
    {
        if (inventory == null) return;

        for (int i = 0; i < inventory.Count; i++)
        {
            if (inventory[i].id == id)
            {
                int newAmount = inventory[i].amount - amount;
                if (newAmount <= 0)
                {
                    // Si estaba equipado, desequipar antes de eliminarlo
                    Unequip(id, 0);
                    Unequip(id, 1);
                    inventory.RemoveAt(i);
                }
                else
                {
                    inventory[i] = new InventoryObjectID(id, newAmount, uso);
                }

                RefreshAllUI();
                OnInventoryChanged?.Invoke();
                return;
            }
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // UI
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Refresca los dos paneles de UI. Llama esto en lugar de los 4 updateUI separados.
    /// </summary>
    public void RefreshAllUI()
    {
        UpdatePanel(equipmentUI, equipmentPool, InventoryDateBase.Uso.Equipable);

        // Usable, SkillNeed y Consumable van todos al mismo panel de objetos
        var objectItems = inventory
            .Where(o => o.uso == InventoryDateBase.Uso.Usable
                     || o.uso == InventoryDateBase.Uso.SkillNeed
                     || o.uso == InventoryDateBase.Uso.Consumable)
            .ToList();

        UpdatePanelWithList(objetsUI, objectsPool, objectItems);
    }

    private void UpdatePanel(Transform ui, List<InventoryUI> pool, InventoryDateBase.Uso uso)
    {
        if (ui == null || prefab == null || datebase == null) return;

        var filtered = inventory.Where(o => o.uso == uso).ToList();
        UpdatePanelWithList(ui, pool, filtered);
    }

    private void UpdatePanelWithList(Transform ui, List<InventoryUI> pool, List<InventoryObjectID> items)
    {
        if (ui == null || prefab == null || datebase == null) return;

        // Actualizar slots existentes
        for (int i = 0; i < items.Count; i++)
        {
            InventoryUI slot;
            if (i < pool.Count)
            {
                slot = pool[i];
                // Si el slot fue destruido (cambio de escena), recrearlo
                if (slot == null)
                {
                    slot = Instantiate(prefab, ui);
                    slot.transform.localScale = Vector3.one;
                    pool[i] = slot;
                }
            }
            else
            {
                slot = Instantiate(prefab, ui);
                slot.transform.localScale = Vector3.one;
                pool.Add(slot);
            }

            PopulateSlot(slot, items[i]);
            slot.gameObject.SetActive(true);
        }

        // Desactivar slots sobrantes
        for (int i = items.Count; i < pool.Count; i++)
        {
            if (pool[i] != null)
                pool[i].gameObject.SetActive(false);
        }
    }

    private void PopulateSlot(InventoryUI slot, InventoryObjectID item)
    {
        if (!TryGetItemData(item.id, out var data)) return;

        slot.itemId           = item.id; // necesario para que InventoryUI consulte equippedByCharacter
        slot.sprite.sprite    = data.sprite;
        slot.amount.text      = item.amount.ToString();
        slot.itemName.text    = data.name;
        slot.itemDescripcion.text = data.characteristic;
        
        // Ya no seteamos un solo statAffected sino que el slot podría mostrar varios o ninguno
        // por ahora dejamos el primero si existe para no romper la UI actual totalmente
        if (data.modifiers != null && data.modifiers.Count > 0)
        {
            slot.statAffected = data.modifiers[0].stat.ToString();
            slot.amountAffected = data.modifiers[0].amount;
        }

        // Refrescar el color del botón según estado persistido
        slot.RefreshEquippedVisual();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Queries
    // ─────────────────────────────────────────────────────────────────────────

    public bool HasItemInIventory(List<InventoryObjectID> itemsNeeded)
    {
        if (itemsNeeded == null || itemsNeeded.Count == 0) return true;

        foreach (var itemNeed in itemsNeeded)
        {
            var match = inventory.FirstOrDefault(x => x.id == itemNeed.id);
            if (match.amount >= itemNeed.amount) return true;
        }
        return false;
    }

    public bool HasItemInIventory(int _id, int _amount)
    {
        var match = inventory.FirstOrDefault(x => x.id == _id);
        return match.amount >= _amount;
    }

    public bool TryGetItemData(int id, out InventoryDateBase.Object data)
    {
        if (_itemCache == null) BuildItemCache();
        
        if (_itemCache.TryGetValue(id, out data))
        {
            return true;
        }

        // Fallback al SO si no está en caché (por ejemplo si se añadió dinámicamente)
        if (datebase != null && id >= 0 && id < datebase.DateBase.Length)
        {
            data = datebase.DateBase[id];
            _itemCache[id] = data; // actualizar caché
            return true;
        }

        data = default;
        return false;
    }

    public InventoryDateBase.Object GetItemInformation(int _id)
    {
        if (TryGetItemData(_id, out var data)) return data;
        Debug.LogWarning($"[InventoryManager] GetItemInformation: id inválido {_id}");
        return default;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Internos
    // ─────────────────────────────────────────────────────────────────────────

    private void BuildItemCache()
    {
        _itemCache = new Dictionary<int, InventoryDateBase.Object>();
        if (datebase != null && datebase.DateBase != null)
        {
            for (int i = 0; i < datebase.DateBase.Length; i++)
                _itemCache[i] = datebase.DateBase[i];
        }
    }

    private void HandleGiveItem(int id, int amount, InventoryDateBase.Uso uso)
    {
        Debug.Log($"[InventoryManager] HandleGiveItem id={id} x{amount}");
        AddItem(id, amount, uso);
    }

    [System.Serializable]
    public struct InventoryObjectID
    {
        public int id;
        public int amount;
        public InventoryDateBase.Uso uso;

        public InventoryObjectID(int id, int amount, InventoryDateBase.Uso uso)
        {
            this.id     = id;
            this.amount = amount;
            this.uso    = uso;
        }
    }
}
