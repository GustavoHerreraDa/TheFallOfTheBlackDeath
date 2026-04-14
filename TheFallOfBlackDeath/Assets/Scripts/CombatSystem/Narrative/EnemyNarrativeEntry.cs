using UnityEngine;

[CreateAssetMenu(fileName = "EnemyNarrativeEntry", menuName = "Narrative/Enemy Narrative Entry", order = 0)]
/// <summary>
/// Stores the reusable narrative lines associated with a specific enemy identity.
/// </summary>
public class EnemyNarrativeEntry : ScriptableObject
{
    [Header("Identity")]
    [Tooltip("Stable ID that matches EnemyFighter.idName or another unique identifier you choose.")]
    public string enemyId;

    [Header("Base Narrative Lines")]
    [TextArea] public string[] encounterMessages;
    [TextArea] public string[] turnMessages;
    [TextArea] public string[] preparingMessages;
    [TextArea] public string[] attackMessages;

    [Header("Contextual Reactions To Player State")] 
    [Tooltip("Used when the player is bleeding.")]
    [TextArea] public string[] playerBleedingMessages;
    [Tooltip("Used when the player's health ratio <= low health threshold set in NarrativeLogManager.")]
    [TextArea] public string[] playerLowHealthMessages;
    [Tooltip("Used when one of the player's arms is destroyed or severely injured.")]
    [TextArea] public string[] playerInjuredArmMessages;

    /// <summary>
    /// Gets the random.
    /// </summary>
    /// <param name="pool">The pool.</param>
    /// <returns>The resulting value.</returns>
    public string GetRandom(string[] pool)
    {
        if (pool == null || pool.Length == 0) return null;
        int idx = Random.Range(0, pool.Length);
        return pool[idx];
    }
}
