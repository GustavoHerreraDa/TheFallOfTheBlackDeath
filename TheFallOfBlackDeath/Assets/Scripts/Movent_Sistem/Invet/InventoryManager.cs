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
    // itemId → índice del personaje que lo tiene equipado (0 = char1, 1 = char2)
    // Es la ÚNICA fuente de verdad sobre qué está equipado.
    public Dictionary<int, int> equippedByCharacter = new Dictionary<int, int>();

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
            Destroy(gameObject);
            return;
        }

        BuildItemCache();
    }

    void Start()
    {
        Debug.Log("[InventoryManager] Start");
        RefreshAllUI();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Equipamiento — fuente única de verdad
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Devuelve true si el item está equipado por ese personaje.
    /// </summary>
    public bool IsEquippedByCharacter(int itemId, int characterIndex)
    {
        return equippedByCharacter.TryGetValue(itemId, out int idx) && idx == characterIndex;
    }

    /// <summary>
    /// Equipa el item al personaje indicado y aplica el stat.
    /// Desequipa automáticamente al otro personaje si lo tenía.
    /// </summary>
    public void Equip(int itemId, int characterIndex, string statAffected, float amountAffected)
    {
        PlayerFighter char1 = GameManager.Instance?.character1;
        PlayerFighter char2 = GameManager.Instance?.character2;

        // Si el otro personaje lo tiene equipado, se lo quitamos primero
        int otherIndex = characterIndex == 0 ? 1 : 0;
        if (IsEquippedByCharacter(itemId, otherIndex))
        {
            PlayerFighter other = otherIndex == 0 ? char1 : char2;
            other?.UpdateStats(statAffected, -amountAffected);
            equippedByCharacter.Remove(itemId);
        }

        // Si ya lo tiene este personaje, no hacer nada (toggle lo maneja InventoryUI)
        if (IsEquippedByCharacter(itemId, characterIndex))
            return;

        // Equipar
        PlayerFighter target = characterIndex == 0 ? char1 : char2;
        target?.UpdateStats(statAffected, amountAffected);
        equippedByCharacter[itemId] = characterIndex;

        if (target != null)
            GameManager.Instance?.SavePlayerState(target);

        OnInventoryChanged?.Invoke();
    }

    /// <summary>
    /// Desequipa el item del personaje indicado y revierte el stat.
    /// </summary>
    public void Unequip(int itemId, int characterIndex, string statAffected, float amountAffected)
    {
        if (!IsEquippedByCharacter(itemId, characterIndex))
            return;

        PlayerFighter char1 = GameManager.Instance?.character1;
        PlayerFighter char2 = GameManager.Instance?.character2;
        PlayerFighter target = characterIndex == 0 ? char1 : char2;

        target?.UpdateStats(statAffected, -amountAffected);
        equippedByCharacter.Remove(itemId);

        if (target != null)
            GameManager.Instance?.SavePlayerState(target);

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
                    if (equippedByCharacter.ContainsKey(id))
                    {
                        int charIdx = equippedByCharacter[id];
                        var item = GetItemInformation(id);
                        Unequip(id, charIdx, item.statsAffected.ToString(), item.amountAffected);
                    }
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
        slot.statAffected     = data.statsAffected.ToString();
        slot.amountAffected   = data.amountAffected;

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
        data = default;
        if (_itemCache != null && _itemCache.TryGetValue(id, out data)) return true;
        if (datebase != null && datebase.DateBase != null && id >= 0 && id < datebase.DateBase.Length)
        {
            data = datebase.DateBase[id];
            return true;
        }
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