using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


/// <summary>
/// Handles tab inventory for the current project workflow.
/// </summary>
public class TabInventory : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject[] tabs;
    public SkillUI[] skillsUI;

    public TextMeshProUGUI MainCharacterBTN;
    public TextMeshProUGUI SecondaryCharacterBTN;

    public PlayerUI mainCharacterUI;
    public PlayerUI secondaryCharacterUI;


    /// <summary>
    /// Initializes the component once the scene dependencies are ready.
    /// </summary>
    private void Start()
    {
        for (int i = 0; i < tabs.Length; i++)
        {
            tabs[i].SetActive(false);
        }
        if (tabs.Length > 0)
            tabs[0].SetActive(true);

    }
    /// <summary>
    /// Executes the turn on tabs workflow.
    /// </summary>
    /// <param name="tab">The tab.</param>
    public void TurnOnTabs(int tab)
    {
        for (int i = 0; i < tabs.Length; i++)
        {
            tabs[i].SetActive(false);
        }
        tabs[tab - 1].SetActive(true);
    }

    /// <summary>
    /// Updates the skill ui.
    /// </summary>
    public void UpdateSkillUI()
    {
        for (int i = 0; i < skillsUI.Length; i++)
        {
            skillsUI[i].UpdateUI();
        }
    }

    /// <summary>
    /// Updates the buttons.
    /// </summary>
    private void UpdateButtons()
    {
        //MainCharacterBTN.text = GameManager.Instance.character1.idName;
        //SecondaryCharacterBTN.text = GameManager.Instance.character2.idName;
    }

    /// <summary>
    /// Registers runtime listeners when the component becomes active.
    /// </summary>
    void OnEnable()
    {
        UpdateButtons();
    }
}
