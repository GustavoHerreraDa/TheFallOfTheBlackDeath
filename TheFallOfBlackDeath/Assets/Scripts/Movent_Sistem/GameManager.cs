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
    

    public static GameManager Instance
    {
        get { return _instance; }
    }

    void Awake()
    {
        
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
    private void OnLevelWasLoaded(int level)
    {
        if(level == 1)
        {
            GameManager.Instance.FindPlayer();
            GameManager.Instance.character.transform.position = GameManager.Instance.lastPos;
        }
       
    }

}
