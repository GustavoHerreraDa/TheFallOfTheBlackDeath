using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

/// <summary>
/// Maneja exclusivamente el menú de pausa dentro del nivel/mundo,
/// evitando superposición de paneles y gestionando el audio y la salida.
/// </summary>
public class InGamePauseMenu : MonoBehaviour
{
    [Header("UI Roots & Panels")]
    [Tooltip("El Canvas o panel padre que contiene todo el menú de pausa")]
    public GameObject pauseMenuRoot; 
    public GameObject mainPausePanel;
    public GameObject optionsPanel;
    public GameObject exitConfirmPanel;

    [Header("Opciones de Audio")]
    public AudioMixer mixer;
    public Slider volumenMasterSlider;
    public Slider volumenFXSlider;
    
    [Header("Feedback de Audio")]
    public AudioSource uiAudioSource;
    public AudioClip clickSound;
    public AudioClip pauseSound;
    public AudioClip resumeSound;

    // Estado interno para saber si el juego está pausado por este script
    private bool isPaused = false;

    private void Awake()
    {
        // Configuramos los listeners de los sliders por código
        if (volumenMasterSlider != null)
            volumenMasterSlider.onValueChanged.AddListener(ChangeVolumenMaster);
            
        if (volumenFXSlider != null)
            volumenFXSlider.onValueChanged.AddListener(ChangeVolumenMasterFX);

        // Nos aseguramos de que el menú arranque apagado
        pauseMenuRoot.SetActive(false);
    }

    private void Update()
    {
        // Lógica del botón ESC
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            HandleEscapeKey();
        }
    }

    /// <summary>
    /// Gestiona la jerarquía de paneles para que no se superpongan y el ESC funcione lógicamente.
    /// </summary>
    private void HandleEscapeKey()
    {
        // Si el juego NO está pausado, lo pausamos y abrimos el panel principal
        if (!isPaused)
        {
            PauseGame();
            return;
        }

        // Si ya estamos pausados, verificamos qué panel está abierto para saber qué cerrar
        if (optionsPanel.activeSelf)
        {
            CloseSubPanel(optionsPanel);
        }
        else if (exitConfirmPanel.activeSelf)
        {
            CloseSubPanel(exitConfirmPanel);
        }
        else if (mainPausePanel.activeSelf)
        {
            // Si solo está abierto el panel principal, despausamos el juego
            ResumeGame();
        }
    }

    // --- CONTROL DE FLUJO DEL JUEGO ---

    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;
        
        // Activamos la UI y el panel principal exclusivamente
        pauseMenuRoot.SetActive(true);
        mainPausePanel.SetActive(true);
        optionsPanel.SetActive(false);
        exitConfirmPanel.SetActive(false);

        PlaySound(pauseSound);
        
        // Solicitamos el cursor usando tu CursorManager
        CursorManager.Instance?.RequestCursor(pauseMenuRoot);
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
        
        pauseMenuRoot.SetActive(false);

        PlaySound(resumeSound);

        // Liberamos el cursor
        CursorManager.Instance?.ReleaseCursor(pauseMenuRoot);
    }

    // --- NAVEGACIÓN DE PANELES (BOTONES) ---

    public void OpenOptionsPanel()
    {
        PlaySound(clickSound);
        mainPausePanel.SetActive(false);
        optionsPanel.SetActive(true);
    }

    public void OpenExitConfirmPanel()
    {
        PlaySound(clickSound);
        mainPausePanel.SetActive(false);
        exitConfirmPanel.SetActive(true);
    }

    /// <summary>
    /// Vuelve al panel principal de pausa desde cualquier sub-panel.
    /// </summary>
    public void CloseSubPanel(GameObject panelToClose)
    {
        PlaySound(clickSound);
        panelToClose.SetActive(false);
        mainPausePanel.SetActive(true);
    }

    // --- ACCIONES FINALES ---

    public void ConfirmExitToMainMenu()
    {
        PlaySound(clickSound);
        Time.timeScale = 1f; // Importantísimo devolver el tiempo a la normalidad antes de cambiar de escena
        SceneManager.LoadScene(0); // Cambia al índice de tu Menú Principal
    }

    public void ConfirmExitToDesktop()
    {
        PlaySound(clickSound);
        Application.Quit();
    }

    // --- AUDIO ---

    public void ChangeVolumenMaster(float v)
    {
        if (mixer != null)
            mixer.SetFloat("VolMaster", v);
    }

    public void ChangeVolumenMasterFX(float v)
    {
        if (mixer != null)
            mixer.SetFloat("VolFX", v);
    }

    private void PlaySound(AudioClip clip)
    {
        if (uiAudioSource != null && clip != null)
        {
            uiAudioSource.PlayOneShot(clip);
        }
    }
}