using UnityEngine;

/// Attach this to each door trigger/collider.
/// Set index (0,1,2) matching the manager.
/// This will ONLY open a door if no other has been opened before.
public class HorizontalTriggerDoor : MonoBehaviour
{
    [SerializeField] private HorizontalSlidingDoor door;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Charecter")) return;

        door.Open();
    }
}