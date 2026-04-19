using UnityEngine;

/// <summary>
/// Logic handler for COMBAT type rooms.
/// Manages enemy spawn points and handles the spawning of enemies upon activation.
/// </summary>
public class CombatRoomLogic : RoomLogicBase
{
    [SerializeField] private Transform enemySpawnPoint;
    [SerializeField] private GameObject enemyPrefab;

    /// <summary>
    /// Spawns the assigned enemy prefab at the designated spawn point.
    /// </summary>
    public override void ExecuteLogic()
    {
        if (enemySpawnPoint != null && enemyPrefab != null)
        {
            GameObject spawnedEnemy = Instantiate(enemyPrefab, enemySpawnPoint.position, enemySpawnPoint.rotation);
            Debug.Log($"[CombatRoom] Enemy '{spawnedEnemy.name}' spawned at {enemySpawnPoint.position}.");
        }
        else
        {
            Debug.LogWarning("[CombatRoom] Missing enemy prefab or spawn point reference.");
        }
    }
}