using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Logic handler for TEAM_MATE type rooms.
/// Teleports an existing teammate already present in the scene
/// to a random spawn point when activated.
/// </summary>
public class TeamMateRoomLogic : RoomLogicBase
{
    [SerializeField] private List<Transform> teammateSpawnPoints;
    [SerializeField] private NavMeshAgent teammateAgent;

    public override void ExecuteLogic()
    {
        if (teammateAgent == null)
        {
            Debug.LogWarning("[TeamMateRoom] No teammate agent assigned.");
            return;
        }

        if (teammateSpawnPoints == null || teammateSpawnPoints.Count == 0)
        {
            Debug.LogWarning("[TeamMateRoom] Spawn points list is empty.");
            return;
        }

        Transform selectedPoint = teammateSpawnPoints[
            Random.Range(0, teammateSpawnPoints.Count)
        ];

        if (selectedPoint == null)
        {
            Debug.LogWarning("[TeamMateRoom] Selected spawn point is null.");
            return;
        }

        teammateAgent.ResetPath();

        bool teleported = teammateAgent.Warp(selectedPoint.position);

        if (!teleported)
        {
            Debug.LogWarning(
                $"[TeamMateRoom] Failed to warp '{teammateAgent.name}'. Is the target position on the NavMesh?"
            );
            return;
        }

        teammateAgent.transform.rotation = selectedPoint.rotation;

        Debug.Log(
            $"[TeamMateRoom] Teammate '{teammateAgent.name}' teleported to {selectedPoint.position}."
        );
    }
}