using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TabInventory : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject[] tabs;
    public SkillUI[] skillsUI;

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
}
