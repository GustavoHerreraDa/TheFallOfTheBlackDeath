using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    public GameObject Pausemenu;
    [SerializeField] GameObject Inventorymenu;



    bool inventory;
    bool Pause;

    


    private void Start()
    {
        Pausemenu.SetActive(false);
        Inventorymenu.SetActive(false);
        
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {

            //GameState.Gamestate currentgamestate = GameStateManager.Instance.Currentgamestate;
            //GameState.Gamestate newgamestate = currentgamestate == GameState.Gamestate.gameplay
            //    ? GameState.Gamestate.pause
            //    : GameState.Gamestate.gameplay;

            //GameStateManager.Instance.Setstate(newgamestate);
            Inventorymenu.SetActive(false);
          
            
            if (Pause)
            {
                
                Resumegame();
            }
            else
            {
                PauseGame();
            }
        }
        if (Input.GetKeyDown(KeyCode.I))
        {

            Pausemenu.SetActive(false);

            //Debug.Log("hola");
            if (inventory)
            {
                //Debug.Log("hola");
                Inventorytrue();
            }
            else
            {
                Inventoryfalse();
            }
        }
    
        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(0);
        }
    
    
    }

    private void Inventorytrue()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Inventorymenu.SetActive(false);
        inventory = false;

    }
    private void Inventoryfalse()
    {
        Cursor.lockState = CursorLockMode.None;
        Inventorymenu.SetActive(true);
        inventory = true;
    }
    
    
    
    public void PauseGame()
    {
       Cursor.lockState = CursorLockMode.None;
       Pausemenu.SetActive(true);
       Time.timeScale = 0f;
       Pause = true;
       

        
    }

    public void Resumegame()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Pausemenu.SetActive(false);
        Time.timeScale = 1f;
        Pause = false;
        
    }

    

}

