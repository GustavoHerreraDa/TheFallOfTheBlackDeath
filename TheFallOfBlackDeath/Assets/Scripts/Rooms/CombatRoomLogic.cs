using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Logic handler for COMBAT type rooms.
/// Manages multiple enemy spawn points and randomly selects one for spawning enemies upon activation.
/// </summary>
public class CombatRoomLogic : RoomLogicBase
{
    [SerializeField] private List<Transform> enemySpawnPoints;
    [SerializeField] private GameObject enemyPrefab;

    /// <summary>
    /// Spawns the assigned enemy prefab at a randomly selected spawn point.
    /// </summary>
    public override void ExecuteLogic()
    {
        if (enemySpawnPoints != null && enemySpawnPoints.Count > 0 && enemyPrefab != null)
        {
            Transform selectedPoint = enemySpawnPoints[Random.Range(0, enemySpawnPoints.Count)];
            if (selectedPoint != null)
            {
                GameObject spawnedEnemy = Instantiate(enemyPrefab, selectedPoint.position, selectedPoint.rotation);
                Debug.Log($"[CombatRoom] Enemy '{spawnedEnemy.name}' spawned at {selectedPoint.position}.");
            }
        }
        else
        {
            Debug.LogWarning("[CombatRoom] Missing enemy prefab or spawn points list is empty.");
        }
    }
}