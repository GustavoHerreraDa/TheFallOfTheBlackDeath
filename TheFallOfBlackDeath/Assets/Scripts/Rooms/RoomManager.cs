using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Centralized manager for tracking and accessing all instantiated rooms in the scene.
/// Implements the Singleton pattern.
/// </summary>
public class RoomManager : MonoBehaviour
{
    /// <summary>
    /// Singleton instance of the RoomManager.
    /// </summary>
    public static RoomManager Instance { get; private set; }

    /// <summary>
    /// Event triggered when any room is activated by the player.
    /// </summary>
    public event Action<RoomScript> OnRoomActivated;

    [SerializeField] private List<RoomScript> activeRooms = new List<RoomScript>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    /// <summary>
    /// Registers a room with the manager to keep track of it.
    /// </summary>
    /// <param name="room">The room to register.</param>
    public void RegisterRoom(RoomScript room)
    {
        if (room != null && !activeRooms.Contains(room))
        {
            activeRooms.Add(room);
        }
    }

    /// <summary>
    /// Unregisters a room from the manager.
    /// </summary>
    /// <param name="room">The room to unregister.</param>
    public void UnregisterRoom(RoomScript room)
    {
        if (room != null && activeRooms.Contains(room))
        {
            activeRooms.Remove(room);
        }
    }

    /// <summary>
    /// Retrieves a list of all currently active rooms.
    /// </summary>
    /// <returns>A read-only collection of active rooms.</returns>
    public IReadOnlyList<RoomScript> GetAllRooms()
    {
        return activeRooms.AsReadOnly();
    }

    /// <summary>
    /// Notifies the system that a room has been activated, firing the associated event.
    /// </summary>
    /// <param name="room">The room that was triggered.</param>
    public void NotifyRoomActivated(RoomScript room)
    {
        OnRoomActivated?.Invoke(room);
        Debug.Log($"[RoomManager] Room activated: {room.gameObject.name}");
    }
}