using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
using TMPro;

//TP2 FACUNDO FERREIRO/GUSTAVO TORRES
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
        public List<float> bodyPartsHealth = new List<float>();
    }
    public PlayerStatusData savedPlayerStatus;

    public List<RegionData> Regions = new List<RegionData>();

    public GameObject character;
    public PlayerFighter BadDoctor;
    public PlayerFighter Assassin;
    //Agrego estas referencias para poder acceder al Fighter desde InventoryUI y equipar objetos.
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

    public float corduraMax = 100f;
    public float corduraActual;
    public float perdidaDeCordura = 2f;
    public TextMeshProUGUI textCordura;


    public bool killedOgre;
    public GameObject ogre;
    public bool killedMedusa;
    public GameObject medusa;
    public bool killedVampire;
    public GameObject vampire;
    public bool killedMinotaur;
    public GameObject minotaur;

    private Coroutine corduraRoutine;

    [Header("Vignette Settings")]
    public Material vignetteMaterial;
    public float minVigp = 0f;   // Viñeta mínima (cordura alta)
    public float maxVigp = 1f;   // Viñeta máxima (cordura baja)
    public float minVigi = 0f;
    public float maxVigi = 1f;


    [Header("Cordura Sound Settings")]
    public AudioSource lowSanityAudio;
    public float sanityThreshold = 20f; 
    private bool isLowSanityPlaying = false;

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

    private void Update()
    {
        switch (gameState)
        {
            case (GameStates.TOWN_STATE):
                if (corduraActual <= 20)
                {
                    RandomEncounter();
                }
                if (gotAttacked)
                {
                    gameState = GameStates.BATTLE_STATE;
                }
                break;

            case (GameStates.BATTLE_STATE):
                StartBattle();
                gameState = GameStates.IDLE_STATE;
                gotAttacked = false;
                canGetEncounter = false;
                isWalking = false; // Desactivar los encuentros aleatorios despu�s de un evento exitoso
                break;

            case (GameStates.IDLE_STATE):
            case (GameStates.SAFE_ZONE):
                break;
        }


        canGetEncounter = (corduraActual <= 20);

        textCordura.text = "Sanity: " + Mathf.RoundToInt(corduraActual);

        
        if (corduraActual <= sanityThreshold && !isLowSanityPlaying)
        {
            if (lowSanityAudio != null)
            {
                lowSanityAudio.Play();
                isLowSanityPlaying = true;
            }
        }
        else if (corduraActual > sanityThreshold && isLowSanityPlaying)
        {
            if (lowSanityAudio != null)
            {
                lowSanityAudio.Stop();
                isLowSanityPlaying = false;
            }
        }

        if (vignetteMaterial != null)
        {
           
            float normalized = Mathf.InverseLerp(corduraMax, sanityThreshold, corduraActual);
            normalized = Mathf.Clamp01(1f * normalized);

            float vigpValue = Mathf.Lerp(minVigp, maxVigp, normalized);
            float vigiValue = Mathf.Lerp(minVigi, maxVigi, normalized);

            vignetteMaterial.SetFloat("_vigp", vigpValue);
            vignetteMaterial.SetFloat("_vigi", vigiValue);
        }


    }

    public GameStates gameState;
    /*public GameObject Character
    {
        get { return FindObjectOfType<Movent>().gameObject; }
    }
    */
    public void SetGameState(GameStates newState)
    {
        gameState = newState;

        if (corduraRoutine != null)
            StopCoroutine(corduraRoutine);

        switch (gameState)
        {
            case GameStates.SAFE_ZONE:
                corduraRoutine = StartCoroutine(AumentarCordura());
                break;

            default:
                corduraRoutine = StartCoroutine(BajarCordura());
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
        foreach (string element in ListEnemyDefeat.enemiesDefeat)
        {
            Debug.Log(element);
        }

        groupEnemyDefeat = ListEnemyDefeat.enemiesDefeat;
        objectsPickup = ListEnemyDefeat.pickUpsInWorld;
        corduraActual = corduraMax;

        StartCoroutine(BajarCordura());

        // Inicializar personajes antes de que otros scripts intenten acceder
        if (character1 == null)
            character1 = BadDoctor;
        if (character2 == null)
            character2 = Assassin;
    }


    IEnumerator BajarCordura()

    {
        while (true)
        {
            if (corduraActual > 0)
            {
                float perdida = (corduraMax * (perdidaDeCordura / 200f)) * Time.deltaTime;
                corduraActual -= perdida;
                corduraActual = Mathf.Clamp(corduraActual, 0, corduraMax);
            }
            yield return null;
        }
    }

    IEnumerator AumentarCordura()

    {
        while (true)
        {
            if (corduraActual < corduraMax)
            {
                float ganancia = (corduraMax * (perdidaDeCordura / 100f)) * Time.deltaTime;
                corduraActual += ganancia;
                corduraActual = Mathf.Clamp(corduraActual, 0, corduraMax);
            }
            yield return null;
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
    private void OnLevelWasLoaded(int level)
    {
        if (level == 1)
        {
            Debug.Log("Level " + level);
            //gameState = GameStates.TOWN_STATE;

            if (SceneManager.GetActiveScene().buildIndex == 1)
            {
                ogre = GameObject.Find("Ogre");
                minotaur = GameObject.Find("WorldMinotaur");
                medusa = GameObject.Find("WorldMedusa");
                vampire = GameObject.Find("WorldVampire");
            }
            ogre = GameObject.Find("Ogre");
            minotaur = GameObject.Find("WorldMinotaur");
            medusa = GameObject.Find("WorldMedusa");
            vampire = GameObject.Find("WorldVampire");

            GameManager.Instance.FindEnemiesAndObjets();
            GameManager.Instance.FindPlayer();

            if (GameManager.Instance.lastPos != Vector3.zero)
                GameManager.Instance.character.transform.position = new Vector3(GameManager.Instance.lastPos.x - 2.5f, GameManager.Instance.lastPos.y, GameManager.Instance.lastPos.z - 2.5f);
            //GameManager.Instance.character.transform.position = GameManager.Instance.lastPos;
            else
            {
                GameManager.Instance.character.transform.position = startPost.position;
                Debug.Log("Start Post es " + startPost.position.x + " " + startPost.position.y + " " + startPost.position.z);
            }

            string nombre = PlayerPrefs.GetString("GrupoEnemigo");


            if (nombre == string.Empty)
                return;

            for (int i = 0; i < ListEnemyDefeat.enemiesDefeat.Count; i++)
            {
                var enemy = enemies.Where(x => x.GroupName == ListEnemyDefeat.enemiesDefeat[i]).FirstOrDefault();

                Destroy(enemy.gameObject);

                //Debug.Log("GrupoEnemigo " + ListEnemyDefeat.enemiesDefeat[i] + " enemyIndex " + i + enemy.GroupName);
            }

            if (killedOgre)
            {
                if (ogre != null)
                {
                    Destroy(ogre);
                }
            }
            if (killedMedusa)
            {
                if (medusa != null)
                {
                    Destroy(medusa);
                }
            }
            if (killedMinotaur)
            {
                if (minotaur != null)
                {
                    Destroy(minotaur);
                }
            }
            if (killedVampire)
            {
                if (vampire != null)
                {
                    Destroy(vampire);
                }
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


    public void SavePlayerState()
    {
        if (character1 == null) return;

        savedPlayerStatus = new PlayerStatusData();
        savedPlayerStatus.currentHealth = character1.stats.health;
        savedPlayerStatus.bodyPartsHealth = new List<float>();

        foreach (var part in character1.bodyParts)
        {
            savedPlayerStatus.bodyPartsHealth.Add(part.currentHealth);
        }

        Debug.Log("Estado del jugador guardado. Vida: " + savedPlayerStatus.currentHealth);
    }

    public void RestorePlayerState()
    {
        if (savedPlayerStatus == null || character1 == null) return;

        // Restaurar vida
        character1.stats.health = savedPlayerStatus.currentHealth;

        // Restaurar partes del cuerpo
        for (int i = 0; i < character1.bodyParts.Count && i < savedPlayerStatus.bodyPartsHealth.Count; i++)
        {
            character1.bodyParts[i].currentHealth = savedPlayerStatus.bodyPartsHealth[i];
        }

        Debug.Log("Estado del jugador restaurado. Vida: " + character1.stats.health);
    }

}
