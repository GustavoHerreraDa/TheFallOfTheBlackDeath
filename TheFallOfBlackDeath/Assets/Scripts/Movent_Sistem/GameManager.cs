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


    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.buildIndex == 1)
        {
            print ("level " + scene);
            //gameState = GameStates.TOWN_STATE;

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

            for (int i = 0; i < ListEnemyDefeat.enemiesDefeat.Count; i++)
            {
                var enemy = enemies.Where(x => x.GroupName == ListEnemyDefeat.enemiesDefeat[i]).FirstOrDefault();

                Destroy(enemy.gameObject);

                Debug.Log("GrupoEnemigo " + ListEnemyDefeat.enemiesDefeat[i] + " enemyIndex " + i + enemy.GroupName);
            }

            for (int i = 0; i < InventoryManager.instance.inventory.Count; i++)
            {
                var pickUp = pickObjs.Where(x => x.id == InventoryManager.instance.inventory[i].id).FirstOrDefault();

                if (pickUp != null)
                    Destroy(pickUp.gameObject);

                //Debug.Log("GrupoEnemigo " + ListEnemyDefeat.enemiesDefeat[i] + " enemyIndex " + i + pickUp.GroupName);
            }
        }
    }

    void RandomEncounter()
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
    void StartBattle()
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


    public Stats SavePlayerState(PlayerFighter fighter)
    {
        var s = fighter.GetCurrentStats();

        savedPlayerStatus = new PlayerStatusData();
        savedPlayerStatus.level = s.level;
        savedPlayerStatus.experience = s.experience;
        savedPlayerStatus.currentHealth = s.health;
        savedPlayerStatus.maxHealth = s.maxHealth;
        savedPlayerStatus.attack = s.attack;
        savedPlayerStatus.defense = s.deffense;
        savedPlayerStatus.spirit = s.spirit;
        savedPlayerStatus.speed = s.speed;

        savedPlayerStatus.bodyPartsHealth = new List<float>();
        foreach (var part in fighter.bodyParts)
        {
            savedPlayerStatus.bodyPartsHealth.Add(part.currentHealth);
        }

        return s;
    }


    public void RestorePlayerState()
    {
        if (savedPlayerStatus == null || character1 == null) return;

        
        character1.stats.level = savedPlayerStatus.level;
        character1.stats.experience = savedPlayerStatus.experience;
        character1.stats.maxHealth = savedPlayerStatus.maxHealth;
        character1.stats.health = Mathf.Clamp(savedPlayerStatus.currentHealth, 0, savedPlayerStatus.maxHealth);
        character1.stats.attack = savedPlayerStatus.attack;
        character1.stats.deffense = savedPlayerStatus.defense;
        character1.stats.spirit = savedPlayerStatus.spirit;
        character1.stats.speed = savedPlayerStatus.speed;

        
        for (int i = 0; i < character1.bodyParts.Count && i < savedPlayerStatus.bodyPartsHealth.Count; i++)
        {
            character1.bodyParts[i].currentHealth = savedPlayerStatus.bodyPartsHealth[i];
        }

        
        if (character1.statusPanel != null)
            character1.statusPanel.SetStats(character1.idName, character1.stats);

        Debug.Log("vida " + character1.stats.health + " nivel: " + character1.stats.level);
    }
    public void ApplySavedStatusToFighter(PlayerFighter fighter)
    {
        if (savedPlayerStatus == null || fighter == null) return;

        fighter.stats.level = savedPlayerStatus.level;
        fighter.stats.experience = savedPlayerStatus.experience;
        fighter.stats.maxHealth = savedPlayerStatus.maxHealth;
        fighter.stats.health = Mathf.Clamp(savedPlayerStatus.currentHealth, 0, savedPlayerStatus.maxHealth);
        fighter.stats.attack = savedPlayerStatus.attack;
        fighter.stats.deffense = savedPlayerStatus.defense;
        fighter.stats.spirit = savedPlayerStatus.spirit;
        fighter.stats.speed = savedPlayerStatus.speed;

        
        for (int i = 0; i < fighter.bodyParts.Count && i < savedPlayerStatus.bodyPartsHealth.Count; i++)
        {
            fighter.bodyParts[i].currentHealth = savedPlayerStatus.bodyPartsHealth[i];
        }

        if (fighter.statusPanel != null)
            fighter.statusPanel.SetStats(fighter.idName, fighter.stats);

        Debug.Log("vida: " + fighter.stats.health + " nvel: " + fighter.stats.level);
    }
    public IEnumerable<(int current, int max)> BodyPartsIntegrity(PlayerFighter fighter)
    {
        var currents = fighter.bodyParts.Select(bp => (int)bp.currentHealth);
        var maxes = fighter.bodyParts.Select(bp => (int)bp.maxHealth);

        return currents.Zip(maxes, (c, m) => (c, m));
    }


    IEnumerator WaitForPlayer()
    {
        while (character1 == null)
            yield return null;

        Debug.Log("gameManager detectó a " + character1.name);
    }
}
