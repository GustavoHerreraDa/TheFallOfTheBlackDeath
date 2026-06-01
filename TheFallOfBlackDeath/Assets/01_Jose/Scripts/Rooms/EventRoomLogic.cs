using UnityEngine;

/// <summary>
/// Logic handler for EVENT type rooms.
/// Triggers predefined events and interacts with the CharacterEventManager.
/// </summary>
public class EventRoomLogic : RoomLogicBase
{
    [SerializeField] private Transform eventTriggerPoint;
    [SerializeField] private EventScript roomEvent;

    /// <summary>
    /// Triggers the assigned event and displays a simulated UI modal debug message.
    /// </summary>
    public override void ExecuteLogic()
    {
        if (roomEvent != null)
        {
            Debug.Log($"[EventRoom] Triggering event '{roomEvent.EventName}' at {eventTriggerPoint.position}.");
            Debug.Log($"[EventRoom UI MODAL] {roomEvent.EventName}: {roomEvent.Description}");

            if (CharacterEventManager.Instance != null)
            {
                CharacterEventManager.Instance.ApplyEvent(roomEvent);
            }
        }
        else
        {
            Debug.LogWarning("[EventRoom] Missing EventScript reference.");
        }
    }
}