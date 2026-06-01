using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Logic handler for KEY_OBJECT type rooms.
/// Handles the random spawning and simulated collection of critical progression items.
/// </summary>
public class KeyObjectRoomLogic : RoomLogicBase
{
    [SerializeField] private List<Transform> keyObjectSpawnPoints;
    [SerializeField] private GameObject keyObjectPrefab;

    /// <summary>
    /// Spawns the key item prefab at a randomly selected spawn point.
    /// </summary>
    public override void ExecuteLogic()
    {
        if (keyObjectSpawnPoints != null && keyObjectSpawnPoints.Count > 0 && keyObjectPrefab != null)
        {
            Transform selectedPoint = keyObjectSpawnPoints[Random.Range(0, keyObjectSpawnPoints.Count)];
            if (selectedPoint != null)
            {
                GameObject keyItem = Instantiate(keyObjectPrefab, selectedPoint.position, selectedPoint.rotation);
                Debug.Log($"[KeyObjectRoom] Key Object '{keyItem.name}' spawned at {selectedPoint.position}.");
            }
        }
        else
        {
            Debug.LogWarning("[KeyObjectRoom] Missing key object prefab or spawn points list is empty.");
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