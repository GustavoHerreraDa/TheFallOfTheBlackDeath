using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
using TMPro;
using UnityEngine.Serialization;
using InventoryNew;


/// <summary>
/// Coordinates persistent world state, party persistence, recruitment, and scene transitions across exploration and battle scenes.
/// </summary>
public class GameManager : MonoBehaviour
{
    static public GameManager _instance;

    //CLASS RANDOM MONSTER
    [System.Serializable]
    /// <summary>
    /// Stores the encounter configuration used to map exploration regions to possible battle scenes and enemy groups.
    /// </summary>
    public class RegionData
    {
        public string BattleScene;
        public string regionName;
        public int maxAmountEnemys = 4;
        public List<GameObject> Enemys = new List<GameObject>();
    }

    [System.Serializable]
    /// <summary>
    /// Stores the runtime stats and body-part health snapshot persisted for each recruited player fighter.
    /// </summary>
    public class PlayerStatusData
    {
        public float currentHealth;
        public float maxHealth;

        public int level;
        public int experience;

        public float attack;
        public float defense;
        public float spirit;
        public float speed;

        public List<float> bodyPartsHealth = new List<float>();
        public List<float> bodyPartsMaxHealth = new List<float>();
        public List<float> prostheticHealths = new List<float>(); // NUEVO: salud de las prótesis

        [System.Serializable]
        public struct EquippedItemData
        {
            public InventoryNew.EquipmentSlot slot;
            public string itemId;
        }
        public List<EquippedItemData> equippedItems = new List<EquippedItemData>();
        public List<string> activeSkillLoadoutIds = new List<string>(); // NUEVO: IDs de las skills activas para restaurar el loadout.
    }
    
    public event System.Action OnPlayerStatsUpdated;

    [System.Serializable]
    public class PartyPositionData
    {
        public int fighterIndex;
        public Vector3 position;
    }

    [SerializeField]
    private List<PartyPositionData> savedPartyPositions = new List<PartyPositionData>();

    public Dictionary<int, PlayerStatusData> savedPlayersStatus = new Dictionary<int, PlayerStatusData>();
    [FormerlySerializedAs("globalEnemyDatabase")] public globalDataBase globalGlobalDatabase;
    public List<RegionData> Regions = new List<RegionData>();

    public GameObject character;
    //agrego estas referencias para poder acceder al Fighter desde InventoryUI y equipar objetos.
    public PlayerFighter character1;
    public PlayerFighter character2;

    [Header("Debug")]
    [SerializeField] private bool enableDebugTools = true;

    [Header("Party System")]
    [SerializeField] private List<int> activePartyIds = new List<int>();
    [SerializeField] private List<int> recruitedCharacterIds = new List<int>();
    [SerializeField] public int maxActivePartySize = 3;

    private List<PlayerFighter> activeParty = new List<PlayerFighter>();

    public void RegisterPartyMember(PlayerFighter fighter)
    {
        if (fighter == null) return;
        NormalizePartyIds();
        if (!activePartyIds.Contains(fighter.figherIndex))
        {
            if (activePartyIds.Count < maxActivePartySize)
            {
                activePartyIds.Add(fighter.figherIndex);
            }
            else
            {
                Debug.LogWarning($"[GameManager] No se pudo añadir a {fighter.idName} a la party activa: límite alcanzado.");
            }
        }
        
        if (!recruitedCharacterIds.Contains(fighter.figherIndex))
        {
            recruitedCharacterIds.Add(fighter.figherIndex);
        }

        if (activePartyIds.Contains(fighter.figherIndex))
        {
            RegisterRuntimePartyReference(fighter);
            SetDatabaseActivePartyFlag(fighter.figherIndex, true);
        }
        else
        {
            SetDatabaseActivePartyFlag(fighter.figherIndex, false);
        }
        
        UpdateCompatibilityProperties();
        RefreshUI();
    }

    public void UnregisterPartyMember(PlayerFighter fighter)
    {
        if (fighter == null) return;
        activePartyIds.Remove(fighter.figherIndex);
        activeParty.Remove(fighter);
        SetDatabaseActivePartyFlag(fighter.figherIndex, false);
        UpdateCompatibilityProperties();
        RefreshUI();
    }

    public List<PlayerFighter> GetPartyMembers()
    {
        RefreshActivePartyReferencesFromScene();
        return new List<PlayerFighter>(activeParty);
    }

    /// <summary>
    /// Obtiene la imagen del personaje desde la base de datos global.
    /// </summary>
    public Sprite GetCharacterImage(int fighterIndex)
    {
        if (globalGlobalDatabase != null && fighterIndex >= 0 && fighterIndex < globalGlobalDatabase.EnemyDB.Count)
        {
            return globalGlobalDatabase.EnemyDB[fighterIndex].characterImage;
        }
        return null;
    }

    public List<int> GetActivePartyIds()
    {
        NormalizePartyIds();
        return new List<int>(activePartyIds);
    }

    public PlayerFighter GetLeader()
    {
        RefreshActivePartyReferencesFromScene();
        return activeParty.Count > 0 ? activeParty[0] : null;
    }

    public void SetLeader(PlayerFighter fighter)
    {
        if (fighter == null) return;

        NormalizePartyIds();
        MarkCharacterRecruited(fighter.figherIndex);
        activePartyIds.Remove(fighter.figherIndex);
        activePartyIds.Insert(0, fighter.figherIndex);
        TrimActivePartyToLimit();
        RegisterRuntimePartyReference(fighter);
        SyncDatabasePartyFlags();
        UpdateCompatibilityProperties();
        RefreshUI();
    }

    public bool IsRecruited(int fighterIndex) => recruitedCharacterIds.Contains(fighterIndex);

    public bool IsActivePartyMember(int fighterIndex)
    {
        NormalizePartyIds();
        return activePartyIds.Contains(fighterIndex);
    }

    public void MarkCharacterRecruited(int fighterIndex)
    {
        if (fighterIndex < 0) return;
        if (!recruitedCharacterIds.Contains(fighterIndex))
        {
            recruitedCharacterIds.Add(fighterIndex);
        }
    }

    public void RegisterSceneFighter(PlayerFighter fighter)
    {
        if (fighter == null) return;

        bool isMain = IsMainCharacterInDatabase(fighter.figherIndex, fighter.fightersDateBase);
        bool isActive = IsActivePartyMember(fighter.figherIndex) ||
                        IsSecondaryCharacterInDatabase(fighter.figherIndex, fighter.fightersDateBase);

        if (isMain || fighter.GetComponent<PlayerControl>() != null)
        {
            SetMainCharacter(fighter);
        }
        else if (isActive)
        {
            RegisterPartyMember(fighter);
        }
    }

    private void UpdateCompatibilityProperties()
    {
        RefreshActivePartyReferencesFromScene();
        character1 = activeParty.Count > 0 ? activeParty[0] : null;
        character2 = activeParty.Count > 1 ? activeParty[1] : null;
        hasRecruitedSecondary = character2 != null;
    }

    private void NormalizePartyIds()
    {
        var uniqueIds = new List<int>();
        foreach (int id in activePartyIds)
        {
            if (id < 0 || uniqueIds.Contains(id)) continue;
            uniqueIds.Add(id);
            if (uniqueIds.Count >= maxActivePartySize) break;
        }

        activePartyIds = uniqueIds;
    }

    private void TrimActivePartyToLimit()
    {
        NormalizePartyIds();
        while (activePartyIds.Count > maxActivePartySize)
        {
            activePartyIds.RemoveAt(activePartyIds.Count - 1);
        }
    }

    private void RegisterRuntimePartyReference(PlayerFighter fighter)
    {
        if (fighter == null) return;

        activeParty.RemoveAll(p => p == null || p.figherIndex == fighter.figherIndex);
        activeParty.Add(fighter);
        OrderRuntimePartyByIds();
    }

    private void RefreshActivePartyReferencesFromScene()
    {
        NormalizePartyIds();
        activeParty.RemoveAll(p => p == null || !activePartyIds.Contains(p.figherIndex));

        var sceneFighters = FindObjectsOfType<PlayerFighter>();
        foreach (int id in activePartyIds)
        {
            if (activeParty.Any(p => p != null && p.figherIndex == id)) continue;

            var match = sceneFighters.FirstOrDefault(p => p != null && p.figherIndex == id);
            if (match != null)
            {
                activeParty.Add(match);
            }
        }

        OrderRuntimePartyByIds();
    }

    private void OrderRuntimePartyByIds()
    {
        activeParty = activeParty
            .Where(p => p != null && activePartyIds.Contains(p.figherIndex))
            .OrderBy(p => activePartyIds.IndexOf(p.figherIndex))
            .ToList();
    }

    private bool IsMainCharacterInDatabase(int fighterIndex, globalDataBase dbOverride = null)
    {
        var db = dbOverride != null ? dbOverride : globalGlobalDatabase;
        return db != null &&
               fighterIndex >= 0 &&
               fighterIndex < db.EnemyDB.Count &&
               db.EnemyDB[fighterIndex].isMainCharacter;
    }

    private bool IsSecondaryCharacterInDatabase(int fighterIndex, globalDataBase dbOverride = null)
    {
        var db = dbOverride != null ? dbOverride : globalGlobalDatabase;
        return db != null &&
               fighterIndex >= 0 &&
               fighterIndex < db.EnemyDB.Count &&
               db.EnemyDB[fighterIndex].isSecondaryCharacter;
    }

    private void SetDatabaseActivePartyFlag(int fighterIndex, bool isActive)
    {
        var db = globalGlobalDatabase;
        if (db == null && character1 != null)
        {
            db = character1.fightersDateBase;
        }

        if (db == null ||
            fighterIndex < 0 ||
            fighterIndex >= db.EnemyDB.Count ||
            db.EnemyDB[fighterIndex].isMainCharacter)
        {
            return;
        }

        db.SetSecondaryCharacter(fighterIndex, isActive);
    }

    private void SyncDatabasePartyFlags()
    {
        if (globalGlobalDatabase == null) return;

        for (int i = 0; i < globalGlobalDatabase.EnemyDB.Count; i++)
        {
            if (globalGlobalDatabase.EnemyDB[i].isMainCharacter)
            {
                globalGlobalDatabase.SetSecondaryCharacter(i, false);
            }
            else
            {
                globalGlobalDatabase.SetSecondaryCharacter(i, activePartyIds.Contains(i));
            }
        }
    }

    public bool hasRecruitedSecondary = false;
    public bool hasValidLastPos = false;
    public Vector3 lastPos;
    private int lastExplorationSceneIndex = 1;
    public Transform startPost;
    public List<string> groupEnemyDefeat;
    public List<string> objectsPickup;

    [Header("Escape Flow")]
    [SerializeField] private float defaultEscapedEnemyStunDuration = 4f;
    private string currentEncounterGroupName = string.Empty;
    private string pendingEscapedEnemyGroupName = string.Empty;
    private float pendingEscapedEnemyStunDuration;
    
    // New persistent pickups using GUIDs
    [SerializeField] private List<string> collectedPickupGuids = new List<string>();

    public bool IsPickupCollected(string guid)
    {
        if (string.IsNullOrEmpty(guid)) return false;
        return collectedPickupGuids.Contains(guid);
    }

    public void RegisterPickupCollected(string guid)
    {
        if (string.IsNullOrEmpty(guid)) return;
        if (!collectedPickupGuids.Contains(guid))
        {
            collectedPickupGuids.Add(guid);
        }
    }

    public void RegisterCurrentEncounterGroup(string groupName)
    {
        currentEncounterGroupName = string.IsNullOrEmpty(groupName) ? string.Empty : groupName;
    }

    public string GetCurrentEncounterGroupName()
    {
        return currentEncounterGroupName;
    }

    public void RegisterEscapedEncounter(string groupName, float stunDuration)
    {
        string groupToStun = string.IsNullOrEmpty(groupName) ? currentEncounterGroupName : groupName;
        if (string.IsNullOrEmpty(groupToStun))
            return;

        pendingEscapedEnemyGroupName = groupToStun;
        pendingEscapedEnemyStunDuration = stunDuration > 0f ? stunDuration : defaultEscapedEnemyStunDuration;
    }

    private void ApplyPendingEscapedEnemyStun()
    {
        if (string.IsNullOrEmpty(pendingEscapedEnemyGroupName))
            return;

        if (enemies == null || enemies.Count == 0)
            FindEnemiesAndObjets();

        var escapedEnemy = enemies.FirstOrDefault(x => x != null && x.GroupName == pendingEscapedEnemyGroupName);
        if (escapedEnemy != null)
        {
            escapedEnemy.StunForSeconds(pendingEscapedEnemyStunDuration);
            Debug.Log($"Enemy group escaped from stunned: {pendingEscapedEnemyGroupName}");
        }
        else
        {
            Debug.LogWarning($"No enemy group found to stun after escape: {pendingEscapedEnemyGroupName}");
        }

        pendingEscapedEnemyGroupName = string.Empty;
        pendingEscapedEnemyStunDuration = 0f;
        currentEncounterGroupName = string.Empty;
    }

    public void RegisterPartyDefeat(Fighter[] defeatedTeam)
    {
        RecoverSavedPartyAfterDefeat(defeatedTeam);
        ClearCombatTransitionState();
        SetGameState(GameStates.IDLE_STATE);
    }

    public void PrepareForGameOverReturnToMenu()
    {
        RecoverSavedPartyAfterDefeat(null);
        ClearCombatTransitionState();
        SetGameState(GameStates.IDLE_STATE);
    }

    private void RecoverSavedPartyAfterDefeat(Fighter[] defeatedTeam)
    {
        bool recoveredRuntimeFighter = false;

        if (defeatedTeam != null)
        {
            foreach (var fighter in defeatedTeam)
            {
                if (fighter is PlayerFighter playerFighter)
                {
                    SavePlayerState(playerFighter);
                    RecoverSavedStatusForRespawn(playerFighter.figherIndex, playerFighter);
                    recoveredRuntimeFighter = true;
                }
            }
        }

        if (!recoveredRuntimeFighter && savedPlayersStatus != null)
        {
            foreach (int fighterIndex in savedPlayersStatus.Keys.ToList())
            {
                RecoverSavedStatusForRespawn(fighterIndex, null);
            }
        }

        RefreshUI();
    }

    private void RecoverSavedStatusForRespawn(int fighterIndex, PlayerFighter runtimeFighter)
    {
        if (savedPlayersStatus == null || !savedPlayersStatus.TryGetValue(fighterIndex, out var savedStatus))
            return;

        float maxHealth = savedStatus.maxHealth;
        if (runtimeFighter != null && runtimeFighter.stats != null)
            maxHealth = Mathf.Max(maxHealth, runtimeFighter.stats.maxHealth);

        savedStatus.maxHealth = maxHealth;
        savedStatus.currentHealth = maxHealth;

        if (runtimeFighter != null && runtimeFighter.bodyParts != null)
        {
            savedStatus.bodyPartsHealth = new List<float>();
            savedStatus.bodyPartsMaxHealth = new List<float>();

            foreach (var part in runtimeFighter.bodyParts)
            {
                float partMaxHealth = part.GetMaxHealth(runtimeFighter);
                part.currentHealth = partMaxHealth;
                savedStatus.bodyPartsHealth.Add(partMaxHealth);
                savedStatus.bodyPartsMaxHealth.Add(partMaxHealth);
            }

            runtimeFighter.stats.health = maxHealth;
            runtimeFighter.statusPanel?.SetStats(runtimeFighter.idName, runtimeFighter.stats);
        }
        else if (savedStatus.bodyPartsMaxHealth != null &&
                 savedStatus.bodyPartsHealth != null &&
                 savedStatus.bodyPartsMaxHealth.Count == savedStatus.bodyPartsHealth.Count)
        {
            savedStatus.bodyPartsHealth = new List<float>(savedStatus.bodyPartsMaxHealth);
        }

        savedPlayersStatus[fighterIndex] = savedStatus;
    }

    private void ClearCombatTransitionState()
    {
        ClearSavedPosition();
        enemyToBattle.Clear();
        enemyAnount = 0;
        enemyAmount = 0;
        canGetEncounter = false;
        gotAttacked = false;
        isWalking = false;
        currentEncounterGroupName = string.Empty;
        pendingEscapedEnemyGroupName = string.Empty;
        pendingEscapedEnemyStunDuration = 0f;
    }

    public bool canGetEncounter = false;
    public bool gotAttacked = false;
    public bool isWalking = false;
    public int enemyAmount;

    public SanitySystem sanity;
    //ENUM
    /// <summary>
    /// Defines the named values used by game states.
    /// </summary>
    public enum GameStates
    {
        TOWN_STATE,
        BATTLE_STATE,
        IDLE_STATE,
        SAFE_ZONE

    }
    
    public void NotifyPlayerStatsUpdated()
    {
        RefreshUI();
    }

    /// <summary>
    /// Refreshes the ui.
    /// </summary>
    public void RefreshUI()
    {
        // El ?.Invoke() significa: "Si hay alguien escuchando, avísale".
        OnPlayerStatsUpdated?.Invoke();
    }

    //BATTLE
    public int enemyAnount;
    public List<GameObject> enemyToBattle = new List<GameObject>();



    public int cuRegions;


    public GameStates gameState;

    /*public GameObject Character
    {
        get { return FindObjectOfType<Movent>().gameObject; }
    }
    */

    /// <summary>
    /// Sets the game state.
    /// </summary>
    /// <param name="newState">The new state.</param>
    public void SetGameState(GameStates newState)
    {
        gameState = newState;

        switch (gameState)
        {
            case GameStates.SAFE_ZONE:
                //sanity.StartIncreaseSanity();
                break;

            case GameStates.TOWN_STATE:
            case GameStates.IDLE_STATE:
                //sanity.StartDecreaseSanity();
                break;

            case GameStates.BATTLE_STATE:
                //sanity.StopSanityChanges();
                break;
        }
    }

    public List<EnemiesGroup> enemies;
    public static GameManager Instance
    {
        get { return _instance; }
    }

    // Assign safe references to main/secondary characters without exposing direct field writes
    /// <summary>
    /// Sets the main character.
    /// </summary>
    /// <param name="pf">The pf.</param>
    public void SetMainCharacter(PlayerFighter pf)
    {
        if (pf == null) return;

        if (globalGlobalDatabase != null)
        {
            for (int i = 0; i < globalGlobalDatabase.EnemyDB.Count; i++)
            {
                if (globalGlobalDatabase.EnemyDB[i].isMainCharacter && i != pf.figherIndex)
                {
                    globalGlobalDatabase.SetMainCharacter(i, false);
                }
            }

            globalGlobalDatabase.SetMainCharacter(pf.figherIndex, true);
        }

        if (!activePartyIds.Contains(pf.figherIndex))
        {
            activePartyIds.Insert(0, pf.figherIndex);
            TrimActivePartyToLimit();
        }

        SetLeader(pf);
    }

    /// <summary>
    /// Sets the secondary character.
    /// </summary>
    /// <param name="pf">The pf.</param>
    public void SetSecondaryCharacter(PlayerFighter pf)
    {
        if (pf == null)
        {
            if (activePartyIds.Count > 1) activePartyIds.RemoveAt(1);
            UpdateCompatibilityProperties();
            RefreshUI();
            return;
        }
        
        if (!activePartyIds.Contains(pf.figherIndex))
        {
            if (activePartyIds.Count >= 2)
            {
                activePartyIds[1] = pf.figherIndex;
            }
            else
            {
                activePartyIds.Add(pf.figherIndex);
            }
        }
        else
        {
            // Mover a la segunda posición si ya está en la party pero no es segundo
            int idx = activePartyIds.IndexOf(pf.figherIndex);
            if (idx != 1)
            {
                activePartyIds.RemoveAt(idx);
                if (activePartyIds.Count >= 1) activePartyIds.Insert(1, pf.figherIndex);
                else activePartyIds.Add(pf.figherIndex);
            }
        }
        
        if (!recruitedCharacterIds.Contains(pf.figherIndex))
        {
            recruitedCharacterIds.Add(pf.figherIndex);
        }

        TrimActivePartyToLimit();
        RegisterRuntimePartyReference(pf);
        SetDatabaseActivePartyFlag(pf.figherIndex, true);
        SyncDatabasePartyFlags();
        
        UpdateCompatibilityProperties();
        RefreshUI();
    }

    // Resolve characters based on DB and scene content to avoid circular deps with PartyManager
    /// <summary>
    /// Updates the characters from database.
    /// </summary>
    private void UpdateCharactersFromDatabase()
    {
        // Prefer the global DB, else try to use one referenced by any PlayerFighter in scene
        var db = globalGlobalDatabase;
        if (db == null)
        {
            var anyPF = FindObjectOfType<PlayerFighter>();
            if (anyPF != null) db = anyPF.fightersDateBase;
        }

        int mainIdx = -1;
        var dbActiveIds = new List<int>();
        if (db != null && db.EnemyDB != null)
        {
            for (int i = 0; i < db.EnemyDB.Count; i++)
            {
                if (db.EnemyDB[i].isMainCharacter) mainIdx = i;
                if (db.EnemyDB[i].isSecondaryCharacter) dbActiveIds.Add(i);
            }
        }

        /// <summary>
        /// Executes the try find by switcher index workflow.
        /// </summary>
        /// <param name="idx">The idx.</param>
        /// <returns>The resulting value.</returns>
        PlayerFighter TryFindByDatabaseIndex(int idx)
        {
            if (idx < 0) return null;
            foreach (var pf in FindObjectsOfType<PlayerFighter>())
            {
                if (pf.figherIndex == idx) return pf;
            }

            // Try via PartyManager list first
            var manager = PartyManager.Instance;
            int switcherIndex = idx;
            if (db != null && idx < db.EnemyDB.Count)
            {
                switcherIndex = db.EnemyDB[idx].CharacterSwitcherIndex;
            }

            if (manager != null && manager.partyObjects != null && switcherIndex < manager.partyObjects.Count && switcherIndex >= 0)
            {
                var go = manager.partyObjects[switcherIndex];
                if (go != null)
                {
                    var pf = go.GetComponent<PlayerFighter>();
                    if (pf != null) return pf;
                }
            }
            return null;
        }

        if (activePartyIds.Count == 0 && mainIdx >= 0)
        {
            activePartyIds.Add(mainIdx);
        }

        foreach (int id in dbActiveIds)
        {
            if (!activePartyIds.Contains(id) && activePartyIds.Count < maxActivePartySize)
            {
                activePartyIds.Add(id);
            }
        }

        var mainPf = TryFindByDatabaseIndex(mainIdx);

        if (mainPf != null) SetMainCharacter(mainPf);

        foreach (int id in activePartyIds.ToArray())
        {
            if (id == mainIdx) continue;

            var partyMember = TryFindByDatabaseIndex(id);
            if (partyMember != null)
            {
                RegisterPartyMember(partyMember);
            }
        }

        SyncDatabasePartyFlags();
        UpdateCompatibilityProperties();
    }

    /// <summary>
    /// Initializes cached references and runtime state before the component starts running.
    /// </summary>
    void Awake()
    {
        //enemies = FindObjectsOfType<EnemiesGroup>();
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        /// <summary>
        /// Executes the if workflow.
        /// </summary>
        /// <param name="!">The !.</param>
        /// <returns>The resulting value.</returns>
        else if (_instance != this)
        {
            Destroy(gameObject);
        }

        if (!GameObject.Find("Character"))
        {
            GameObject Hero = Instantiate(character, Vector3.zero, Quaternion.identity) as GameObject;
            Hero.name = "Character";
        }

        if (enableDebugTools)
        {
            if (GetComponent<FighterDebugTools>() == null)
            {
                gameObject.AddComponent<FighterDebugTools>();
                Debug.Log("[GameManager] FighterDebugTools añadido al GameManager.");
            }
        }
        else
        {
            Debug.Log("[GameManager] enableDebugTools está desactivado. No se añadirán las herramientas de debug.");
        }
    }

    /// <summary>
    /// Initializes the component once the scene dependencies are ready.
    /// </summary>
    private void Start()
    {
        // 1. Limpiar estado de la DB al iniciar para evitar persistencia del ScriptableObject
        if (globalGlobalDatabase != null)
        {
            for (int i = 0; i < globalGlobalDatabase.EnemyDB.Count; i++)
            {
                // Solo limpiamos si NO es el protagonista.
                if (!globalGlobalDatabase.EnemyDB[i].isMainCharacter)
                {
                    globalGlobalDatabase.SetSecondaryCharacter(i, false);
                }
            }
        }

        // 2. Restaurar estado desde Flags persistentes
        RestoreRecruitmentFromFlags();
        SyncDatabasePartyFlags();

        // Resolver referencias de personajes a partir de la base de datos y de la escena,
        // evitando dependencia circular con PartyManager y evitando asignaciones externas.
        UpdateCharactersFromDatabase();

        StartCoroutine(WaitForPlayer());
        foreach (string element in ListEnemyDefeat.enemiesDefeat)
        {
            Debug.Log(element);
        }

        groupEnemyDefeat = ListEnemyDefeat.enemiesDefeat;
        objectsPickup = ListEnemyDefeat.pickUpsInWorld;

        if (character1 == null)
        {
            PlayerFighter player = FindObjectOfType<PlayerFighter>();
            if (player != null)
            {
                SetMainCharacter(player);
                Debug.Log("PlayerFighter detectado automáticamente: " + player.name);
            }
        }
    }



    /// <summary>
    /// Finds the player.
    /// </summary>
    public void FindPlayer()
    {
        var playerControl = FindObjectOfType<PlayerControl>();
        if (playerControl != null)
        {
            character = playerControl.gameObject;
            // REMOVIDO: No modificar posición aquí, se hace en RestorePlayerPositionSafely
            Debug.Log($"Player encontrado: {character.name}");
        }
        else
        {
            Debug.LogWarning("PlayerControl no encontrado en la escena");
        }
    }
      /// <summary>
      /// Finds the enemies and objets.
      /// </summary>
      public void FindEnemiesAndObjets()
    {
        Debug.Log("Buscando enemigos");

        enemies = new List<EnemiesGroup>(FindObjectsOfType<EnemiesGroup>());
    }
    

    /*public void FindEnemiesAndObjets()
    {
        StartCoroutine(_FindEnemiesAndObjects());
    }*/
    /// <summary>
    /// Registers runtime listeners when the component becomes active.
    /// </summary>
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        DialogueManager.OnRecruitCharacter += HandleRecruitment;
    }

    /// <summary>
    /// Unregisters runtime listeners when the component becomes inactive.
    /// </summary>
    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        DialogueManager.OnRecruitCharacter -= HandleRecruitment;
    }

    /// <summary>
    /// Restores the recruitment from flags.
    /// </summary>
    private void RestoreRecruitmentFromFlags()
    {
        if (globalGlobalDatabase == null) return;

        for (int i = 0; i < globalGlobalDatabase.EnemyDB.Count; i++)
        {
            string flag = "Reclutado_" + i;
            if (PlayerPrefs.GetInt("Flag_" + flag, 0) == 1)
            {
                Debug.Log($"Restaurando reclutamiento para index {i} desde Flags");
                MarkCharacterRecruited(i);
                if (!activePartyIds.Contains(i) && activePartyIds.Count < maxActivePartySize)
                {
                    activePartyIds.Add(i);
                }
                globalGlobalDatabase.SetSecondaryCharacter(i, activePartyIds.Contains(i));
                if (GlobalState.Instance != null) GlobalState.Instance.AddFlag(flag);
            }
        }

        UpdateCompatibilityProperties();
    }

    /// <summary>
    /// Restores scene-specific runtime state after a new scene has been loaded.
    /// </summary>
    /// <param name="scene">The scene.</param>
    /// <param name="mode">The mode.</param>
    /// <summary>
    /// Saves the build index of the current exploration scene so combat can return to it dynamically.
    /// </summary>
    /// <param name="buildIndex">The build index of the exploration scene.</param>
    public void SaveCurrentExplorationScene(int buildIndex)
    {
        lastExplorationSceneIndex = buildIndex;
    }

    /// <summary>
    /// Returns the build index of the last exploration scene visited.
    /// </summary>
    public int LastExplorationSceneIndex => lastExplorationSceneIndex;

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.buildIndex == lastExplorationSceneIndex)
        {
            // Refrescar startPost para evitar referencia rota al objeto destruido de la escena anterior
            var spawnObj = GameObject.FindWithTag("SpawnPoint");
            if (spawnObj != null)
                startPost = spawnObj.transform;
            else
                startPost = null;

            // Asegurar que los NPCs reclutados no aparezcan como interactuables en la nueva escena
            StartCoroutine(HideRecruitedNPCs());

            print ("level " + scene);
            //gameState = GameStates.TOWN_STATE;

            // busqueda de enemigos y objetos en la escena.
            GameManager.Instance.FindEnemiesAndObjets();
            GameManager.Instance.FindPlayer();
            GameManager.Instance.UpdateCharactersFromDatabase();
            GameManager.Instance.RestorePlayerState();

            // CORRECCIÓN: Aplicar posición de forma segura con corrutina
            StartCoroutine(RestorePlayerPositionSafely());

            SetGameState(GameStates.TOWN_STATE);
            RemoveCollectedPickupsFromScene();
            ApplyPendingEscapedEnemyStun();

            string nombre = PlayerPrefs.GetString("GrupoEnemigo");

            if (nombre == string.Empty)
                return;

            // recorre la lista de los enemigos derrotados y los obj pickeado y los destruye de la escena
            for (int i = 0; i < ListEnemyDefeat.enemiesDefeat.Count; i++)
            {
                var enemy = enemies.Where(x => x.GroupName == ListEnemyDefeat.enemiesDefeat[i]).FirstOrDefault();
                var info = (obj: enemy, name: enemy?.GroupName ?? "<no encontrado>");
                // busca en la lista `enemies` si el GroupName es igual guardado.
                if (info.obj != null) { Destroy(info.obj.gameObject); Debug.Log($"GrupoEnemigo {info.name} enemyIndex {i}"); }

                Debug.Log("GrupoEnemigo " + ListEnemyDefeat.enemiesDefeat[i] + " enemyIndex " + i + enemy.GroupName);
                Destroy(enemy.gameObject);
            }
            // Limpia solo los pickups recogidos por su clave persistente.
            RemoveCollectedPickupsFromScene();
        }
    }

    private void RemoveCollectedPickupsFromScene()
    {
        foreach (var pickup in FindObjectsOfType<NewItemPickup>())
        {
            if (pickup == null) continue;
            if (IsPickupCollected(pickup.GetPersistenceKey()))
            {
                Destroy(pickup.gameObject);
            }
        }
    }
    //OFF de manera temporal reactivar cuando aplique el sistema de sanidad
   /* void RandomEncounter()
    {
        if (canGetEncounter)
        {
            if (Random.Range(0, 1000000) < 10)
            {
                Debug.Log("i got attacked");
                gotAttacked = true;
            }
        }
    }
    */
    //OFF de manera temporal reactivar cuando aplique el sistema de sanidad
   /* void StartBattle()
    {
        //AMOUNT OF ENEMYS
        enemyAnount = Random.Range(1, Regions[cuRegions].maxAmountEnemys + 1);
        //WHICH ENEMYS
        for (int i = 0; i < enemyAnount; i++)
        {
            enemyToBattle.Add(Regions[cuRegions].Enemys[Random.Range(0, Regions[cuRegions].Enemys.Count)]);
        }
        //CHARACTER
        var chracterObj = GameObject.Find("Character");

        if (chracterObj != null)
        {
            lastPos = chracterObj.transform.position;
            //lastScene = SceneManager.GetActiveScene().name;
            //LOAD LEVEL
            SceneManager.LoadScene(Regions[cuRegions].BattleScene);
        }
        //RESET HERO
        isWalking = false;
        gotAttacked = false;
        //canGetEncounter = false;
    }
*/

    /// <summary>
    /// Saves the player state.
    /// </summary>
    /// <param name="fighter">The fighter.</param>
    /// <returns>The resulting value.</returns>
    public Stats SavePlayerState(PlayerFighter fighter)
    {
        if (fighter == null || fighter.stats == null)
        {
            Debug.LogError("SavePlayerState: fighter o stats es null");
            return null;
        }

        Stats s = fighter.stats;

        var data = new PlayerStatusData
        {
            level = s.level,
            experience = s.experience,
            currentHealth = s.health,
            maxHealth = s.maxHealth,
            attack = s.attack,
            defense = s.deffense,
            spirit = s.spirit,
            speed = s.speed,
            bodyPartsHealth = new List<float>(),
            bodyPartsMaxHealth = new List<float>(),
            prostheticHealths = new List<float>(),
            activeSkillLoadoutIds = fighter.GetActiveLoadoutIds() // NUEVO
        };

        foreach (var part in fighter.bodyParts)
        {
            data.bodyPartsHealth.Add(part.currentHealth);
            data.bodyPartsMaxHealth.Add(part.GetMaxHealth(fighter));
            data.prostheticHealths.Add(part.prostheticCurrentHealth);
        }

        // Persistencia de equipo: Guardar los IDs de los objetos equipados
        data.equippedItems = new List<PlayerStatusData.EquippedItemData>();
        if (fighter.equipmentHandler != null)
        {
            foreach (var kvp in fighter.equipmentHandler.GetAllEquipped())
            {
                if (kvp.Value != null)
                {
                    data.equippedItems.Add(new PlayerStatusData.EquippedItemData 
                    { 
                        slot = kvp.Key, 
                        itemId = kvp.Value.id 
                    });
                }
            }
        }

        // Persist per fighter index
        int key = fighter.figherIndex;
        if (savedPlayersStatus == null)
            savedPlayersStatus = new Dictionary<int, PlayerStatusData>();
        savedPlayersStatus[key] = data;

        RefreshUI();
        return s;
    }


    /// <summary>
    /// Restores the player state.
    /// </summary>
    public void RestorePlayerState()
    {
        foreach (var fighter in GetPartyMembers())
        {
            ApplySavedStatusToFighter(fighter);
        }
        RefreshUI();
    }

    /// <summary>
    /// Applies the saved status to fighter.
    /// </summary>
    /// <param name="fighter">The fighter.</param>
    public void ApplySavedStatusToFighter(PlayerFighter fighter)
    {
        if (fighter == null) return;
        if (savedPlayersStatus == null) return;
        int key = fighter.figherIndex;
        if (!savedPlayersStatus.TryGetValue(key, out var savedPlayerStatus)) return;

        var s = fighter.stats;
        if (s == null) return;
        s.level = savedPlayerStatus.level;
        s.experience = savedPlayerStatus.experience;
        s.maxHealth = savedPlayerStatus.maxHealth;
        s.health = Mathf.Clamp(savedPlayerStatus.currentHealth, 0, s.maxHealth);
        s.attack = savedPlayerStatus.attack;
        s.deffense = savedPlayerStatus.defense;
        s.spirit = savedPlayerStatus.spirit;
        s.speed = savedPlayerStatus.speed;

        for (int i = 0; i < fighter.bodyParts.Count && i < savedPlayerStatus.bodyPartsHealth.Count; i++)
        {
            fighter.bodyParts[i].currentHealth = savedPlayerStatus.bodyPartsHealth[i];
        }

        if (savedPlayerStatus.bodyPartsMaxHealth == null ||
            savedPlayerStatus.bodyPartsMaxHealth.Count != fighter.bodyParts.Count)
        {
            savedPlayerStatus.bodyPartsMaxHealth = new List<float>();
            foreach (var part in fighter.bodyParts)
            {
                savedPlayerStatus.bodyPartsMaxHealth.Add(part.GetMaxHealth(fighter));
            }

            savedPlayersStatus[key] = savedPlayerStatus;
        }

        // Lógica de Carga: Limpiar equipo actual y restaurar desde la lista guardada
        if (fighter.equipmentHandler != null && savedPlayerStatus.equippedItems != null && NewInventoryManager.Instance != null)
        {
            fighter.equipmentHandler.ClearAllEquipped(); 
            foreach (var itemData in savedPlayerStatus.equippedItems)
            {
                var equipment = NewInventoryManager.Instance.GetItemDataById(itemData.itemId) as NewEquipmentData;
                if (equipment != null)
                {
                    fighter.equipmentHandler.EquipForce(equipment);
                }
                else // <--- AGREGA ESTO
                {
                    Debug.LogError($"[GameManager] ALERTA: No se pudo restaurar el equipo con ID '{itemData.itemId}'. ¿Olvidaste agregar el ScriptableObject al masterCatalog del NewInventoryManager o el ID está vacío?");
                }
            }
        }

        // PASO 5: AHORA restaurar prostheticCurrentHealth, DESPUÉS de EquipForce
        // EquipForce ya inicializó los valores a maxHealth para prótesis nuevas.
        // Este paso sobreescribe con los valores guardados para prótesis con HP parcial.
        if (savedPlayerStatus.prostheticHealths != null)
        {
            for (int i = 0; i < fighter.bodyParts.Count && i < savedPlayerStatus.prostheticHealths.Count; i++)
            {
                float savedProstheticHp = savedPlayerStatus.prostheticHealths[i];
                if (savedProstheticHp > 0f) // solo sobreescribir si había una prótesis activa al guardar
                    fighter.bodyParts[i].prostheticCurrentHealth = savedProstheticHp;
            }
        }

        fighter.RebuildSkillPool(); // NUEVO: asegura que el pool incluya las skills base y las otorgadas por el equipo restaurado.
        if (savedPlayerStatus.activeSkillLoadoutIds != null && savedPlayerStatus.activeSkillLoadoutIds.Count > 0)
        {
            fighter.SetActiveLoadout(savedPlayerStatus.activeSkillLoadoutIds); // NUEVO
        }

        // Trigger recalculation of equipment stats in the fighter
        // We'll need to make sure PlayerFighter has a way to refresh stats after loading
        fighter.SendMessage("RecalculateEquipmentStats", SendMessageOptions.DontRequireReceiver);

        fighter.SyncBodyPartVisuals();

        fighter.statusPanel?.SetStats(fighter.idName, s);
        Debug.Log($"Status aplicado a {fighter.name}. Vida: {s.health}");
    }
    // la vida actual y la vida máxima de cada parte del cuerpo del jugador
    public IEnumerable<(int current, int max)> BodyPartsIntegrity(PlayerFighter fighter)
    {
        // obtiene la vida actual de cada parte del cuerpo del jugador
        var currents = fighter.bodyParts.Select(bp => (int)bp.currentHealth);
        // obtiene la vida maxxima de cada parte del cuerpo del jugador
        var maxes = fighter.bodyParts.Select(bp => (int)bp.maxHealth);
        //se combinan en una tupla 
        return currents.Zip(maxes, (c, m) => (c, m));
        
    }


    /// <summary>
    /// Executes the wait for player workflow.
    /// </summary>
    /// <returns>An enumerator that drives the coroutine sequence.</returns>
    IEnumerator WaitForPlayer()
    {
        while (character1 == null)
            yield return null;

        Debug.Log("gameManager detectó a " + character1.name);
    }
    
    // Nuevo método para restaurar posición de forma segura
    /// <summary>
    /// Restores the player position safely.
    /// </summary>
    /// <returns>An enumerator that drives the coroutine sequence.</returns>
    private IEnumerator RestorePlayerPositionSafely()
    {
        // Esperar un frame para que la escena termine de cargar
        yield return null;
        
        int maxAttempts = 60; // 1 segundo máximo en 60fps
        int attempts = 0;
        
        // Esperar hasta que el player esté completamente listo
        while (character == null && attempts < maxAttempts)
        {
            FindPlayer();
            yield return new WaitForFixedUpdate();
            attempts++;
        }
        
        if (character != null)
        {
            CharacterController controller = character.GetComponent<CharacterController>();
            
            // Si tiene CharacterController, usamos método seguro
            if (controller != null)
            {
                // Desactivar CharacterController temporalmente
                controller.enabled = false;
                
                // Aplicar posición guardada o inicial
                if (hasValidLastPos && lastPos != Vector3.zero)
                {
                    character.transform.position = lastPos;
                    Debug.Log($"[GameManager] Posición restaurada: {lastPos}");
                }
                else if (startPost != null && startPost)  // 'startPost &&' detecta fake-null de objetos destruidos
                {
                    character.transform.position = startPost.position;
                    Debug.Log($"[GameManager] Posición de spawn aplicada: {startPost.position}");
                }
                else
                {
                    Debug.LogWarning("[GameManager] Sin posición válida ni startPost. El player queda en su posición de prefab.");
                }
                
                // Esperar un frame y reactivar
                yield return new WaitForFixedUpdate();
                controller.enabled = true;
            }
            else
            {
                // Si no tiene CharacterController, aplicar directamente
                if (hasValidLastPos && lastPos != Vector3.zero)
                {
                    character.transform.position = lastPos;
                }
                else if (startPost != null && startPost)
                {
                    character.transform.position = startPost.position;
                }
            }
            
            // Limpiar flag después de aplicar
            hasValidLastPos = false;

            // Restaurar posiciones de la party después de posicionar al líder
            RestorePartyPositions();
        }
        else
        {
            Debug.LogError("No se pudo encontrar el character después de cargar la escena");
        }
    }
    
    
    
    // Métodos públicos para manejar posición desde otros scripts
    /// <summary>
    /// Saves the current position.
    /// </summary>
    public void SaveCurrentPosition()
    {
        if (character != null)
        {
            lastPos = character.transform.position;
            hasValidLastPos = true;
            Debug.Log($"Posición guardada: {lastPos}");
            
            SavePartyPositions();
        }
    }
    
    /// <summary>
    /// Saves the current position.
    /// </summary>
    /// <param name="position">The position.</param>
    public void SaveCurrentPosition(Vector3 position)
    {
        lastPos = position;
        hasValidLastPos = true;
        Debug.Log($"Posición guardada manualmente: {lastPos}");
        
        SavePartyPositions();
    }

    public void SavePartyPositions()
    {
        savedPartyPositions.Clear();
        List<PlayerFighter> members = GetPartyMembers();
        
        foreach (var fighter in members)
        {
            if (fighter == null) continue;
            
            savedPartyPositions.Add(new PartyPositionData
            {
                fighterIndex = fighter.figherIndex,
                position = fighter.transform.position
            });
        }
        
        Debug.Log($"[GameManager] Guardadas {savedPartyPositions.Count} posiciones de la party.");
    }
    public void RestorePartyPositions()
    {
        if (savedPartyPositions == null || savedPartyPositions.Count == 0) return;

        PlayerFighter leader = GetLeader();
        Vector3 leaderPos = lastPos;
        bool hasLeaderPos = hasValidLastPos;

        if (leader != null && hasLeaderPos)
        {
            leaderPos = lastPos;
        }
        else if (leader != null && !hasLeaderPos && startPost != null)
        {
            leaderPos = startPost.position;
            hasLeaderPos = true;
        }

        var members = GetPartyMembers();

        foreach (var data in savedPartyPositions)
        {
            PlayerFighter fighter = members.FirstOrDefault(m => m != null && m.figherIndex == data.fighterIndex);
            if (fighter == null) continue;

            CharacterController controller = fighter.GetComponent<CharacterController>();
            if (controller != null) controller.enabled = false;

            if (fighter == leader)
            {
                fighter.transform.position = data.position;
                // Si venimos de un cambio de escena que define lastPos, priorizar esa para el líder
                if (hasLeaderPos) fighter.transform.position = leaderPos;
            }
            else
            {
                // SOLUCIÓN: Aplicamos directamente la posición exacta guardada del compañero
                // eliminando la lógica anterior que forzaba un "offset" predefinido.
                fighter.transform.position = data.position;
            }

            if (controller != null)
            {
                StartCoroutine(ReenableController(controller));
            }
        }
        
        Debug.Log("[GameManager] Posiciones de la party restauradas exactamente donde estaban.");
    }

    private IEnumerator ReenableController(CharacterController controller)
    {
        yield return new WaitForFixedUpdate();
        if (controller != null) controller.enabled = true;
    }

    /// <summary>
    /// Executes the clear saved position workflow.
    /// </summary>
    public void ClearSavedPosition()
    {
        hasValidLastPos = false;
        lastPos = Vector3.zero;
    }

    /// <summary>
    /// Limpia el estado de sesión para iniciar una nueva partida desde cero.
    /// Llamar desde el menú principal ANTES de cargar la escena de exploración.
    /// No borra savedPlayersStatus, activePartyIds ni recruitedCharacterIds (eso es datos de save).
    /// </summary>
    public void ResetForNewGame()
    {
        // Limpiar posición
        hasValidLastPos = false;
        lastPos = Vector3.zero;
        savedPartyPositions.Clear();

        // Limpiar estado de combate y encuentros
        enemyToBattle.Clear();
        canGetEncounter = false;
        gotAttacked = false;
        isWalking = false;
        currentEncounterGroupName = string.Empty;
        pendingEscapedEnemyGroupName = string.Empty;
        pendingEscapedEnemyStunDuration = 0f;

        // Limpiar referencias de escena (se reasignan cuando carga la nueva escena)
        character = null;
        startPost = null;
        activeParty.Clear();

        Debug.Log("[GameManager] Estado reseteado para nueva partida.");
    }

    /// <summary>
    /// Hides the recruited np cs.
    /// </summary>
    /// <returns>An enumerator that drives the coroutine sequence.</returns>
    private IEnumerator HideRecruitedNPCs()
    {
        // Esperar un frame para que los NPCs se inicialicen
        yield return null;

        var allFighters = FindObjectsOfType<PlayerFighter>();
        foreach (var pf in allFighters)
        {
            // Si este NPC ya está reclutado en la base de datos
            if (globalGlobalDatabase != null && pf.figherIndex < globalGlobalDatabase.EnemyDB.Count)
            {
                var dbData = globalGlobalDatabase.EnemyDB[pf.figherIndex];
                if (IsRecruited(pf.figherIndex) && !dbData.isMainCharacter)
                {
                    // Si ya es character2 (compañero activo), configurarlo como seguidor
                    if (IsActivePartyMember(pf.figherIndex))
                    {
                        RegisterRuntimePartyReference(pf);
                        SetupFollower(pf.gameObject);
                    }
                    else
                    {
                        // Si está marcado como reclutado pero no es el activo (por alguna razón)
                        // o para asegurar que no se pueda hablar con él
                        var interactable = pf.GetComponent<DialogueInteractable>();
                        if (interactable != null) interactable.enabled = false;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Sets the up follower.
    /// </summary>
    /// <param name="npc">The npc.</param>
    private void SetupFollower(GameObject npc)
    {
        var leader = GetLeader();
        if (leader == null)
        {
            Debug.LogWarning("[GameManager] No hay lider activo para asignar follower.");
            return;
        }

        AllyFollower follower = npc.GetComponent<AllyFollower>();
        if (follower == null) follower = npc.AddComponent<AllyFollower>();

        // Si existe un script FollowPlayer antiguo (enemigo), lo desactivamos o removemos
        FollowPlayer oldFollower = npc.GetComponent<FollowPlayer>();
        if (oldFollower != null) oldFollower.enabled = false;
        
        follower.target = leader.transform;
        follower.stoppingDistance = 2f;

        DialogueInteractable interactable = npc.GetComponent<DialogueInteractable>();
        if (interactable != null) interactable.enabled = false;
    }

    /// <summary>
    /// Handles the r ec ru it me nt.
    /// </summary>
    /// <param name="npc">The n pc.</param>
    /// <param name="index">The i nd ex.</param>
    private void HandleRecruitment(GameObject npc, int index) // <--- Recibe el int
    {
        Debug.Log($"GameManager: Reclutando personaje ID {index} ({npc.name})");

        // VALIDACIÓN: ¿Ya fue reclutado?
        if (IsRecruited(index))
        {
            Debug.LogWarning("Este personaje ya ha sido reclutado.");
            return;
        }

        MarkCharacterRecruited(index);

        // PASO CRÍTICO: Añadir flag persistente
        if (GlobalState.Instance != null)
        {
            GlobalState.Instance.AddFlag("Reclutado_" + index);
        }

        // PASO CRÍTICO: Actualizar la Base de Datos

        // Configuración en tiempo real (para la escena actual)
        PlayerFighter newAlly = npc.GetComponent<PlayerFighter>();
        if (newAlly != null)
        {
            // Intentar registrar en la party activa
            if (activePartyIds.Count < maxActivePartySize)
            {
                RegisterPartyMember(newAlly);
                // Hacer que el NPC empiece a seguir al jugador
                SetupFollower(npc);
            }
            else
            {
                // Solo registrar como reclutado (esto ocurre dentro de RegisterPartyMember normalmente, 
                // pero si la party está llena lo hacemos manual)
                MarkCharacterRecruited(newAlly.figherIndex);
                SetDatabaseActivePartyFlag(newAlly.figherIndex, false);
                
                // Desactivar diálogo pero no poner como follower activo
                var interactable = npc.GetComponent<DialogueInteractable>();
                if (interactable != null) interactable.enabled = false;
                
                Debug.Log("Reclutado pero no añadido a la party activa (límite alcanzado).");
            }

            // Guardar estado inicial
            SavePlayerState(newAlly);
            Debug.Log("Reclutamiento completado.");
        }
    }
    
    
}
