using UnityEngine;
using System.Collections.Generic;
using InventoryNew;
//TP2 FACUNDO FERREIRO/GUSTAVO TORRES
/// <summary>
/// Implements a controllable combatant that can execute skills, receive inventory upgrades, gain experience, and persist combat state between scenes.
/// </summary>
public class PlayerFighter : Fighter
{
    [Header("UI")]
    public PlayerSkillPanel skillPanel;
    public EnemiesPanel enemiesPanel;
    public BodyPartPanel bodyPartPanel;

    [Header("Combat Scanner")]
    public bool hasCombatScanner;

    public globalDataBase fightersDateBase;
    public int figherIndex;
    private int activeAllyIndex;
    public Fighter ally1;
    public Fighter ally2;


    private Skill skillToBeExecuted;

    private List<Fighter> allies;

    // Runtime flags for save/restore flow
    private bool hasSavedDataForThisFighter = false;
    private bool appliedSavedDataThisScene = false;
 
    [Header("New Inventory System")]
    public EquipmentHandler equipmentHandler;

    [Header("Skill Loadout")]
    public int maxActiveSkills = 4; // NUEVO: cantidad maxima de skills activas visibles/ejecutables.
    public Skill[] allLearnedSkills = new Skill[0]; // NUEVO: pool total de skills base + skills otorgadas por equipo.
    private readonly List<string> requestedActiveLoadoutIds = new List<string>(); // NUEVO: IDs pedidos para restaurar loadouts persistidos.

    /// <summary>
    /// Devuelve el modificador de un ítem para una estadística específica.
    /// Útil para previsualizar cambios.
    /// </summary>
    public float GetPreviewModifier(string itemId, StatType type)
    {
        if (NewInventoryManager.Instance == null) return 0;
        var data = NewInventoryManager.Instance.GetItemDataById(itemId) as NewEquipmentData;
        if (data == null || data.modifiers == null) return 0;

        float total = 0;
        foreach (var mod in data.modifiers)
        {
            if (mod.stat == type) total += mod.amount;
        }
        return total;
    }

    /// <summary>
    /// Initializes cached references and runtime state before the component starts running.
    /// </summary>
    void Awake()
    {
        if (bodyParts != null)
        {
            foreach (var part in bodyParts)
            {
                part.SetOwner(this);
            }
        }

        // Detect existing saved data first to avoid overwriting restored state
        if (GameManager.Instance != null)
        {
            hasSavedDataForThisFighter = GameManager.Instance.savedPlayersStatus != null &&
                                         GameManager.Instance.savedPlayersStatus.ContainsKey(figherIndex);
            Debug.Log($"[PlayerFighter.Awake] figherIndex={figherIndex} hasSavedData={hasSavedDataForThisFighter}");
        }

        //Esto es por si el globaldatabase falla en algun momento
        Stats safeDefaults = new Stats(21, 60, 60, 45, 4, 20, 20);

        // Only initialize from database when NO saved runtime data exists for this fighter
        if (!hasSavedDataForThisFighter)
        {
            if (fightersDateBase != null && figherIndex >= 0 && figherIndex < fightersDateBase.EnemyDB.Count)
            {
                var data = fightersDateBase.EnemyDB[figherIndex];
                Debug.Log($"[PlayerFighter.Awake] Init from DB for figherIndex={figherIndex} | lvl={data.level}, maxHp={data.maxHealth}, hp={data.hp}, atk={data.attack}, def={data.deffense}, spr={data.spirit}, spd={data.speed}");

                // Per-stat validation and fallbacks to avoid zero-initialization at runtime
                int level = data.level > 0 ? data.level : safeDefaults.level;
                float maxHp = data.maxHealth > 0 ? data.maxHealth : safeDefaults.maxHealth;
                float hp = data.hp > 0 ? data.hp : safeDefaults.health;
                float atk = data.attack > 0 ? data.attack : safeDefaults.attack;
                float def = data.deffense > 0 ? data.deffense : safeDefaults.deffense;
                float spr = data.spirit > 0 ? data.spirit : safeDefaults.spirit;
                float spd = data.speed > 0 ? data.speed : safeDefaults.speed;

                this.stats = new Stats(level, maxHp,  hp, atk,def, spr, spd, data.experience, data.experienceToNextLevel);
            }
            else
            {
                Debug.LogWarning($"[PlayerFighter.Awake] fightersDateBase is null or figherIndex out of range (index={figherIndex}). Using safe defaults.");
                this.stats = safeDefaults;
            }

            // Ensure health is within bounds when initializing from DB/defaults
            this.stats.health = Mathf.Clamp(this.stats.health, 1, this.stats.maxHealth);
        }
        else
        {
            // Create a temporary minimal stats object so references aren't null; will be overwritten by restore in Start
            if (this.stats == null)
            {
                this.stats = safeDefaults;
            }
            Debug.Log($"[PlayerFighter.Awake] Skipping DB init because saved data exists for figherIndex={figherIndex}");
        }

        // MODIFICADO: inicializar equipo antes de reconstruir el pool de skills.
        if (equipmentHandler == null)
            equipmentHandler = GetComponent<EquipmentHandler>();

        if (equipmentHandler != null)
        {
            equipmentHandler.Initialize(this);
            equipmentHandler.OnEquipChanged -= HandleEquipmentChanged;
            equipmentHandler.OnEquipChanged += HandleEquipmentChanged;
        }

        RebuildSkillPool(); // NUEVO
        int skillsCountInit = (this.skills != null) ? this.skills.Length : 0;
        Debug.Log($"[PlayerFighter.Awake] active loadout skills count={skillsCountInit}");

        allies = new List<Fighter>();
        allies.Add(this); // Agregar al jugador actual como el primer aliado activo
        activeAllyIndex = 0; // Establecer el jugador actual como el aliado activo inicialmente
        
        // Register with GameManager but DO NOT save over existing data here
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RegisterSceneFighter(this);
            if (!hasSavedDataForThisFighter)
            {
                // Only save when first time creating character with DB/default values
                GameManager.Instance.SavePlayerState(this);
                Debug.Log($"[PlayerFighter.Awake] Saved initial state (no prior save) for figherIndex={figherIndex} hp={stats.health}/{stats.maxHealth}");
            }
            else
            {
                Debug.Log($"[PlayerFighter.Awake] Not saving in Awake to avoid overwriting restored state for figherIndex={figherIndex}");
            }
        }
    }

    private void OnDestroy() // NUEVO
    {
        if (equipmentHandler != null)
            equipmentHandler.OnEquipChanged -= HandleEquipmentChanged;
    }

    private void HandleEquipmentChanged() // NUEVO
    {
        RebuildSkillPool();
    }

    public void RebuildSkillPool() // NUEVO
    {
        List<string> loadoutIdsToKeep = requestedActiveLoadoutIds.Count > 0
            ? new List<string>(requestedActiveLoadoutIds)
            : GetActiveLoadoutIds();

        var rebuiltPool = new List<Skill>();
        var seen = new HashSet<Skill>();

        AddSkillsToPool(GetComponentsInChildren<Skill>(true), rebuiltPool, seen);

        if (equipmentHandler != null)
            AddSkillsToPool(equipmentHandler.GetGrantedSkills(), rebuiltPool, seen);

        allLearnedSkills = rebuiltPool.ToArray();
        ApplyActiveLoadout(loadoutIdsToKeep, true);

        int poolCount = allLearnedSkills != null ? allLearnedSkills.Length : 0;
        int activeCount = skills != null ? skills.Length : 0;
        Debug.Log($"[PlayerFighter.RebuildSkillPool] pool={poolCount}, active={activeCount}, maxActive={maxActiveSkills}");
    }

    public void SetActiveLoadout(List<string> skillNames) // NUEVO
    {
        requestedActiveLoadoutIds.Clear();

        if (skillNames != null)
        {
            foreach (string skillName in skillNames)
            {
                if (string.IsNullOrEmpty(skillName) || requestedActiveLoadoutIds.Contains(skillName))
                    continue;

                requestedActiveLoadoutIds.Add(skillName);
            }
        }

        if (allLearnedSkills == null || allLearnedSkills.Length == 0)
            RebuildSkillPool();
        else
            ApplyActiveLoadout(requestedActiveLoadoutIds, true);
    }

    public List<string> GetActiveLoadoutIds() // NUEVO
    {
        var ids = new List<string>();
        if (skills == null) return ids;

        foreach (var skill in skills)
        {
            if (skill == null) continue;

            string id = GetSkillIdentifier(skill);
            if (!string.IsNullOrEmpty(id))
                ids.Add(id);
        }

        return ids;
    }

    private void AddSkillsToPool(Skill[] sourceSkills, List<Skill> pool, HashSet<Skill> seen) // NUEVO
    {
        if (sourceSkills == null) return;

        foreach (var skill in sourceSkills)
        {
            if (skill == null || seen.Contains(skill))
                continue;

            seen.Add(skill);
            pool.Add(skill);
        }
    }

    private void ApplyActiveLoadout(List<string> loadoutIds, bool fillEmptySlots) // NUEVO
    {
        int slotLimit = Mathf.Max(0, maxActiveSkills);
        var activeSkills = new List<Skill>(slotLimit);
        var used = new HashSet<Skill>();

        if (loadoutIds != null)
        {
            foreach (string loadoutId in loadoutIds)
            {
                if (activeSkills.Count >= slotLimit)
                    break;

                Skill skill = FindSkillInPool(loadoutId, used);
                if (skill == null)
                    continue;

                used.Add(skill);
                activeSkills.Add(skill);
            }
        }

        if (fillEmptySlots && allLearnedSkills != null)
        {
            foreach (var skill in allLearnedSkills)
            {
                if (activeSkills.Count >= slotLimit)
                    break;

                if (skill == null || used.Contains(skill))
                    continue;

                used.Add(skill);
                activeSkills.Add(skill);
            }
        }

        skills = activeSkills.ToArray();
    }

    private Skill FindSkillInPool(string skillId, HashSet<Skill> ignoredSkills) // NUEVO
    {
        if (string.IsNullOrEmpty(skillId) || allLearnedSkills == null)
            return null;

        foreach (var skill in allLearnedSkills)
        {
            if (skill == null || ignoredSkills.Contains(skill))
                continue;

            if (SkillMatchesId(skill, skillId))
                return skill;
        }

        return null;
    }

    private bool SkillMatchesId(Skill skill, string skillId) // NUEVO
    {
        if (skill == null || string.IsNullOrEmpty(skillId))
            return false;

        string stableId = GetSkillIdentifier(skill);
        if (string.Equals(stableId, skillId, System.StringComparison.Ordinal))
            return true;

        return !string.IsNullOrEmpty(skill.skillName) &&
               string.Equals(skill.skillName, skillId, System.StringComparison.Ordinal);
    }

    private string GetSkillIdentifier(Skill skill) // NUEVO
    {
        if (skill == null) return string.Empty;
        return !string.IsNullOrEmpty(skill.skillId) ? skill.skillId : skill.skillName;
    }

    /// <summary>
    /// Initializes the component once the scene dependencies are ready.
    /// </summary>
    private void Start()
    {
        // Ensure saved data is applied after all initializations in the scene
        if (GameManager.Instance != null && hasSavedDataForThisFighter)
        {
            StartCoroutine(DeferredApplySavedStatus());
        }

        // Body-part runtime state is restored by GameManager.PlayerStatusData.
    }

    /// <summary>
    /// Executes the deferred apply saved status workflow.
    /// </summary>
    /// <returns>An enumerator that drives the coroutine sequence.</returns>
    private System.Collections.IEnumerator DeferredApplySavedStatus()
    {
        // Wait one frame to allow other Awake/Start methods to run
        yield return null;

        if (GameManager.Instance != null && hasSavedDataForThisFighter && !appliedSavedDataThisScene)
        {
            Debug.Log($"[PlayerFighter.Start] Applying saved status AFTER init for figherIndex={figherIndex}");
            GameManager.Instance.ApplySavedStatusToFighter(this);
            appliedSavedDataThisScene = true;
            Debug.Log($"[PlayerFighter.Start] Post-restore hp={stats.health}/{stats.maxHealth}, atk={stats.attack}, def={stats.deffense}");
        }

        // Log skills count after any potential restore
        int count = (this.skills != null) ? this.skills.Length : 0;
        Debug.Log($"[PlayerFighter.Start] skills count after restore: {count}");
    }

    /// <summary>
    /// Initializes the turn.
    /// </summary>
    public override void InitTurn()
    {
        int count = (this.skills != null) ? this.skills.Length : 0;
        Debug.Log($"[PlayerFighter.InitTurn] skills count={count} for {idName}");

        this.skillPanel.ShowForPlayer(this);
        //statusPanel?.gameObject.SetActive(true);
    }

    /// <summary>
    /// Changes the ally.
    /// </summary>
    /// <param name="newIndex">The new index.</param>
    ///
    ///
    public void ChangeAlly(int newIndex)
    {
        if (newIndex < 0 || newIndex >= allies.Count)
        {
            Debug.LogError("Invalid ally index");
            return;
        }

        activeAllyIndex = newIndex;

        // Actualizar la informaci n del nuevo aliado activo en el panel de estado
        Fighter activeAlly = allies[activeAllyIndex];
        statusPanel.SetStats(activeAlly.idName, activeAlly.stats);

        // Realizar cualquier otra l gica necesaria al cambiar de aliado
    }

    /// <summary>
    /// Starts the execution flow for the selected player skill, including manual targeting when required.
    /// </summary>
    /// <param name="index">The index.</param>
    public void ExecuteSkill(int index)
    {
        int count = (this.skills != null) ? this.skills.Length : 0;
        Debug.Log($"[PlayerFighter.ExecuteSkill] received index={index}, skills count={count}");

        if (this.skills == null || index < 0 || index >= this.skills.Length)
        {
            Debug.LogError($"[PlayerFighter.ExecuteSkill] Invalid skill index {index}. Aborting.");
            return;
        }

        this.skillToBeExecuted = this.skills[index];
        if (this.skillToBeExecuted == null)
        {
            Debug.LogError($"[PlayerFighter.ExecuteSkill] Skill at index {index} is null. Aborting.");
            return;
        }

        this.skillToBeExecuted.SetEmitter(this);

        if (this.skillToBeExecuted.needsManualTargeting)
        {
            // Play specific animation depending on skill type
            if (this.skillToBeExecuted.skillType == SkillType.Range)
            {
                animator.Play("SkillSelected_Range");
            }
            else
            {
                // Default to Melee if it's not explicitly Range and needs targeting
                animator.Play("SkillSelected_Melee");
            }

            Fighter[] receivers = this.GetSkillTargets(this.skillToBeExecuted);
            this.enemiesPanel.Show(this, receivers);
            this.skillPanel.Hide();
            statusPanel?.gameObject.SetActive(false);
        }
        else
        {
            this.AutoConfigureSkillTargeting(this.skillToBeExecuted);
            this.combatManager.OnFighterSkill(this.skillToBeExecuted);

            this.skillPanel.Hide();
            
            statusPanel?.gameObject.SetActive(false);
        }
    }

    public void AttemptRun()
    {
        if (combatManager == null)
        {
            Debug.LogWarning($"[PlayerFighter.AttemptRun] combatManager is null on {idName}");
            return;
        }

        combatManager.TryRunFromCombat(this);
    }

    public override Stats GetCurrentStats()
    {
        float bonusMaxHealth = 0;
        float bonusAttack = 0;
        float bonusDefense = 0;
        float bonusSpirit = 0;
        float bonusSpeed = 0;

        // Sumar bonos del nuevo sistema si existe
        if (equipmentHandler != null)
        {
            bonusMaxHealth += equipmentHandler.GetTotalModifier(InventoryNew.StatType.MaxHealth);
            bonusAttack += equipmentHandler.GetTotalModifier(InventoryNew.StatType.Attack);
            bonusDefense += equipmentHandler.GetTotalModifier(InventoryNew.StatType.Defense);
            bonusSpirit += equipmentHandler.GetTotalModifier(InventoryNew.StatType.Spirit);
            bonusSpeed += equipmentHandler.GetTotalModifier(InventoryNew.StatType.Speed);
        }

        Stats total = new Stats(stats.level,
                                stats.maxHealth + bonusMaxHealth,
                                stats.health,
                                stats.attack + bonusAttack,
                                stats.deffense + bonusDefense,
                                stats.spirit + bonusSpirit,
                                stats.speed + bonusSpeed,
                                stats.experience,
                                stats.experienceToNextLevel);

        // Ensure health is clamped by total max health
        total.health = Mathf.Clamp(total.health, 0, total.maxHealth);

        // Actualizar la salud global basada en las partes del cuerpo si corresponde
        if (bodyParts != null && bodyParts.Count > 0)
        {
            float currentSum = 0;
            float maxPartSum = 0;
            foreach (var part in bodyParts)
            {
                currentSum += part.currentHealth;
                maxPartSum += part.GetMaxHealth(this);
            }
            total.health = currentSum;
            total.maxHealth = maxPartSum;
        }

        foreach (var mod in this.statusMods)
        {
            total = mod.Apply(total);
        }

        return total;
    }

    public float GetNewTotalStat(InventoryNew.StatType type)
    {
        float bonus = equipmentHandler != null ? equipmentHandler.GetTotalModifier(type) : 0;
        switch (type)
        {
            case InventoryNew.StatType.Attack: return stats.attack + bonus;
            case InventoryNew.StatType.Defense: return stats.deffense + bonus;
            case InventoryNew.StatType.Speed: return stats.speed + bonus;
            case InventoryNew.StatType.Spirit: return stats.spirit + bonus;
            case InventoryNew.StatType.MaxHealth: return stats.maxHealth + bonus;
            default: return 0;
        }
    }

    [System.Obsolete("Usar GetNewTotalStat en su lugar")]
    public float GetTotalStat(StatType type)
    {
        switch (type)
        {
            case StatType.Attack: return stats.attack + (equipmentHandler != null ? equipmentHandler.GetTotalModifier(type) : 0);
            case StatType.Defense: return stats.deffense + (equipmentHandler != null ? equipmentHandler.GetTotalModifier(type) : 0);
            case StatType.Speed: return stats.speed + (equipmentHandler != null ? equipmentHandler.GetTotalModifier(type) : 0);
            case StatType.Spirit: return stats.spirit + (equipmentHandler != null ? equipmentHandler.GetTotalModifier(type) : 0);
            case StatType.MaxHealth: return stats.maxHealth + (equipmentHandler != null ? equipmentHandler.GetTotalModifier(type) : 0);
            default: return 0;
        }
    }

    public void UpdateStats(StatType statType, float amountAffected)
    {
        switch (statType)
        {
            case StatType.Attack: stats.attack += amountAffected; break;
            case StatType.Defense: stats.deffense += amountAffected; break;
            case StatType.MaxHealth: stats.maxHealth += amountAffected; break;
            case StatType.Speed: stats.speed += amountAffected; break;
            case StatType.Spirit: stats.spirit += amountAffected; break;
        }
        
        // Persistir si no estamos en play (aunque esto parece ser lógica vieja)
        if (!Application.isPlaying && fightersDateBase != null)
        {
            fightersDateBase.UpdateFighterStats(figherIndex, amountAffected, statType);
        }

        statusPanel?.SetStats(idName, GetCurrentStats());
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SavePlayerState(this);
        }
    }

    /// <summary>
    /// Updates the stats.
    /// </summary>
    /// <param name="statAffected">The stat affected.</param>
    /// <param name="amountAffected">The amount affected.</param>
    public void UpdateStats(string statAffected, float amountAffected)
    {
        // Intentar parsear a StatType para reutilizar lógica
        if (System.Enum.TryParse(statAffected, out StatType type))
        {
            UpdateStats(type, amountAffected);
        }
        else
        {
            Debug.LogWarning("Stat no válido: " + statAffected);
        }
    }


    /// <summary>
    /// Finalizes the current player skill target and dispatches the action to the combat manager.
    /// </summary>
    /// <param name="enemyFighter">The enemy fighter.</param>
    public void SetTargetAndAttack(Fighter enemyFighter)
    {
        EnemyButtonUI enemyBtn = enemiesPanel.GetButtonFor(enemyFighter);
        if (enemyBtn != null)
            enemyBtn.ForceReset();

        if (this.skillToBeExecuted is BodyPartTargetSkill)
        {
            enemiesPanel.Hide();
            bodyPartPanel.Show(this, enemyFighter, this.skillToBeExecuted);
        }
        else
        {
            // Regresar a Idle o dejar que la animación de ataque tome el control
            animator.Play("Idle");

            skillToBeExecuted.AddReceiver(enemyFighter);
            combatManager.OnFighterSkill(skillToBeExecuted);
            skillPanel.Hide();
            enemiesPanel.Hide();
            combatManager.UpdateStatsUI();
        }
    }

    /// <summary>
    /// Executes the return workflow.
    /// </summary>
    public void Return()
    {
        animator.Play("Idle");

        if (this.skillPanel != null)
        {
            this.skillPanel.ShowForPlayer(this); 
        }
        else
        {
            Debug.LogWarning($"[PlayerFighter.Return] skillPanel is null on {idName}");
        }

        if (this.enemiesPanel != null)
        {
            this.enemiesPanel.Hide();
        }
    }

    /// <summary>
    /// Adds the allies to team.
    /// </summary>
    private void AddAlliesToTeam()
    {
        allies.Clear();
        allies.Add(ally1);
        allies.Add(ally2);
        // Agrega aqu  el resto de los aliados a la lista allies
    }

    /// <summary>
    /// Switches the active ally.
    /// </summary>
    private void SwitchActiveAlly()
    {
        activeAllyIndex++;
        if (activeAllyIndex >= allies.Count)
        {
            activeAllyIndex = 0;
        }

        Fighter activeAlly = allies[activeAllyIndex];
        // Realizar las acciones necesarias con el aliado activo
    }

    /// <summary>
    /// Gets the skill panel.
    /// </summary>
    /// <param name="newSkillPanel">The new skill panel.</param>
    /// <param name="newStatusPanel">The new status panel.</param>
    /// <param name="newEnemiesPanel">The new enemies panel.</param>
    /// <param name="newBodyPartPanel">The new body part panel.</param>
    /// <returns>The resulting value.</returns>
    public PlayerFighter GetSkillPanel(PlayerSkillPanel newSkillPanel, StatusPanel newStatusPanel, EnemiesPanel newEnemiesPanel, BodyPartPanel newBodyPartPanel)
    {
        skillPanel = newSkillPanel;
        statusPanel = newStatusPanel;
        enemiesPanel = newEnemiesPanel;
        bodyPartPanel = newBodyPartPanel;
        return this;
    }

    /// <summary>
    /// Applies an inventory-driven stat adjustment to the player fighter and persists the result.
    /// </summary>
    /// <param name="stat">The stat.</param>
    /// <param name="amount">The amount.</param>
    public void ApplyStatUpgrade(StatType stat, float amount)
    {
        Debug.Log($"[{idName}] +{amount} en {stat}");
        switch (stat)
        {
            case StatType.Attack:
                stats.attack += amount;
                break;
            case StatType.Defense:
                stats.deffense += amount;
                break;
            case StatType.Speed:
                stats.speed += amount;
                break;
            case StatType.Spirit:
                stats.spirit += amount;
                break;
            case StatType.MaxHealth:
                stats.maxHealth += amount;
                break;
        }

        // Reflect changes in UI and persist via GameManager runtime save
        statusPanel?.SetStats(idName, stats);
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SavePlayerState(this);
        }
    }

    /// <summary>
    /// Adds experience to the fighter, performs level-ups when thresholds are reached, and persists the result.
    /// </summary>
    /// <param name="amount">The amount.</param>
    public void AddExperience(int amount)
    {
        Debug.Log($"{idName} gana {amount} XP");

        stats.experience += amount;

        bool leveledUp = false;

        while (stats.experience >= stats.experienceToNextLevel)
        {
            stats.experience -= stats.experienceToNextLevel;
            LevelUp();
            leveledUp = true;
        }

        if (leveledUp && statusPanel != null)
            statusPanel.SetStats(idName, stats);

        if (leveledUp && GameManager.Instance != null)
        {
            GameManager.Instance.SavePlayerState(this);
        }
    }
    /// <summary>
    /// Executes the level up workflow.
    /// </summary>
    private void LevelUp()
    {
        stats.level++;

        
        stats.experienceToNextLevel = CalculateExpNeeded(stats.level);
        stats.maxHealth += 10;
        stats.attack += 5;
        stats.deffense += 3;
        stats.spirit += 2;
        stats.speed += 1;

        stats.health = stats.maxHealth;

        Debug.Log($"{idName} subiÃ³ al nivel {stats.level}!");
    }

    /// <summary>
    /// Executes the calculate exp needed workflow.
    /// </summary>
    /// <param name="level">The level.</param>
    /// <returns>The resulting value.</returns>
    private int CalculateExpNeeded(int level)
    {
        
        return Mathf.FloorToInt(50f * Mathf.Pow(level, 1.4f));
    }
    /// <summary>
    /// Removes the stat upgrade.
    /// </summary>
    /// <param name="stat">The stat.</param>
    /// <param name="amount">The amount.</param>
    public void RemoveStatUpgrade(StatType stat, float amount)
    {
        ApplyStatUpgrade(stat, -amount);
    }

    /// <summary>
    /// Saves the body part state.
    /// </summary>
    /// <param name="part">The part.</param>
    public void SaveBodyPartState(BodyPart part)
    {
        if (GameManager.Instance != null)
            GameManager.Instance.SavePlayerState(this);

        Debug.Log($"[PlayerFighter.SaveBodyPartState] Runtime body state saved for {idName}: {part}");
    }

    /// <summary>
    /// Loads the body state.
    /// </summary>
    public void LoadBodyState()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.ApplySavedStatusToFighter(this);
    }

    /// <summary>
    /// Retorna el multiplicador de movilidad considerando prótesis de piernas.
    /// 1.0 = normal, 0.5 = mitad, etc.
    /// </summary>
    public float GetLegMobilityMultiplier()
    {
        if (bodyParts == null) return 1.0f;
        float multiplier = 1.0f;
        foreach (var part in bodyParts)
        {
            if ((part.part == BodyPart.LeftLeg || part.part == BodyPart.RightLeg)
                && part.IsDestroyed && part.HasActiveProsthetic
                && equipmentHandler != null)
            {
                var slot = BodyPartToEquipmentSlot(part.part);
                var prosthetic = equipmentHandler.GetEquippedItem(slot) as InventoryNew.ProstheticData;
                if (prosthetic != null)
                    multiplier = Mathf.Min(multiplier, prosthetic.mobilityRestorePercent);
            }
        }
        return multiplier;
    }
}

