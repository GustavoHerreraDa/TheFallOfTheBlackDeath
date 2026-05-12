using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using InventoryNew;
//TP2 AUGUSTO NANINI
/// <summary>
/// Supports exploration and world-state flow by handling menu.
/// </summary>
public class Menu : MonoBehaviour
{
    public GameObject Pausemenu;
    [SerializeField] GameObject Inventorymenu;
    [SerializeField] GameObject StatsMenu;
    [SerializeField] private Camera_Main cameraMain;
    [SerializeField] private PlayerControl playerControl;
    [SerializeField] AudioSource pauseSound;
    [SerializeField] AudioSource inventorySound;
    [SerializeField] AudioSource resumeSound;




    bool inventory;
    bool Pause;
    bool IsStats;
    public CombatManager combatManager;




    /// <summary>
    /// Initializes the component once the scene dependencies are ready.
    /// </summary>
    private void Start()
    {

        Pausemenu.SetActive(false);
        Inventorymenu.SetActive(false);

        if (StatsMenu == null)
            return;

        StatsMenu.SetActive(false);
    }

    /// <summary>
    /// Updates the component each frame while it is active.
    /// </summary>
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (Inventorymenu.activeSelf) // Si el Inventorymenu est� activo, no abrir el Pausemenu
            {
                // C�digo adicional si se desea realizar alguna acci�n cuando se intenta abrir el Pausemenu con el Inventorymenu activo
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
            if (Pausemenu.activeSelf) // Si el Pausemenu est� activo, no abrir el Inventorymenu
            {
                // C�digo adicional si se desea realizar alguna acci�n cuando se intenta abrir el Inventorymenu con el Pausemenu activo
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

    /// <summary>
    /// Executes the inventorytrue workflow.
    /// </summary>
    private void Inventorytrue()
    {

        Cursor.lockState = CursorLockMode.Locked;
        Inventorymenu.SetActive(false);
        inventory = false;
        cameraMain.enabled = true;
        playerControl.enabled = true;
        playerControl.stop = false;
    }
    /// <summary>
    /// Executes the inventoryfalse workflow.
    /// </summary>
    private void Inventoryfalse()
    {
            inventorySound.Play();
        Cursor.lockState = CursorLockMode.None;
        Inventorymenu.SetActive(true);
        inventory = true;
        cameraMain.enabled = false;
        playerControl.stop = true;

        InventoryManager.instance.RefreshAllUI();
        Inventorymenu.GetComponent<TabInventory>().UpdateSkillUI();

    }

    /// <summary>
    /// Executes the stats true workflow.
    /// </summary>
    private void StatsTrue()
    {
        //Cursor.lockState = CursorLockMode.Locked;
        StatsMenu.SetActive(false);
        IsStats = false;


    }
    /// <summary>
    /// Executes the statsfalse workflow.
    /// </summary>
    private void Statsfalse()
    {
        //Cursor.lockState = CursorLockMode.None;
        StatsMenu.SetActive(true);
        IsStats = true;
        StatsMenu.GetComponent<PlayerUI>().UpdatePlayerStats();
    }



    /// <summary>
    /// Executes the pause game workflow.
    /// </summary>
    public void PauseGame()
    {
        pauseSound.Play();
        Cursor.lockState = CursorLockMode.None;
        Pausemenu.SetActive(true);
        Time.timeScale = 0f;
        Pause = true;



    }

    /// <summary>
    /// Executes the resumegame workflow.
    /// </summary>
    public void Resumegame()
    {

        if (combatManager.isCombatActive == true)
        {
            resumeSound.Play();
            Cursor.lockState = CursorLockMode.None;
            Pausemenu.SetActive(false);
            Time.timeScale = 1f;
            Pause = false;

        }
        else
        {
            resumeSound.Play();
            Cursor.lockState = CursorLockMode.Locked;
            Pausemenu.SetActive(false);
            Time.timeScale = 1f;
            Pause = false;

        }


    }

}

