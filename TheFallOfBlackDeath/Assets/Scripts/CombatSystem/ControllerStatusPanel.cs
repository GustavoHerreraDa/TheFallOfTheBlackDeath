using UnityEngine;

/// <summary>
/// Supports the combat system by handling status panel controller.
/// </summary>
public class StatusPanelController : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private KeyCode toggleKey = KeyCode.Tab;

    private bool isOpen = false;

    private PlayerFighter mainFighter;
    private PlayerFighter secondaryFighter;

    private StatusPanel mainStatusPanel;
    private StatusPanel secondaryStatusPanel;

    /// <summary>
    /// Initializes the component once the scene dependencies are ready.
    /// </summary>
    private void Start()
    {
        FindFightersAndPanels();
        SetPanelsActive(false);
    }

    /// <summary>
    /// Updates the component each frame while it is active.
    /// </summary>
    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            ToggleStatusPanel();
        }
    }

    /// <summary>
    /// Finds the fighters and panels.
    /// </summary>
    private void FindFightersAndPanels()
    {
        if (GameManager.Instance == null) return;

        mainFighter = GameManager.Instance.character1;
        secondaryFighter = GameManager.Instance.character2;

        if (mainFighter != null)
        {
            mainStatusPanel = mainFighter.statusPanel;
        }

        if (secondaryFighter != null)
        {
            secondaryStatusPanel = secondaryFighter.statusPanel;
        }
    }

    /// <summary>
    /// Executes the toggle status panel workflow.
    /// </summary>
    public void ToggleStatusPanel()
    {
        isOpen = !isOpen;

        SetPanelsActive(isOpen);

        if (isOpen)
        {
            RefreshAllUI();
        }
    }

    /// <summary>
    /// Sets the panels active.
    /// </summary>
    /// <param name="value">The value.</param>
    private void SetPanelsActive(bool value)
    {
        if (mainStatusPanel != null)
            mainStatusPanel.gameObject.SetActive(value);

        if (secondaryStatusPanel != null)
            secondaryStatusPanel.gameObject.SetActive(value);
    }

    /// <summary>
    /// Refreshes the all ui.
    /// </summary>
    private void RefreshAllUI()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RefreshUI();
        }

        if (mainFighter != null && mainFighter.statusPanel != null)
        {
            mainFighter.statusPanel.SetStats(mainFighter.idName, mainFighter.stats);
        }

        if (secondaryFighter != null && secondaryFighter.statusPanel != null)
        {
            secondaryFighter.statusPanel.SetStats(secondaryFighter.idName, secondaryFighter.stats);
        }
    }
}
