using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneLoadManager : MonoBehaviour
{
    [SerializeField] private Slider loadbar;
    [SerializeField] private GameObject loadPanel;
    [SerializeField] private GameObject introCanvas;

    public void Start()
    {


        SceneLoad(1);
    }
    public void SceneLoad(int sceneIndex)
    {
        StartCoroutine(ActivateLoadPanel());
        StartCoroutine(LoadAsync(sceneIndex));
    }
    IEnumerator ActivateLoadPanel()
    {
        PlayerPrefs.SetString("GrupoEnemigo", string.Empty);
        yield return 0.1f;

        if (introCanvas != null)
            introCanvas.SetActive(false);

        loadbar.value = 0;
        loadPanel.SetActive(true);
        yield return 0.1f;
    }

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
