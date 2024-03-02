
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverController : MonoBehaviour
{
    public void ReturnToMainMenu()
    {
        Debug.Log("Botón clickeado, intentando cargar la escena");
        SceneManager.LoadScene(0);
    }
    private void Update()
    {
        if (Input.anyKeyDown)
        {
            gobackmenu();
        }
    }

    public void gobackmenu()
    {

        SceneManager.LoadScene(0);
    }

}