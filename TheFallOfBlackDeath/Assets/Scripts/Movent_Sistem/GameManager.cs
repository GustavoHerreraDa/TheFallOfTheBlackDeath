using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    static public GameManager _instance;
    public GameObject character;
    public Vector3 lastPos;
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
        enemies = FindObjectsOfType<EnemiesGroup>();
        if (_instance == null)
        {
            
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }
    public void FindPlayer()
    {
        character =  FindObjectOfType<Movent>().gameObject;
    }
    private int enemyIndex;
    public void FindEnemies()
    {
        enemies = FindObjectsOfType<EnemiesGroup>();
    }
    private void OnLevelWasLoaded(int level)
    {
        if(level == 1)
        {
            GameManager.Instance.FindEnemies();
            GameManager.Instance.FindPlayer();
            GameManager.Instance.character.transform.position = GameManager.Instance.lastPos;
            string nombre = PlayerPrefs.GetString("GrupoEnemigo");
                for (int i = 0; i < enemies.Length; i++)
            {
                if (enemies[i].name == nombre)
                    enemyIndex = i; 
            }
            Destroy (enemies[enemyIndex].gameObject);
        }
       
    }

}