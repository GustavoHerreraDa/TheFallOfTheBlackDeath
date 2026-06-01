using UnityEngine;

/// <summary>
/// The primary component attached to a room prefab. 
/// Handles interaction triggers, stores the room's type, and routes execution to the specific logic layer.
/// </summary>
[RequireComponent(typeof(Collider))]
public class RoomScript : MonoBehaviour
{
    [Header("Room Configuration")]
    [SerializeField] private RoomType roomType;
    [SerializeField] private RoomLogicBase roomLogic;
    
    private bool isActivated = false;

    /// <summary>
    /// Gets the type of this room.
    /// </summary>
    public RoomType Type => roomType;

    private void Start()
    {
        if (RoomManager.Instance != null)
        {
            RoomManager.Instance.RegisterRoom(this);
        }

        Collider triggerCollider = GetComponent<Collider>();
        if (triggerCollider != null)
        {
            triggerCollider.isTrigger = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isActivated) return;

        // Assuming standard player detection via tag.
        if (other.CompareTag("Charecter"))
        {
            ActivateRoom();
        }
    }

    /// <summary>
    /// Activates the room, preventing further triggers, and executes the assigned modular logic.
    /// </summary>
    public void ActivateRoom()
    {
        isActivated = true;
        
        if (RoomManager.Instance != null)
        {
            RoomManager.Instance.NotifyRoomActivated(this);
        }

        if (roomLogic != null)
        {
            roomLogic.ExecuteLogic();
        }
        else
        {
            Debug.LogError($"[RoomScript] No RoomLogicBase assigned on {gameObject.name} (Type: {roomType})");
        }
    }

    private void OnDestroy()
    {
        if (RoomManager.Instance != null)
        {
            RoomManager.Instance.UnregisterRoom(this);
        }
    }
}
