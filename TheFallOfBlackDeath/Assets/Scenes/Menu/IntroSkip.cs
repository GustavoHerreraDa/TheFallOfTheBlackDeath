using UnityEngine;
using UnityEngine.SceneManagement;

public class IntroSkip : MonoBehaviour
{
    public string sceneToLoad;
    public GameObject introPanel;


    private void Update()
    {
        if (introPanel.activeSelf && Input.anyKeyDown)
        {
            SkipIntro();
        }
    }

    public void SkipIntro()
    {
        
        SceneManager.LoadScene(14);
    }
}