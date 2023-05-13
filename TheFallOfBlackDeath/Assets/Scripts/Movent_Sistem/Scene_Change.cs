using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Scene_Change : MonoBehaviour
{

    [SerializeField] private Collider Box;
    [SerializeField] private GameObject player;
    
   

    public void OnTriggerEnter(Collider Box)
    {
        PlayerPrefs.SetFloat("PosX", player.transform.position.x);
        PlayerPrefs.SetFloat("PosY", player.transform.position.y);
        PlayerPrefs.SetFloat("PosZ", player.transform.position.z);

        SceneManager.LoadSceneAsync(2);
        
        Cursor.lockState = CursorLockMode.None;

        float A;

        A = PlayerPrefs.GetFloat("PosX");

        Debug.Log(A);
    }
}
