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

        // Create a copy of the available spawn points to randomly select from
        List<Transform> availablePoints = new List<Transform>(objectSpawnPoints);
        
        int count = Mathf.Min(objectSpawnPoints.Count, objectPrefabs.Count);
        for (int i = 0; i < count; i++)
        {
            int randomPointIndex = Random.Range(0, availablePoints.Count);
            Transform selectedPoint = availablePoints[randomPointIndex];
            availablePoints.RemoveAt(randomPointIndex); // Remove so it isn't picked twice
            
            if (selectedPoint != null && objectPrefabs[i] != null)
            {
                GameObject spawnedObj = Instantiate(objectPrefabs[i], selectedPoint.position, selectedPoint.rotation);
                Debug.Log($"[ObjectRoom] Spawned object '{spawnedObj.name}' at {selectedPoint.position}.");
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