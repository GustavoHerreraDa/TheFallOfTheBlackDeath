using UnityEngine;
using System.Collections.Generic;
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
 
    /// <summary>
    /// Initializes cached references and runtime state before the component starts running.
    /// </summary>
    void Awake()
    {
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

        // Always fetch skills from children (include inactive) to be the single source of truth
        this.skills = GetComponentsInChildren<Skill>(true);
        int skillsCountInit = (this.skills != null) ? this.skills.Length : 0;
        Debug.Log($"[PlayerFighter.Awake] fetched skills from children (includeInactive=true): count={skillsCountInit}");

        allies = new List<Fighter>();
        allies.Add(this); // Agregar al jugador actual como el primer aliado activo
        activeAllyIndex = 0; // Establecer el jugador actual como el aliado activo inicialmente
        
        // Register with GameManager but DO NOT save over existing data here
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetMainCharacter(this); // importante
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

        // Always restore destroyed body part visuals (legBroken se computa desde bodyParts)
        LoadBodyState();
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

        // Re-apply body part visual state after stats are restored
        LoadBodyState();

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
        statusPanel?.gameObject.SetActive(true);
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

    /// <summary>
    /// Updates the stats.
    /// </summary>
    /// <param name="statAffected">The stat affected.</param>
    /// <param name="amountAffected">The amount affected.</param>
    public void UpdateStats(string statAffected, float amountAffected)
    {
        Debug.Log($"[PlayerFighter.UpdateStats] id={idName} idx={figherIndex} stat={statAffected} delta={amountAffected}");

        switch (statAffected)
        {
            case "Attack":
                stats.attack += amountAffected;
                break;

            case "Defense":
                stats.deffense += amountAffected;
                break;

            case "Health":
                stats.health = Mathf.Clamp(stats.health + amountAffected, 0, stats.maxHealth);
                break;

            case "MaxHealth":
                stats.maxHealth = Mathf.Max(1, stats.maxHealth + amountAffected);
                stats.health = Mathf.Clamp(stats.health, 0, stats.maxHealth);
                break;

            case "Speed":
                stats.speed += amountAffected;
                break;

            case "Spirit":
                stats.spirit += amountAffected;
                break;

            default:
                Debug.LogWarning("Stat no vÃ¡lido: " + statAffected);
                break;
        }

        // Do NOT mutate ScriptableObject assets during play. Persist progression via GameManager saves instead.
        if (!Application.isPlaying && fightersDateBase != null)
        {
            fightersDateBase.UpdateFighterStats(figherIndex, amountAffected, statAffected);
        }

        // Refresh UI and save runtime state
        statusPanel?.SetStats(idName, stats);
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SavePlayerState(this);
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
        this.skillPanel.Show();
        this.enemiesPanel.Hide();
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
    public void ApplyStatUpgrade(InventoryDateBase.StatsUpgrade stat, float amount)
    {
        Debug.Log($"[{idName}] +{amount} en {stat}");
        switch (stat)
        {
            case InventoryDateBase.StatsUpgrade.Health:
                stats.health = Mathf.Clamp(stats.health + amount, 0, stats.maxHealth);
                break;
            case InventoryDateBase.StatsUpgrade.Attack:
                stats.attack += amount;
                break;
            case InventoryDateBase.StatsUpgrade.Defense:
                stats.deffense += amount;
                break;
            case InventoryDateBase.StatsUpgrade.Speed:
                stats.speed += amount;
                break;
            case InventoryDateBase.StatsUpgrade.Spirit:
                stats.spirit += amount;
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
    public void RemoveStatUpgrade(InventoryDateBase.StatsUpgrade stat, float amount)
    {

        ApplyStatUpgrade(stat, -amount);
    }

    /// <summary>
    /// Saves the body part state.
    /// </summary>
    /// <param name="part">The part.</param>
    public void SaveBodyPartState(BodyPart part)
    {
        if (fightersDateBase == null || figherIndex < 0 || figherIndex >= fightersDateBase.EnemyDB.Count)
        {
            Debug.LogWarning($"[PlayerFighter.SaveBodyPartState] Cannot save: DB null or index out of range (index={figherIndex})");
            return;
        }

        globalDataBase.EnemyStats entry = fightersDateBase.EnemyDB[figherIndex];

        if (entry.destroyedParts == null)
            entry.destroyedParts = new System.Collections.Generic.List<BodyPart>();

        if (!entry.destroyedParts.Contains(part))
            entry.destroyedParts.Add(part);

        entry.currentHealth = stats.health;
        fightersDateBase.EnemyDB[figherIndex] = entry;
        Debug.Log($"[PlayerFighter.SaveBodyPartState] Saved destroyed part {part}, hp={stats.health} for figherIndex={figherIndex}");
    }

    /// <summary>
    /// Loads the body state.
    /// </summary>
    public void LoadBodyState()
    {
        if (fightersDateBase == null || figherIndex < 0 || figherIndex >= fightersDateBase.EnemyDB.Count)
            return;

        var entry = fightersDateBase.EnemyDB[figherIndex];

        if (entry.destroyedParts == null || entry.destroyedParts.Count == 0)
            return;

        foreach (BodyPart part in entry.destroyedParts)
        {
            BodyPartData partData = GetBodyPart(part);
            if (partData != null)
                partData.currentHealth = 0;
        }

        SyncBodyPartVisuals();

        if (entry.currentHealth > 0)
            stats.health = Mathf.Clamp(entry.currentHealth, 0, stats.maxHealth);

        Debug.Log($"[PlayerFighter.LoadBodyState] Restored {entry.destroyedParts.Count} parts, legBroken={legBroken} (computed), hp={stats.health} for figherIndex={figherIndex}");
    }
}

