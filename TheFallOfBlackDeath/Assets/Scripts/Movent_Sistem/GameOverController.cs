
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Supports exploration and world-state flow by handling game over controller.
/// </summary>
public class GameOverController : MonoBehaviour
{
    /// <summary>
    /// Executes the return to main menu workflow.
    /// </summary>
    public void ReturnToMainMenu()
    {
        Debug.Log("Bot�n clickeado, intentando cargar la escena");
        PrepareGameOverExit();
        SceneManager.LoadScene(0);
    }
    /// <summary>
    /// Updates the component each frame while it is active.
    /// </summary>
    private void Update()
    {
        if (Input.anyKeyDown)
        {
            gobackmenu();
        }
    }

    /// <summary>
    /// Executes the gobackmenu workflow.
    /// </summary>
    public void gobackmenu()
    {

        PrepareGameOverExit();
        SceneManager.LoadScene(0);
    }

    private void PrepareGameOverExit()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.PrepareForGameOverReturnToMenu();
        }
    }
}
