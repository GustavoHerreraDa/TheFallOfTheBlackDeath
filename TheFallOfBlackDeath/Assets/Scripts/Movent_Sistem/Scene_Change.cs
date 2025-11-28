using UnityEngine;
using UnityEngine.SceneManagement;

public class Scene_Change : MonoBehaviour
{
    [SerializeField] private int fightSceneIndex;

    private void OnTriggerEnter(Collider other)
    {
        var player = other.GetComponent<PlayerControl>();
        if (player)
        {
            Debug.Log("Saving lastPos: " + GameManager.Instance.character.transform.position);
            GameManager.Instance.lastPos = GameManager.Instance.character.transform.position;

            
            PlayerPrefs.SetInt("NextScene", fightSceneIndex);

           
            SceneManager.LoadScene("LoadingScene");

            Cursor.lockState = CursorLockMode.None;
        }
    }
}
