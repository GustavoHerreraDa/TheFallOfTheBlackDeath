using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TabInventory : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject[] tabs;
    public SkillUI[] skillsUI;

    public TextMeshProUGUI MainCharacterBTN;
    public TextMeshProUGUI SecondaryCharacterBTN;

    public PlayerUI mainCharacterUI;
    public PlayerUI secondaryCharacterUI;

    private void Start()
    {
        for (int i = 0; i < tabs.Length; i++)
        {
            tabs[i].SetActive(false);
        }
        if (tabs.Length > 0)
            tabs[0].SetActive(true);
    }
    public void TurnOnTabs(int tab)
    {
        for (int i = 0; i < tabs.Length; i++)
        {
            tabs[i].SetActive(false);
        }
        tabs[tab - 1].SetActive(true);
    }

    public void UpdateSkillUI()
    {
        for (int i = 0; i < skillsUI.Length; i++)
        {
            skillsUI[i].UpdateUI();
        }
    }

    private void UpdateButtons()
    {
        //MainCharacterBTN.text = GameManager.Instance.character1.idName;
        //SecondaryCharacterBTN.text = GameManager.Instance.character2.idName;
    }

    void OnEnable()
    {
        UpdateButtons();
    }
}
