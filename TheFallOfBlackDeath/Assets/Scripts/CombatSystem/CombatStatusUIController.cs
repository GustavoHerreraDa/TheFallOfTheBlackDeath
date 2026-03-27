using UnityEngine;
using System.Collections.Generic;

public class CombatStatusUIController : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private KeyCode toggleKey = KeyCode.Tab;

    private bool isStatusPanelOpen = false;

    private List<PlayerFighter> playerFighters = new List<PlayerFighter>();

    private void Start()
    {
        // Asegurar que todos los paneles arranquen apagados
        SetStatusPanelsActive(false);
    }

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

    private void ToggleStatusPanels()
    {
        isStatusPanelOpen = !isStatusPanelOpen;

        SetStatusPanelsActive(isStatusPanelOpen);

        if (isStatusPanelOpen)
        {
            RefreshStatusPanels();
        }
    }

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
    public void ForceRefresh()
    {
        if (isStatusPanelOpen)
        {
            RefreshStatusPanels();
        }
    }

    public bool IsOpen()
    {
        return isStatusPanelOpen;
    }
    
    public void RegisterPlayer(PlayerFighter pf)
    {
        if (!playerFighters.Contains(pf))
            playerFighters.Add(pf);
    }
}