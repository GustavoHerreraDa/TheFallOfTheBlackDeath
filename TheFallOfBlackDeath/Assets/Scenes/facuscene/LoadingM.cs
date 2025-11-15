using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class LoadingM : MonoBehaviour
{
    void Start()
    {
        int sceneToLoad = PlayerPrefs.GetInt("NextScene");
        StartCoroutine(LoadSceneAsync(sceneToLoad));
    }

    IEnumerator LoadSceneAsync(int sceneIndex)
    {
        yield return new WaitForSeconds(1f);
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneIndex);
        operation.allowSceneActivation = false;

        while (!operation.isDone)
        {
            
            if (operation.progress >= 0.9f)
            {
                operation.allowSceneActivation = true;
            }
            yield return null;
        }
    }
}