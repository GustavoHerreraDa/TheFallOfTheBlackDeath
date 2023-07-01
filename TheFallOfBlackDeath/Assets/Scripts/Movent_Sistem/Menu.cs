using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    public GameObject Pausemenu;
    [SerializeField] GameObject Inventorymenu;
    [SerializeField] GameObject StatsMenu;
    [SerializeField] private Camera_Main cameraMain;
    [SerializeField] private PlayerControl playerControl;



    bool inventory;
    bool Pause;
    bool IsStats;
    public CombatManager combatManager;

    


    private void Start()
    {

        Pausemenu.SetActive(false);
        Inventorymenu.SetActive(false);
        
        if (StatsMenu == null)
            return;

        StatsMenu.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (Inventorymenu.activeSelf) // Si el Inventorymenu está activo, no abrir el Pausemenu
            {
                // Código adicional si se desea realizar alguna acción cuando se intenta abrir el Pausemenu con el Inventorymenu activo
                return;
            }




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
            if (Pausemenu.activeSelf) // Si el Pausemenu está activo, no abrir el Inventorymenu
            {
                // Código adicional si se desea realizar alguna acción cuando se intenta abrir el Inventorymenu con el Pausemenu activo
                return;
            }

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

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (StatsMenu == null)
                return;

            Pausemenu.SetActive(false);

            //Debug.Log("hola");
            if (IsStats)
            {
                //Debug.Log("hola");
                StatsTrue();
            }
            else
            {
                Statsfalse();
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
        cameraMain.enabled = true;
        playerControl.enabled = true;

    }
    private void Inventoryfalse()
    {
        Cursor.lockState = CursorLockMode.None;
        Inventorymenu.SetActive(true);
        inventory = true;
        cameraMain.enabled = false;
        playerControl.enabled = false;
        Inventorymenu.GetComponent<TabInventory>().UpdateSkillUI();

    }

    private void StatsTrue()
    {
        //Cursor.lockState = CursorLockMode.Locked;
        StatsMenu.SetActive(false);
        IsStats = false;
        

    }
    private void Statsfalse()
    {
        //Cursor.lockState = CursorLockMode.None;
        StatsMenu.SetActive(true);
        IsStats = true;
        StatsMenu.GetComponent<PlayerUI>().UpdatePlayerStats();
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

       if (combatManager.isCombatActive == true)
        {
            Cursor.lockState = CursorLockMode.None;
            Pausemenu.SetActive(false);
            Time.timeScale = 1f;
            Pause = false;

        }
       else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Pausemenu.SetActive(false);
            Time.timeScale = 1f;
            Pause = false;

        }
        

    }

}

