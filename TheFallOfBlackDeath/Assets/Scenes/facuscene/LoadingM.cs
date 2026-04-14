using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Handles loading m for the current project workflow.
/// </summary>
public class LoadingM : MonoBehaviour
{
    /// <summary>
    /// Initializes the component once the scene dependencies are ready.
    /// </summary>
    void Start()
    {
        int sceneToLoad = PlayerPrefs.GetInt("NextScene");
        StartCoroutine(LoadSceneAsync(sceneToLoad));
    }

    /// <summary>
    /// Loads the scene async.
    /// </summary>
    /// <param name="sceneIndex">The scene index.</param>
    /// <returns>An enumerator that drives the coroutine sequence.</returns>
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
