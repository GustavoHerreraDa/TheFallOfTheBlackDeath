using UnityEngine;

/// Attach this to each door trigger/collider.
/// Set index (0,1,2) matching the manager.
/// This will ONLY open a door if no other has been opened before.
public class VerticalDoorTriggerSpecial : MonoBehaviour
{
    [SerializeField] private VerticalDoorGroupController controller;
    [SerializeField] private int doorIndex;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Charecter")) return;

        controller.TryOpenDoor(doorIndex);
    }
}