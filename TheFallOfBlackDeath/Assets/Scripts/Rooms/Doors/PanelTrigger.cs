using UnityEngine;

/// Attach this to a trigger/collider.
/// Shows a shared panel and allows opening the assigned door.
public class PanelTrigger : MonoBehaviour
{
    public static PanelTrigger Current;

    [Header("References")]
    [SerializeField] private GameObject panel;
    [SerializeField] private VerticalDoorGroupController controller;
    [SerializeField] private int doorIndex;

    private bool playerInside;

    private void Awake()
    {
        if (panel != null)
            panel.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Charecter")) return;

        Current = this;
        playerInside = true;

        if (panel != null)
            panel.SetActive(true);

        ShowMouse();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Charecter")) return;

        if (Current == this)
            Current = null;

        playerInside = false;

        if (panel != null)
            panel.SetActive(false);

        HideMouse();
    }

    public void OpenDoor()
    {
        if (!playerInside) return;

        controller?.TryOpenDoor(doorIndex);

        if (panel != null)
            panel.SetActive(false);

        HideMouse();
    }

    public void DismissPanel()
    {
        if (panel != null)
            panel.SetActive(false);

        HideMouse();
    }

    private void ShowMouse()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private void HideMouse()
    {
        Debug.Log("[VERTICAL DOOR TRIGGER] CURSOR INVISIBLE");
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
}