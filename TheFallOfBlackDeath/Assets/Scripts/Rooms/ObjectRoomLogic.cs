using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Logic handler for OBJECT type rooms.
/// Spawns multiple interactable objects and handles simulated collection logging.
/// </summary>
public class ObjectRoomLogic : RoomLogicBase
{
    [SerializeField] private List<Transform> objectSpawnPoints;
    [SerializeField] private List<GameObject> objectPrefabs;

    /// <summary>
    /// Iterates through spawn points and instantiates the object prefabs.
    /// </summary>
    public override void ExecuteLogic()
    {
        if (objectSpawnPoints == null || objectPrefabs == null) return;

        int count = Mathf.Min(objectSpawnPoints.Count, objectPrefabs.Count);
        for (int i = 0; i < count; i++)
        {
            if (objectSpawnPoints[i] != null && objectPrefabs[i] != null)
            {
                GameObject spawnedObj = Instantiate(objectPrefabs[i], objectSpawnPoints[i].position, objectSpawnPoints[i].rotation);
                Debug.Log($"[ObjectRoom] Spawned object '{spawnedObj.name}' at {objectSpawnPoints[i].position}.");
            }
        }
    }

    /// <summary>
    /// Logs a debug message simulating the collection of an object.
    /// Can be wired to the interactable objects later.
    /// </summary>
    /// <param name="objectName">The name of the collected object.</param>
    public void OnObjectCollected(string objectName)
    {
        Debug.Log($"[ObjectRoom] Object collected: {objectName}");
    }
}