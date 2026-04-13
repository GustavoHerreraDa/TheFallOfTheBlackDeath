using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
using System.Collections;
using URPGlitch; 

public class Scene_Change : MonoBehaviour
{
    [SerializeField] private int fightSceneIndex;
    [SerializeField] private Volume postProcessVolume; 
    [SerializeField] private float glitchDuration = 0.5f;

    private AnalogGlitchVolume analogGlitch;
    private DigitalGlitchVolume digitalGlitch;

    private void Start()
    {
        if (postProcessVolume != null && postProcessVolume.profile != null)
        {
            postProcessVolume.profile.TryGet(out analogGlitch);
            postProcessVolume.profile.TryGet(out digitalGlitch);
        }
    }

    private void OnTriggerEnter(Collider other)
    {

        var player = other.GetComponent<PlayerControl>();
        if (player)
        {
            FollowPlayer enemyScript = GetComponent<FollowPlayer>();
            if (enemyScript != null)
            {
                enemyScript.StopEnemyForTransition();
            }
            
            StartCoroutine(DirectGlitchTransition(player.transform.position));
        }
    }

    private IEnumerator DirectGlitchTransition(Vector3 playerPos)
    {
        Time.timeScale = 0.1f; 
        Time.fixedDeltaTime = 0.02f * Time.timeScale; // Mantiene la física estable
        
        if (analogGlitch != null && digitalGlitch != null)
        {
            analogGlitch.active = true;
            digitalGlitch.active = true;


            analogGlitch.scanLineJitter.Override(0.2f);
            analogGlitch.colorDrift.Override(0.8f);
            analogGlitch.horizontalShake.Override(0.2f);
            digitalGlitch.intensity.Override(0.2f); 
        }


        GameManager.Instance.lastPos = playerPos;
        GameManager.Instance.hasValidLastPos = true;


        yield return new WaitForSecondsRealtime(glitchDuration);
        
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;

        SceneManager.LoadScene(fightSceneIndex);
        
        Cursor.lockState = CursorLockMode.None;
    }
}