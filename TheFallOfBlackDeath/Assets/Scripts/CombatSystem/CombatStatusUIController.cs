using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Supports the combat system by handling combat status ui controller.
/// </summary>
public class CombatStatusUIController : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private KeyCode toggleKey = KeyCode.Tab;

    private bool isStatusPanelOpen = false;

    private List<PlayerFighter> playerFighters = new List<PlayerFighter>();

    /// <summary>
    /// Initializes the component once the scene dependencies are ready.
    /// </summary>
    private void Start()
    {
        // Asegurar que todos los paneles arranquen apagados
        SetStatusPanelsActive(false);
    }

    /// <summary>
    /// Updates the component each frame while it is active.
    /// </summary>
    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            Debug.Log("TOGGLE");

            if (playerFighters.Count == 0)
            {
                Debug.LogError("NO HAY PLAYER FIGHTERS REGISTRADOS");
            }

            ToggleStatusPanels();
        }
    }

    /// <summary>
    /// Executes the toggle status panels workflow.
    /// </summary>
    private void ToggleStatusPanels()
    {
        isStatusPanelOpen = !isStatusPanelOpen;

        SetStatusPanelsActive(isStatusPanelOpen);

        if (isStatusPanelOpen)
        {
            RefreshStatusPanels();
        }
    }

    /// <summary>
    /// Sets the status panels active.
    /// </summary>
    /// <param name="active">The active.</param>
    private void SetStatusPanelsActive(bool active)
    {
        foreach (var pf in playerFighters)
        {
            if (pf != null && pf.statusPanel != null)
            {
                pf.statusPanel.gameObject.SetActive(active);
            }
        }
    }

    /// <summary>
    /// Refreshes the status panels.
    /// </summary>
    private void RefreshStatusPanels()
    {
        foreach (var pf in playerFighters)
        {
            if (pf != null && pf.statusPanel != null)
            {
                pf.statusPanel.SetStats(pf.idName, pf.stats);
            }
        }
    }

    // Por si querés refrescar desde otro sistema (ej: después de curar)
    /// <summary>
    /// Executes the force refresh workflow.
    /// </summary>
    public void ForceRefresh()
    {
        if (isStatusPanelOpen)
        {
            RefreshStatusPanels();
        }
    }

    /// <summary>
    /// Determines whether the component is open.
    /// </summary>
    /// <returns>True when the requested condition is met; otherwise, false.</returns>
    public bool IsOpen()
    {
        return isStatusPanelOpen;
    }
    
    /// <summary>
    /// Executes the register player workflow.
    /// </summary>
    /// <param name="pf">The pf.</param>
    public void RegisterPlayer(PlayerFighter pf)
    {
        if (!playerFighters.Contains(pf))
            playerFighters.Add(pf);
    }
}
