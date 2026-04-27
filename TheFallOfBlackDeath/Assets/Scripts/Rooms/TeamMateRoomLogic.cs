using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Logic handler for TEAM_MATE type rooms.
/// Manages the spawning of a random teammate from a provided list upon activation.
/// </summary>
public class TeamMateRoomLogic : RoomLogicBase
{
    [SerializeField] private List<Transform> teammateSpawnPoints;
    [SerializeField] private List<GameObject> teammatePrefabs;

    /// <summary>
    /// Spawns a randomly selected teammate prefab at a randomly selected spawn point.
    /// </summary>
    public override void ExecuteLogic()
    {
        if (teammateSpawnPoints != null && teammateSpawnPoints.Count > 0 && teammatePrefabs != null && teammatePrefabs.Count > 0)
        {
            int randomIndex = Random.Range(0, teammatePrefabs.Count);
            GameObject selectedPrefab = teammatePrefabs[randomIndex];
            
            Transform selectedPoint = teammateSpawnPoints[Random.Range(0, teammateSpawnPoints.Count)];
            if (selectedPrefab != null && selectedPoint != null)
            {
                GameObject spawnedTeammate = Instantiate(selectedPrefab, selectedPoint.position, selectedPoint.rotation);
                Debug.Log($"[TeamMateRoom] Teammate '{spawnedTeammate.name}' spawned at {selectedPoint.position}.");
            }
        }
        else
        {
            Debug.LogWarning("[TeamMateRoom] Missing teammate prefabs or spawn points list is empty.");
        }
    }
}