using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Supports menu and scene loading flow by handling main menu.
/// </summary>
public class MainMenu : MonoBehaviour
{
    // Start is called before the first frame update
    /// <summary>
    /// Initializes the component once the scene dependencies are ready.
    /// </summary>
    void Start()
    {
        
    }

    // Update is called once per frame
    /// <summary>
    /// Updates the component each frame while it is active.
    /// </summary>
    void Update()
    {
        
    }
    /// <summary>
    /// Executes the escena juego workflow.
    /// </summary>
    public void EscenaJuego()
    {
        SceneManager.LoadSceneAsync(1);
    }
}
