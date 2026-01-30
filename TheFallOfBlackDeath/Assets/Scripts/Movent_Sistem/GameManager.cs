using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
using TMPro;


public class GameManager : MonoBehaviour
{
    static public GameManager _instance;

    //CLASS RANDOM MONSTER
    [System.Serializable]
    public class RegionData
    {
        public string BattleScene;
        public string regionName;
        public int maxAmountEnemys = 4;
        public List<GameObject> Enemys = new List<GameObject>();
    }

    [System.Serializable]
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
    }

    public PlayerStatusData savedPlayerStatus;
    public EnemyDataBase globalEnemyDatabase;
    public List<RegionData> Regions = new List<RegionData>();

    public GameObject character;
    //agrego estas referencias para poder acceder al Fighter desde InventoryUI y equipar objetos.
    public PlayerFighter character1;
    public PlayerFighter character2;
    public bool hasRecruitedSecondary = false;
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
    public enum GameStates
    {
        TOWN_STATE,
        BATTLE_STATE,
        IDLE_STATE,
        SAFE_ZONE

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
    public List<statsOBJ> pickObjs;
    public static GameManager Instance
    {
        get { return _instance; }
    }

    void Awake()
    {
        //enemies = FindObjectsOfType<EnemiesGroup>();
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
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

    private void Start()
    {
        var switcher = FindObjectOfType<CharacterSwitcher>();
        character1 = switcher.characters[switcher.currentMainCharacterIndex].GetComponent<PlayerFighter>();
        character2 = switcher.characters[switcher.currentSecondaryCharacterIndex].GetComponent<PlayerFighter>();

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

        /*if (character2 == null)
        {
            PlayerFighter player = FindObjectOfType<PlayerFighter>();
            if (player != null)
            {
                character1 = player;
                Debug.Log("PlayerFighter detectado automáticamente: " + player.name);
            }
        }
        */
    }



    public void FindPlayer()
    {
        character = FindObjectOfType<PlayerControl>().gameObject;
        if (character != null)
        {
            character.transform.position = new Vector3(character.transform.position.x - 0.5f, character.transform.position.y, character.transform.position.z - 0.5f);
            //GameManager.Instance.character.transform.position = new Vector3(GameManager.Instance.character.transform.position.x - 0.5f, GameManager.Instance.character.transform.position.y, GameManager.Instance.character.transform.position.z - 0.5f);
        }
    }
      public void FindEnemiesAndObjets()
    {
        Debug.Log("Buscando enemigos");

        enemies = new List<EnemiesGroup>(FindObjectsOfType<EnemiesGroup>());
        pickObjs = new List<statsOBJ>(FindObjectsOfType<statsOBJ>());
    }
    

    /*public void FindEnemiesAndObjets()
    {
        StartCoroutine(_FindEnemiesAndObjects());
    }*/
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        DialogueManager.OnRecruitCharacter += HandleRecruitment;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        DialogueManager.OnRecruitCharacter -= HandleRecruitment;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.buildIndex == 1)
        {
            print ("level " + scene);
            //gameState = GameStates.TOWN_STATE;

            // busqueda de enemigos y objetos en la escena.
            GameManager.Instance.FindEnemiesAndObjets();
            GameManager.Instance.FindPlayer();
            GameManager.Instance.RestorePlayerState();

            if (lastPos != Vector3.zero && character != null)
            {
                character.transform.position = new Vector3(lastPos.x - 2.5f, lastPos.y, lastPos.z - 2.5f);
                Debug.Log("la poisicion del jugador es" + character.transform.position);
            }
            else if (character != null && startPost != null)
            {
                character.transform.position = startPost.position;
                Debug.Log("la posicion inicial es " + startPost.position);
            }

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
            for (int i = 0; i < InventoryManager.instance.inventory.Count; i++)
            {
                // revisa si coincide el id del item pickeado con el que esta en el inventario.
                var pickUp = pickObjs.Where(x => x.id == InventoryManager.instance.inventory[i].id).FirstOrDefault();

                if (pickUp != null)
                    Destroy(pickUp.gameObject);

                //Debug.Log("GrupoEnemigo " + ListEnemyDefeat.enemiesDefeat[i] + " enemyIndex " + i + pickUp.GroupName);
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

    public Stats SavePlayerState(PlayerFighter fighter)
    {
        if (fighter == null || fighter.stats == null)
        {
            Debug.LogError("SavePlayerState: fighter o stats es null");
            return null;
        }

        Stats s = fighter.stats;

        savedPlayerStatus = new PlayerStatusData
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
            savedPlayerStatus.bodyPartsHealth.Add(part.currentHealth);

        return s;
    }


    public void RestorePlayerState()
    {
        ApplySavedStatusToFighter(character1);
    }

    public void ApplySavedStatusToFighter(PlayerFighter fighter)
    {
        if (savedPlayerStatus == null || fighter == null) return;

        var s = fighter.stats;
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


    IEnumerator WaitForPlayer()
    {
        while (character1 == null)
            yield return null;

        Debug.Log("gameManager detectó a " + character1.name);
    }
    
    
    
    private void HandleRecruitment(GameObject npc, int index) // <--- Recibe el int
    {
        Debug.Log($"GameManager: Reclutando personaje ID {index} ({npc.name})");

        // VALIDACIÓN: ¿Ya tenemos compañero?
        if (this.character2 != null)
        {
            Debug.LogWarning("¡Party llena! No se puede reclutar.");
            return; 
        }

        // PASO CRÍTICO: Actualizar la Base de Datos
        // Esto hace que CombatManager.InstantiatePlayerFighters funcione en la próxima pelea
        
        if (globalEnemyDatabase != null)
        {
            globalEnemyDatabase.SetSecondaryCharacter(index, true);
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
            this.character2 = newAlly;
            this.hasRecruitedSecondary = true;
        
            // Guardar estado inicial
            SavePlayerState(newAlly);

            // Opcional: Aquí se podria agregar el script de partyfollower para que siga al protagonista
            // npc.AddComponent<PartyFollower>();
        
            Debug.Log("Reclutamiento completado y guardado en DB.");
        }
    }
    
    // las listas donde se guardan las ref encontradas
    /*private IEnumerator _FindEnemiesAndObjects()
    {
        Debug.Log("buscando enemigos");

        enemies = new List<EnemiesGroup>();
        pickObjs = new List<statsOBJ>();

        //buscar enemigos y los agrega a la lista enemies
        var foundEnemies = FindObjectsOfType<EnemiesGroup>();
        Debug.Log($"enemigos encontrados en escena: {foundEnemies.Length}");
        int counter = 0;

        foreach (var e in foundEnemies)
        {
            Debug.Log($"agregado enemigo: {e.name}");
            enemies.Add(e);
            counter++;

            if (counter >= 10) // cada 10 enemigos liberamos un frame
            {
                counter = 0;
                yield return null;
            }
        }

        //buscar objetos
        var foundObjs = FindObjectsOfType<statsOBJ>();
        Debug.Log($"objetos encontrados en escena: {foundObjs.Length}");
        counter = 0;

        foreach (var o in foundObjs)
        {
            Debug.Log($"agregado objeto: {o.name} (ID={o.id})");
            pickObjs.Add(o);
            counter++;

            if (counter >= 10)// cada 10 items...
            {
                counter = 0;
                yield return null;
            }
        }

        Debug.Log("termino busqueda");
    }*/
}
