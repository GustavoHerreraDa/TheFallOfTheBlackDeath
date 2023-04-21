using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Scene_Change : MonoBehaviour
{

    [SerializeField] private Collider Box;

    

    private void OnTriggerEnter(Collider Box)
    {
        SceneManager.LoadSceneAsync(1);
        Debug.Log("1");
    }
}
