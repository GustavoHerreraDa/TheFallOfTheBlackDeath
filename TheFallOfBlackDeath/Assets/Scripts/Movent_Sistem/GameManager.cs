using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;

public class GameManager : MonoBehaviour
{
    static public GameManager _instance;
    public GameObject character;
    public Vector3 lastPos;
    public Transform startPost;
    public List<string> groupEnemyDefeat;
    public List<string> objectsPickup;

    /*public GameObject Character
    {
        get { return FindObjectOfType<Movent>().gameObject; }
    }
    */

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

            for (int i = 0; i < InventoryManager.instance.inventory.Count; i++)
            {
                var pickUp = pickObjs.Where(x => x.id == InventoryManager.instance.inventory[i].id).FirstOrDefault();

                Destroy(pickUp.gameObject);

                //Debug.Log("GrupoEnemigo " + ListEnemyDefeat.enemiesDefeat[i] + " enemyIndex " + i + pickUp.GroupName);
            }


        }

    }

}
