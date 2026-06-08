using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
using System.Collections;
using URPGlitch; 

/// <summary>
/// Supports exploration and world-state flow by handling scene change.
/// </summary>
public class Scene_Change : MonoBehaviour
{
    [SerializeField] private int fightSceneIndex;
    [SerializeField] private Volume postProcessVolume; 
    [SerializeField] private float glitchDuration = 0.2f;

    private AnalogGlitchVolume analogGlitch;
    private DigitalGlitchVolume digitalGlitch;

    /// <summary>
    /// Initializes the component once the scene dependencies are ready.
    /// </summary>
    private void Start()
    {
        // 1. Intentar encontrar automáticamente cualquier Volume en la escena
        if (postProcessVolume == null)
        {
            postProcessVolume = FindObjectOfType<Volume>();
        }

        // 2. Extraer los perfiles de glitch si el Volume existe
        if (postProcessVolume != null && postProcessVolume.profile != null)
        {
            postProcessVolume.profile.TryGet(out analogGlitch);
            postProcessVolume.profile.TryGet(out digitalGlitch);
        }
        else
        {
            Debug.LogWarning("Scene_Change: No se encontró un Global Volume en la escena.");
        }
    }

    /// <summary>
    /// Responds to the corresponding Unity trigger callback for this component.
    /// </summary>
    /// <param name="other">The other.</param>
    private void OnTriggerEnter(Collider other)
    {
        EnemiesGroup enemiesGroup = GetEncounterGroup();
        if (enemiesGroup != null && enemiesGroup.IsStunned)
            return;

        var player = other.GetComponent<PlayerControl>();
        if (player)
        {
            if (GameManager.Instance != null)
            {
                // Captura instantánea en el frame exacto del trigger (líder + party)
                GameManager.Instance.SaveCurrentPosition(player.transform.position);
            }

            FollowPlayer enemyScript = GetComponent<FollowPlayer>();
            if (enemyScript != null)
            {
                enemyScript.StopEnemyForTransition();
            }
            
            StartCoroutine(DirectGlitchTransition());
        }
    }

    /// <summary>
    /// Executes the direct glitch transition workflow.
    /// </summary>
    /// <returns>An enumerator that drives the coroutine sequence.</returns>
private IEnumerator DirectGlitchTransition()
{
    Debug.Log("[DirectGlitchTransition] Started");

    Time.timeScale = 0.1f;
    Time.fixedDeltaTime = 0.02f * Time.timeScale; // Mantiene la física estable

    if (analogGlitch != null && digitalGlitch != null)
    {
        Debug.Log("[DirectGlitchTransition] Glitch effects found, applying settings");

        analogGlitch.active = true;
        digitalGlitch.active = true;

        analogGlitch.scanLineJitter.Override(0.2f);
        analogGlitch.colorDrift.Override(0.4f);
        analogGlitch.horizontalShake.Override(0.2f);
        digitalGlitch.intensity.Override(0.2f);
    }

    EnemiesGroup enemiesGroup = GetEncounterGroup();
    if (enemiesGroup != null)
    {
        GameManager.Instance.RegisterCurrentEncounterGroup(enemiesGroup.GroupName);
    }

    Debug.Log("[DirectGlitchTransition] Waiting before scene transition");

    yield return new WaitForSecondsRealtime(glitchDuration);

    Time.timeScale = 1f;
    Time.fixedDeltaTime = 0.02f;

    SceneManager.LoadScene(fightSceneIndex);
    
    Cursor.visible = true;
    Cursor.lockState = CursorLockMode.None;
    Debug.Log("[DirectGlitchTransition] Cursor unlocked");
}

private EnemiesGroup GetEncounterGroup()
{
    EnemiesGroup enemiesGroup = GetComponentInParent<EnemiesGroup>();
    if (enemiesGroup == null)
        enemiesGroup = GetComponentInChildren<EnemiesGroup>();

    return enemiesGroup;
}
}
