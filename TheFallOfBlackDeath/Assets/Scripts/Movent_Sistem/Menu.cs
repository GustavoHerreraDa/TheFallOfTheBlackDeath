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

    [Header("Main Menu Reference")]
    [SerializeField] private MainPanel mainPanelScript; // <-- NUEVA REFERENCIA

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
        // Cerrar inventario con Esc si está abierto
// Cerrar inventario con Esc si está abierto
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (Inventorymenu.activeSelf)
            {
                Inventorytrue();
                return;
            }

            // NUEVO: Si el juego está pausado y hay un subpanel abierto, volvemos atrás
            if (Pause && mainPanelScript != null && mainPanelScript.IsAnySubPanelOpen())
            {
                mainPanelScript.ReturnToMainPanel();
                return; // Cortamos la ejecución acá para que NO cierre la pausa completa
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
        // Abrir/cerrar inventario con Tab o con I
        if (Input.GetKeyDown(KeyCode.Tab) || Input.GetKeyDown(KeyCode.I))
        {
            if (Pausemenu.activeSelf) // Si el Pausemenu está activo, no abrir el Inventorymenu
            {
                // Evitar abrir inventario cuando está el menú de pausa
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

        // Nota: El Tab previamente cambiaba el StatsMenu. Se elimina para evitar conflicto con el inventario.

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

        CursorManager.Instance?.ReleaseCursor(Inventorymenu);
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
        CursorManager.Instance?.RequestCursor(Inventorymenu);
        Inventorymenu.SetActive(true);
        inventory = true;
        cameraMain.enabled = false;
        playerControl.stop = true;

        
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
        CursorManager.Instance?.RequestCursor(Pausemenu);
        Pausemenu.SetActive(true);
        Time.timeScale = 0f;
        Pause = true;



    }

    /// <summary>
    /// Executes the resumegame workflow.
    /// </summary>
    public void Resumegame()
    {
        resumeSound.Play();
        CursorManager.Instance?.ReleaseCursor(Pausemenu);
        
        // NUEVO: Nos aseguramos de limpiar cualquier panel residual por si acaso
        if (mainPanelScript != null)
        {
            mainPanelScript.CloseAllPanels();
        }

        Pausemenu.SetActive(false);
        Time.timeScale = 1f;
        Pause = false;
    }

}

