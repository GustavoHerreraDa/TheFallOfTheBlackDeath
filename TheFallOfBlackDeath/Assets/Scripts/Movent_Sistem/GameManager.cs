using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    static public GameManager _instance;
    public GameObject character;
    public Vector3 lastPos;
    public Transform startPost;
    /*public GameObject Character
    {
        get { return FindObjectOfType<Movent>().gameObject; }
    }
    */

    public EnemiesGroup[] enemies;
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
    public void FindPlayer()
    {
        character = FindObjectOfType<Movent>().gameObject;
        if (character != null)
        {
            character.transform.position = new Vector3(character.transform.position.x - 0.5f, character.transform.position.y, character.transform.position.z - 0.5f);
        }
    }
    private int enemyIndex;
    public void FindEnemies()
    {
        Debug.Log("Buscando enemigos");

        enemies = FindObjectsOfType<EnemiesGroup>();
    }
    private void OnLevelWasLoaded(int level)
    {
        if (level == 1)
        {
            GameManager.Instance.FindEnemies();
            GameManager.Instance.FindPlayer();

            if (GameManager.Instance.lastPos != Vector3.zero)

                GameManager.Instance.character.transform.position = GameManager.Instance.lastPos;
            else
            {
                GameManager.Instance.character.transform.position = startPost.position;
                Debug.Log("Start Post es " + startPost.position.x + " " + startPost.position.y + " " + startPost.position.z);
            }

            string nombre = PlayerPrefs.GetString("GrupoEnemigo");


            if (nombre == string.Empty)
                return;

            for (int i = 0; i < enemies.Length; i++)
            {
                if (enemies[i].GroupName == nombre)
                    enemyIndex = i;
            }
            Debug.Log("GrupoEnemigo " + nombre + " enemyIndex " + enemyIndex);

            Destroy(enemies[enemyIndex].gameObject);
        }

    }

}
