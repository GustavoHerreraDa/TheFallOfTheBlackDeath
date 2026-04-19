using UnityEngine;

/// <summary>
/// Logic handler for KEY_OBJECT type rooms.
/// Handles the spawning and simulated collection of critical progression items.
/// </summary>
public class KeyObjectRoomLogic : RoomLogicBase
{
    [SerializeField] private Transform keyObjectSpawnPoint;
    [SerializeField] private GameObject keyObjectPrefab;

    /// <summary>
    /// Spawns the key item prefab at the designated key spawn point.
    /// </summary>
    public override void ExecuteLogic()
    {
        if (keyObjectSpawnPoint != null && keyObjectPrefab != null)
        {
            GameObject keyItem = Instantiate(keyObjectPrefab, keyObjectSpawnPoint.position, keyObjectSpawnPoint.rotation);
            Debug.Log($"[KeyObjectRoom] Key Object '{keyItem.name}' spawned at {keyObjectSpawnPoint.position}.");
        }
        else
        {
            Debug.LogWarning("[KeyObjectRoom] Missing key object prefab or spawn point reference.");
        }
    }

    /// <summary>
    /// Logs a debug message simulating the collection of the key object.
    /// </summary>
    /// <param name="keyName">The name of the collected key item.</param>
    public void OnKeyCollected(string keyName)
    {
        Debug.Log($"[KeyObjectRoom] Critical Key Object collected: {keyName}. Progression updated.");
    }
}