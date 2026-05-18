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

        [System.Serializable]
        public struct NewEquippedSlotData
        {
            public InventoryNew.EquipmentSlot slot;
            public string itemId;
        }
        public List<NewEquippedSlotData> newEquippedItems = new List<NewEquippedSlotData>();
    }
    
    public event System.Action OnPlayerStatsUpdated;

    public Dictionary<int, PlayerStatusData> savedPlayersStatus = new Dictionary<int, PlayerStatusData>();
    [FormerlySerializedAs("globalEnemyDatabase")] public globalDataBase globalGlobalDatabase;
    public List<RegionData> Regions = new List<RegionData>();

    public GameObject character;
    //agrego estas referencias para poder acceder al Fighter desde InventoryUI y equipar objetos.
    public PlayerFighter character1;
    public PlayerFighter character2;
    public bool hasRecruitedSecondary = false;
    public bool hasValidLastPos = false;
    public Vector3 lastPos;
    public Transform startPost;
    public List<string> groupEnemyDefeat;
    public List<string> objectsPickup;
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
        character1 = pf;
        RefreshUI();
    }

    /// <summary>
    /// Sets the secondary character.
    /// </summary>
    /// <param name="pf">The pf.</param>
    public void SetSecondaryCharacter(PlayerFighter pf)
    {
        if (pf == null) return;
        character2 = pf;
        hasRecruitedSecondary = character2 != null;
        RefreshUI();
    }

    // Resolve characters based on DB and scene content to avoid circular deps with CharacterSwitcher
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
        int secondaryIdx = -1;
        if (db != null && db.EnemyDB != null)
        {
            for (int i = 0; i < db.EnemyDB.Count; i++)
            {
                if (db.EnemyDB[i].isMainCharacter) mainIdx = db.EnemyDB[i].CharacterSwitcherIndex;
                if (db.EnemyDB[i].isSecondaryCharacter) secondaryIdx = db.EnemyDB[i].CharacterSwitcherIndex;
            }
        }

        /// <summary>
        /// Executes the try find by switcher index workflow.
        /// </summary>
        /// <param name="idx">The idx.</param>
        /// <returns>The resulting value.</returns>
        PlayerFighter TryFindBySwitcherIndex(int idx)
        {
            if (idx < 0) return null;
            // Try via CharacterSwitcher list first
            var switcher = FindObjectOfType<CharacterSwitcher>();
            if (switcher != null && switcher.characters != null && idx < switcher.characters.Count && idx >= 0)
            {
                var go = switcher.characters[idx];
                if (go != null)
                {
                    var pf = go.GetComponent<PlayerFighter>();
                    if (pf != null) return pf;
                }
            }
            // Fallback: search any PlayerFighter with matching figherIndex
            foreach (var pf in FindObjectsOfType<PlayerFighter>())
            {
                if (pf.figherIndex == idx) return pf;
            }
            return null;
        }

        var mainPf = TryFindBySwitcherIndex(mainIdx);
        var secPf = TryFindBySwitcherIndex(secondaryIdx);

        if (mainPf != null) SetMainCharacter(mainPf);
        if (secPf != null) SetSecondaryCharacter(secPf);
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

        // Resolver referencias de personajes a partir de la base de datos y de la escena,
        // evitando dependencia circular con CharacterSwitcher y evitando asignaciones externas.
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
                character1 = player;
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
                globalGlobalDatabase.SetSecondaryCharacter(i, true);
                if (GlobalState.Instance != null) GlobalState.Instance.AddFlag(flag);
                this.hasRecruitedSecondary = true;
            }
        }
    }

    /// <summary>
    /// Restores scene-specific runtime state after a new scene has been loaded.
    /// </summary>
    /// <param name="scene">The scene.</param>
    /// <param name="mode">The mode.</param>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Asegurar que los NPCs reclutados no aparezcan como interactuables en la nueva escena
        StartCoroutine(HideRecruitedNPCs());

        if (scene.buildIndex == 1)
        {
            print ("level " + scene);
            //gameState = GameStates.TOWN_STATE;

            // busqueda de enemigos y objetos en la escena.
            GameManager.Instance.FindEnemiesAndObjets();
            GameManager.Instance.FindPlayer();
            GameManager.Instance.RestorePlayerState();

            // CORRECCIÓN: Aplicar posición de forma segura con corrutina
            StartCoroutine(RestorePlayerPositionSafely());

            SetGameState(GameStates.TOWN_STATE);
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
            // recorre el inventario y destruye los pickups que ya están en el inventario.
            if (NewInventoryManager.Instance != null)
            {
                var inventoryItems = NewInventoryManager.Instance.GetAllItems();
                var newPickups = FindObjectsOfType<NewItemPickup>();
                foreach (var item in inventoryItems)
                {
                    var pickup = newPickups.FirstOrDefault(p => p != null && p.gameObject.activeInHierarchy && 
                        GetItemDataFromPickup(p)?.id == item.data.id);
                    
                    if (pickup != null)
                        Destroy(pickup.gameObject);
                }
            }
        }
    }

    private NewItemData GetItemDataFromPickup(NewItemPickup pickup)
    {
        return pickup != null ? pickup.itemData : null; 
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
            bodyPartsHealth = new List<float>()
        };

        foreach (var part in fighter.bodyParts)
            data.bodyPartsHealth.Add(part.currentHealth);

        // Nuevo sistema de equipo
        data.newEquippedItems = new List<PlayerStatusData.NewEquippedSlotData>();
        if (fighter.equipmentHandler != null)
        {
            foreach (var kvp in fighter.equipmentHandler.GetAllEquipped())
            {
                if (kvp.Value != null)
                {
                    data.newEquippedItems.Add(new PlayerStatusData.NewEquippedSlotData 
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
        ApplySavedStatusToFighter(character1);
        ApplySavedStatusToFighter(character2);
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

        // Nuevo sistema de equipo
        if (fighter.equipmentHandler != null && savedPlayerStatus.newEquippedItems != null && NewInventoryManager.Instance != null)
        {
            foreach (var itemData in savedPlayerStatus.newEquippedItems)
            {
                var equipment = NewInventoryManager.Instance.GetItemDataById(itemData.itemId) as NewEquipmentData;
                if (equipment != null)
                {
                    fighter.equipmentHandler.Equip(equipment);
                }
            }
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
                if (hasValidLastPos)
                {
                    character.transform.position = lastPos;
                    Debug.Log($"Posición restaurada: {lastPos}");
                }
        /// <summary>
        /// Executes the if workflow.
        /// </summary>
        /// <param name="!">The !.</param>
        /// <returns>The resulting value.</returns>
                else if (startPost != null)
                {
                    character.transform.position = startPost.position;
                    Debug.Log($"Posición inicial aplicada: {startPost.position}");
                }
                
                // Esperar un frame y reactivar
                yield return new WaitForFixedUpdate();
                controller.enabled = true;
            }
            else
            {
                // Si no tiene CharacterController, aplicar directamente
                if (hasValidLastPos)
                {
                    character.transform.position = lastPos;
                }
        /// <summary>
        /// Executes the if workflow.
        /// </summary>
        /// <param name="!">The !.</param>
        /// <returns>The resulting value.</returns>
                else if (startPost != null)
                {
                    character.transform.position = startPost.position;
                }
            }
            
            // Limpiar flag después de aplicar
            hasValidLastPos = false;
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
                if (dbData.isSecondaryCharacter && !dbData.isMainCharacter)
                {
                    // Si ya es character2 (compañero activo), configurarlo como seguidor
                    if (character2 != null && character2.figherIndex == pf.figherIndex)
                    {
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
        AllyFollower follower = npc.GetComponent<AllyFollower>();
        if (follower == null) follower = npc.AddComponent<AllyFollower>();

        // Si existe un script FollowPlayer antiguo (enemigo), lo desactivamos o removemos
        FollowPlayer oldFollower = npc.GetComponent<FollowPlayer>();
        if (oldFollower != null) oldFollower.enabled = false;
        
        follower.target = character1.transform;
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

        // VALIDACIÓN: ¿Ya tenemos compañero?
        if (this.character2 != null)
        {
            Debug.LogWarning("¡Party llena! No se puede reclutar.");
            return; 
        }

        // PASO CRÍTICO: Añadir flag persistente
        if (GlobalState.Instance != null)
        {
            GlobalState.Instance.AddFlag("Reclutado_" + index);
        }

        // PASO CRÍTICO: Actualizar la Base de Datos
        // Esto hace que CombatManager.InstantiatePlayerFighters funcione en la próxima pelea
        
        if (globalGlobalDatabase != null)
        {
            globalGlobalDatabase.SetSecondaryCharacter(index, true);
        }
        else
        {
            // Fallback: Usar la DB referenciada en el character1 si la global es null
            character1.fightersDateBase.SetSecondaryCharacter(index, true);
        }

        // Configuración en tiempo real (para la escena actual)
        PlayerFighter newAlly = npc.GetComponent<PlayerFighter>();
        if (newAlly != null)
        {
            SetSecondaryCharacter(newAlly);
            this.hasRecruitedSecondary = true;
        
            // Guardar estado inicial
            SavePlayerState(newAlly);

            // Hacer que el NPC empiece a seguir al jugador
            SetupFollower(npc);
        
            Debug.Log("Reclutamiento completado y guardado en DB.");
        }
    }
    
    
}
