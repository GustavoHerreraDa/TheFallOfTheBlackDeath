using UnityEngine;
using InventoryNew;

/// Attach this to a trigger/collider.
/// Shows a shared panel and allows opening the assigned door.
public class PanelTriggerHorizontal : MonoBehaviour
{
    public static PanelTriggerHorizontal Current;

    [Header("References")]
    [SerializeField] private GameObject panel;
    [SerializeField] private HorizontalDoorGroupController controller;
    [SerializeField] private int doorIndex;

    [Header("Requirements Settings")]
    [SerializeField] private bool requiresItem = false;
    [SerializeField] private string requiredItemId = "Key_ID";
    [SerializeField] private bool consumeItemOnUse = false;

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
        bool canopenDoor = controller.CanOpenDoor(doorIndex);
        if (canopenDoor == false) return;
        if (panel != null)
            panel.SetActive(true);

        ShowMouse();
    }
    public void OpenCurrentDoor()
    {
        PanelTrigger.Current?.OpenDoor();
    }

    public void DismissCurrentPanel()
    {
        PanelTrigger.Current?.DismissPanel();
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

        // Validación de requisitos de inventario
        if (requiresItem)
        {
            if (NewInventoryManager.Instance != null)
            {
                if (!NewInventoryManager.Instance.HasItem(requiredItemId))
                {
                    // FEEDBACK: El jugador no tiene el ítem
                    Debug.Log($"<color=orange>[Door System]</color> Cerrado. Necesitas: {requiredItemId}");
                    
                    // Aquí se puede disparar un evento de UI para mostrar un mensaje en pantalla
                    // ej: UIManager.Instance.ShowNotification("Necesitas la " + requiredItemId);
                    
                    return;
                }

                // Si se llega aquí, tiene el ítem. ¿Se consume?
                if (consumeItemOnUse)
                {
                    NewInventoryManager.Instance.RemoveItem(requiredItemId, 1);
                    Debug.Log($"<color=green>[Door System]</color> Ítem '{requiredItemId}' consumido al abrir la puerta.");
                }
            }
            else
            {
                Debug.LogWarning("[Door System] NewInventoryManager no encontrado. Permitiendo acceso por defecto en modo debug.");
            }
        }

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