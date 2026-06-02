using UnityEngine;
using System;
using System.Collections.Generic;

namespace InventoryNew
{
    public class EquipmentHandler : MonoBehaviour
    {
        // Event for UI to react to
        public event Action OnEquipChanged;

        // The 8 anatomical slots
        private Dictionary<EquipmentSlot, NewEquipmentData> equippedItems = new Dictionary<EquipmentSlot, NewEquipmentData>();

        // Current totals for quick access
        private Dictionary<StatType, float> totalModifiers = new Dictionary<StatType, float>();

        [SerializeField] private PlayerFighter owner; // NUEVO: Fighter que recibe las skills otorgadas por equipo.
        private Dictionary<EquipmentSlot, List<GameObject>> grantedSkillInstances = new Dictionary<EquipmentSlot, List<GameObject>>(); // NUEVO

        private void Awake()
        {
            EnsureInitialized();
        }

        public void Initialize(PlayerFighter owner) // NUEVO
        {
            this.owner = owner;
            EnsureInitialized();
            RebuildGrantedSkillInstances();
        }

        private void EnsureInitialized()
        {
            if (equippedItems == null || equippedItems.Count == 0)
            {
                InitializeSlots();
            }
            else
            {
                EnsureEquipmentSlotKeys(); // NUEVO
            }

            if (totalModifiers == null || totalModifiers.Count == 0)
            {
                InitializeStats();
            }

            EnsureGrantedSkillSlotCache(); // NUEVO
        }

        private void InitializeSlots()
        {
            if (equippedItems == null) equippedItems = new Dictionary<EquipmentSlot, NewEquipmentData>();
            foreach (EquipmentSlot slot in Enum.GetValues(typeof(EquipmentSlot)))
            {
                equippedItems[slot] = null;
            }
        }

        private void EnsureEquipmentSlotKeys() // NUEVO
        {
            foreach (EquipmentSlot slot in Enum.GetValues(typeof(EquipmentSlot)))
            {
                if (!equippedItems.ContainsKey(slot))
                    equippedItems[slot] = null;
            }
        }

        private void InitializeStats()
        {
            if (totalModifiers == null) totalModifiers = new Dictionary<StatType, float>();
            foreach (StatType stat in Enum.GetValues(typeof(StatType)))
            {
                totalModifiers[stat] = 0;
            }
        }

        public void Equip(NewEquipmentData equipment)
        {
            EnsureInitialized();
            if (equipment == null) return;

            var currentItem = equippedItems[equipment.slot];

            if (NewInventoryManager.Instance != null)
            {
                if (!NewInventoryManager.Instance.TryRemoveItem(equipment.id, 1))
                {
                    Debug.LogWarning($"[EquipmentHandler] No se pudo equipar {equipment.itemName} porque no hay suficientes unidades en el inventario.");
                    return;
                }

                if (currentItem != null)
                {
                    NewInventoryManager.Instance.AddItem(currentItem, 1);
                }
            }

            equippedItems[equipment.slot] = equipment;
            DestroyGrantedSkillsForSlot(equipment.slot); // NUEVO
            CreateGrantedSkillsForEquipment(equipment); // NUEVO
            RecalculateStats();
            OnEquipChanged?.Invoke();
            
           /* if (AudioManager.Instance != null && AudioManager.Instance.equipSound != null)
            {
                AudioManager.Instance.PlaySFX(AudioManager.Instance.equipSound);
            }*/
        }

        public void Unequip(EquipmentSlot slot)
        {
            EnsureInitialized();
            if (equippedItems.ContainsKey(slot) && equippedItems[slot] != null)
            {
                var item = equippedItems[slot];
                equippedItems[slot] = null;
                DestroyGrantedSkillsForSlot(slot); // NUEVO

                // Devolver al inventario
                if (NewInventoryManager.Instance != null)
                {
                    NewInventoryManager.Instance.AddItem(item, 1);
                }

                RecalculateStats();
                OnEquipChanged?.Invoke();
            }
        }

        private void RecalculateStats()
        {
            InitializeStats();

            foreach (var item in equippedItems.Values)
            {
                if (item == null) continue;
                if (item.modifiers == null) continue; // MODIFICADO: equipos que solo otorgan skills pueden no tener modificadores.

                foreach (var mod in item.modifiers)
                {
                    totalModifiers[mod.stat] += mod.amount;
                }
            }
        }

        public float GetTotalModifier(StatType stat)
        {
            EnsureInitialized();
            if (totalModifiers.ContainsKey(stat))
            {
                return totalModifiers[stat];
            }
            return 0;
        }

        public float GetModifierForSlot(EquipmentSlot slot, StatType stat)
        {
            EnsureInitialized();
            if (equippedItems.ContainsKey(slot) && equippedItems[slot] != null)
            {
                if (equippedItems[slot].modifiers == null) return 0; // MODIFICADO

                float total = 0;
                foreach (var mod in equippedItems[slot].modifiers)
                {
                    if (mod.stat == stat) total += mod.amount;
                }
                return total;
            }
            return 0;
        }

        public NewEquipmentData GetEquippedItem(EquipmentSlot slot)
        {
            EnsureInitialized();
            if (equippedItems.TryGetValue(slot, out var item))
            {
                return item;
            }
            return null;
        }

        public Dictionary<EquipmentSlot, NewEquipmentData> GetAllEquipped()
        {
            EnsureInitialized();
            return new Dictionary<EquipmentSlot, NewEquipmentData>(equippedItems);
        }

        public void ClearAllEquipped()
        {
            EnsureInitialized();
            ClearAllGrantedSkillInstances(); // NUEVO
            InitializeSlots();
            RecalculateStats();
            OnEquipChanged?.Invoke();
        }

        /// <summary>
        /// Equips an item without consuming it from inventory and without unequipping/returning the previous one to inventory.
        /// Useful for loading saved states.
        /// </summary>
        public void EquipForce(NewEquipmentData equipment)
        {
            EnsureInitialized();
            if (equipment == null) return;
            DestroyGrantedSkillsForSlot(equipment.slot); // NUEVO
            equippedItems[equipment.slot] = equipment;
            CreateGrantedSkillsForEquipment(equipment); // NUEVO
            RecalculateStats();
            OnEquipChanged?.Invoke();
        }

        public Skill[] GetGrantedSkills() // NUEVO
        {
            EnsureInitialized();
            var skills = new List<Skill>();

            foreach (var instances in grantedSkillInstances.Values)
            {
                if (instances == null) continue;

                foreach (var instance in instances)
                {
                    if (instance == null) continue;
                    skills.AddRange(instance.GetComponentsInChildren<Skill>(true));
                }
            }

            return skills.ToArray();
        }

        private void EnsureGrantedSkillSlotCache() // NUEVO
        {
            if (grantedSkillInstances == null)
                grantedSkillInstances = new Dictionary<EquipmentSlot, List<GameObject>>();

            foreach (EquipmentSlot slot in Enum.GetValues(typeof(EquipmentSlot)))
            {
                if (!grantedSkillInstances.ContainsKey(slot) || grantedSkillInstances[slot] == null)
                    grantedSkillInstances[slot] = new List<GameObject>();
            }
        }

        private void RebuildGrantedSkillInstances() // NUEVO
        {
            ClearAllGrantedSkillInstances();
            if (owner == null) return;

            foreach (var item in equippedItems.Values)
            {
                if (item == null) continue;
                CreateGrantedSkillsForEquipment(item);
            }
        }

        private void CreateGrantedSkillsForEquipment(NewEquipmentData equipment) // NUEVO
        {
            if (owner == null || equipment == null || equipment.grantedSkillPrefabs == null)
                return;

            EnsureGrantedSkillSlotCache();

            foreach (var prefab in equipment.grantedSkillPrefabs)
            {
                if (prefab == null) continue;

                var instance = Instantiate(prefab, owner.transform);
                instance.name = $"{prefab.name} (Granted by {equipment.itemName})";
                grantedSkillInstances[equipment.slot].Add(instance);

                if (instance.GetComponentInChildren<Skill>(true) == null)
                {
                    Debug.LogWarning($"[EquipmentHandler] El prefab '{prefab.name}' otorgado por '{equipment.itemName}' no contiene ningun componente Skill.");
                }
            }
        }

        private void DestroyGrantedSkillsForSlot(EquipmentSlot slot) // NUEVO
        {
            EnsureGrantedSkillSlotCache();
            if (!grantedSkillInstances.TryGetValue(slot, out var instances) || instances == null)
                return;

            foreach (var instance in instances)
            {
                DestroySkillInstance(instance);
            }

            instances.Clear();
        }

        private void ClearAllGrantedSkillInstances() // NUEVO
        {
            EnsureGrantedSkillSlotCache();

            foreach (var slot in grantedSkillInstances.Keys)
            {
                DestroyGrantedSkillsForSlot(slot);
            }
        }

        private void DestroySkillInstance(GameObject instance) // NUEVO
        {
            if (instance == null) return;

            instance.transform.SetParent(null); // NUEVO: evita que el pool del PlayerFighter vea objetos pendientes de Destroy().

            if (Application.isPlaying)
                Destroy(instance);
            else
                DestroyImmediate(instance);
        }
    }
}
