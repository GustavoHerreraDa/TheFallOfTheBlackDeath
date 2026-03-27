using UnityEngine;

public class StatusPanelController : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private KeyCode toggleKey = KeyCode.Tab;

    private bool isOpen = false;

    private PlayerFighter mainFighter;
    private PlayerFighter secondaryFighter;

    private StatusPanel mainStatusPanel;
    private StatusPanel secondaryStatusPanel;

    private void Start()
    {
        FindFightersAndPanels();
        SetPanelsActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            ToggleStatusPanel();
        }
    }

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

    public void ToggleStatusPanel()
    {
        isOpen = !isOpen;

        SetPanelsActive(isOpen);

        if (isOpen)
        {
            RefreshAllUI();
        }
    }

    private void SetPanelsActive(bool value)
    {
        if (mainStatusPanel != null)
            mainStatusPanel.gameObject.SetActive(value);

        if (secondaryStatusPanel != null)
            secondaryStatusPanel.gameObject.SetActive(value);
    }

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