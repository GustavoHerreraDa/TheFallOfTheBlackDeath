using UnityEngine;
using UnityEngine.SceneManagement;

public class ExitButton : MonoBehaviour
{
    public void QuitGame()
    {
        Application.Quit();
        
    }
    public void LoadMainScene()
    {
        SceneManager.LoadScene("Menu");
       
    }
}
