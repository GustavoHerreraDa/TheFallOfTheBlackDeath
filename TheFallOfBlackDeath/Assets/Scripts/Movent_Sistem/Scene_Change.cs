using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Scene_Change : MonoBehaviour
{
   
    [SerializeField] private int figthScene;

    /*[SerializeField] private Collider Box;
    [SerializeField] private GameObject player;
    */



    public void OnTriggerEnter(Collider Box)
    {

        GameManager.Instance.lastPos = GameManager.Instance.character.transform.position;
        /*PlayerPrefs.SetFloat("PosX", player.transform.position.x);
        PlayerPrefs.SetFloat("PosY", player.transform.position.y);
        PlayerPrefs.SetFloat("PosZ", player.transform.position.z);
        */
        
        
        SceneManager.LoadScene(figthScene);


        Cursor.lockState = CursorLockMode.None;
      


    }

}
