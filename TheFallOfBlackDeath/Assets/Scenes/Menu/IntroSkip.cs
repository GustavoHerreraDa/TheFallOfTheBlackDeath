using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Supports menu and scene loading flow by handling intro skip.
/// </summary>
public class IntroSkip : MonoBehaviour
{
    public string sceneToLoad;
    public GameObject introPanel;


    /// <summary>
    /// Updates the component each frame while it is active.
    /// </summary>
    private void Update()
    {
        if (introPanel.activeSelf && Input.anyKeyDown)
        {
            SkipIntro();
        }
    }

    /// <summary>
    /// Executes the skip intro workflow.
    /// </summary>
    public void SkipIntro()
    {
        
        SceneManager.LoadScene(1);
    }
}
