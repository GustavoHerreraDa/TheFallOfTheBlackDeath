using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//TP2 FACUNDO FERREIRO/GUSTAVO TORRES
/// <summary>
/// Supports exploration and world-state flow by handling enemies group.
/// </summary>
public class EnemiesGroup : MonoBehaviour
{
    public string GroupName;
    public bool IsStunned { get; private set; }

    private Coroutine stunRoutine;

    /// <summary>
    /// Temporarily disables this group as an encounter source after the player escapes.
    /// </summary>
    /// <param name="duration">The stun duration in seconds.</param>
    public void StunForSeconds(float duration)
    {
        if (duration <= 0f)
            return;

        if (stunRoutine != null)
            StopCoroutine(stunRoutine);

        stunRoutine = StartCoroutine(StunRoutine(duration));
    }

    private IEnumerator StunRoutine(float duration)
    {
        IsStunned = true;

        Scene_Change[] sceneChanges = GetComponentsInChildren<Scene_Change>(true);
        bool[] sceneChangeStates = new bool[sceneChanges.Length];
        for (int i = 0; i < sceneChanges.Length; i++)
        {
            var sceneChange = sceneChanges[i];
            if (sceneChange != null)
            {
                sceneChangeStates[i] = sceneChange.enabled;
                sceneChange.enabled = false;
            }
        }

        FollowPlayer[] followers = GetComponentsInChildren<FollowPlayer>(true);
        foreach (var follower in followers)
        {
            if (follower != null)
                follower.StunForSeconds(duration);
        }

        yield return new WaitForSeconds(duration);

        for (int i = 0; i < sceneChanges.Length; i++)
        {
            var sceneChange = sceneChanges[i];
            if (sceneChange != null)
                sceneChange.enabled = sceneChangeStates[i];
        }

        IsStunned = false;
        stunRoutine = null;
    }
}
