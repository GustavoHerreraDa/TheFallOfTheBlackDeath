using UnityEngine;
using UnityEngine.UI; // Necesario para controlar el componente Button
using TMPro; 
using InventoryNew;
using System; // Necesario para el manejo de excepciones (try-catch)

/// Attach this to a trigger/collider.
/// Shows a shared panel and allows opening the assigned door.
public class PanelTriggerHorizontal : MonoBehaviour
{
    public static PanelTriggerHorizontal Current;

    [Header("References")]
    [SerializeField] private GameObject panel;
    [SerializeField] private HorizontalDoorGroupController controller;
    [SerializeField] private int doorIndex;
    
    [SerializeField] private TMP_Text doorStatusText; 
    
    // NUEVO: Referencia al botón para desactivarlo si no tienes la llave
    [SerializeField] private Button openDoorButton; 

    [Header("Requirements Settings")]
    [SerializeField] private bool requiresItem = false;
    [SerializeField] private string requiredItemId = "Key_ID";
    [SerializeField] private bool consumeItemOnUse = false;

    [Header("UI Messages")]
    [SerializeField] private string messageHasKey = "¿Estás seguro de usar la llave?";
    [SerializeField] private string messageNeedsKey = "Necesitas una llave para abrir esta puerta.";
    [SerializeField] private string messageNoKeyNeeded = "¿Abrir puerta?";

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

        // Actualizamos el texto y el estado del botón ANTES de mostrar el panel
        UpdatePanelText();

        if (panel != null)
            panel.SetActive(true);

        ShowMouse();
    }

    private void UpdatePanelText()
    {
        if (doorStatusText == null) return;

        // Si no requiere ítem, habilitamos el botón y cambiamos texto
        if (!requiresItem)
        {
            doorStatusText.text = messageNoKeyNeeded;
            SetButtonInteractable(true);
            return;
        }

        if (NewInventoryManager.Instance != null)
        {
            bool hasKey = false;
            
            // Usamos un bloque try-catch por si el inventario da un error al buscar un ítem que ya se gastó
            try 
            {
                hasKey = NewInventoryManager.Instance.HasItem(requiredItemId);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Door System] Excepción capturada al verificar inventario: {e.Message}");
                hasKey = false; // Asumimos que no la tiene si hay un error
            }

            if (hasKey)
            {
                doorStatusText.text = messageHasKey;
                SetButtonInteractable(true); // Permite hacer clic
            }
            else
            {
                doorStatusText.text = messageNeedsKey;
                SetButtonInteractable(false); // Bloquea el botón visual y funcionalmente
            }
        }
        else
        {
            doorStatusText.text = "Error: Inventario no encontrado.";
            SetButtonInteractable(false);
        }
    }

    // Helper para habilitar/deshabilitar el botón de forma segura
    private void SetButtonInteractable(bool state)
    {
        if (openDoorButton != null)
        {
            openDoorButton.interactable = state;
        }
    }

    public void OpenCurrentDoor()
    {
        PanelTriggerHorizontal.Current?.OpenDoor();
    }

    public void DismissCurrentPanel()
    {
        PanelTriggerHorizontal.Current?.DismissPanel();
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

        if (requiresItem)
        {
            if (NewInventoryManager.Instance != null)
            {
                // Doble validación de seguridad
                bool hasKey = false;
                try { hasKey = NewInventoryManager.Instance.HasItem(requiredItemId); } catch { }

                if (!hasKey)
                {
                    Debug.Log($"<color=orange>[Door System]</color> Cerrado. Necesitas: {requiredItemId}");
                    return;
                }

                if (consumeItemOnUse)
                {
                    try
                    {
                        NewInventoryManager.Instance.RemoveItem(requiredItemId, 1);
                        Debug.Log($"<color=green>[Door System]</color> Ítem '{requiredItemId}' consumido.");
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"[Door System] Error al consumir la llave: {e.Message}");
                    }
                }
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
        CursorManager.Instance?.RequestCursor(this);
    }

    private void HideMouse()
    {
        Debug.Log("[VERTICAL DOOR TRIGGER] CURSOR INVISIBLE");
        CursorManager.Instance?.ReleaseCursor(this);
    }
}