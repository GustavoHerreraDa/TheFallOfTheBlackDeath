using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Supports menu and scene loading flow by handling scene load manager.
/// </summary>
public class SceneLoadManager : MonoBehaviour
{
    [SerializeField] private Slider loadbar;
    public GameObject loadPanel;
    [SerializeField] private GameObject introCanvas;

    /// <summary>
    /// Initializes the component once the scene dependencies are ready.
    /// </summary>
    public void Start()
    {
        SceneLoad(1);
    }
    /// <summary>
    /// Executes the scene load workflow.
    /// </summary>
    /// <param name="sceneIndex">The scene index.</param>
    public void SceneLoad(int sceneIndex)
    {
        StartCoroutine(ActivateLoadPanel());
        StartCoroutine(LoadAsync(sceneIndex));
    }
    /// <summary>
    /// Executes the activate load panel workflow.
    /// </summary>
    /// <returns>An enumerator that drives the coroutine sequence.</returns>
    IEnumerator ActivateLoadPanel()
    {
        if (introCanvas != null) { 
            introCanvas.SetActive(false);
            Debug.Log("SE DESACTIVO EL CANVAS INTRO");
        }

        loadbar.value = 0;
        loadPanel.SetActive(true);

        yield return 0.5f;
    }

    /// <summary>
    /// Loads the async.
    /// </summary>
    /// <param name="sceneIndex">The scene index.</param>
    /// <returns>An enumerator that drives the coroutine sequence.</returns>
    IEnumerator LoadAsync(int sceneIndex)
    {
        PlayerPrefs.SetString("GrupoEnemigo", string.Empty);

        loadbar.value = 0;
        loadPanel.SetActive(true);

        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneIndex);
        asyncOperation.allowSceneActivation = false;
        float progress = 0;

        while (!asyncOperation.isDone)
        {
            progress = Mathf.MoveTowards(progress, asyncOperation.progress, Time.deltaTime);
            loadbar.value = progress;
            if (progress >= 0.9f)
            {
                loadbar.value = 1;
                asyncOperation.allowSceneActivation = true;
            }
            yield return null;
        }

        //AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneIndex);

        //while(!asyncOperation.isDone)
        //{
        //    Debug.Log(asyncOperation.progress);
        //    loadbar.value = asyncOperation.progress / 0.9f;
        //    yield return null;
        //}
    }
}
