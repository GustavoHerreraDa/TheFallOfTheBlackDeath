
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static InventoryManager;

public class PlayerSkillPanel : MonoBehaviour
{
    public GameObject[] skillButtons;
    public Text[] skillButtonLabels;
    public GameObject skillPanel;
    //public InventoryManager inventoryManager;

    private void Awake()
    {
        this.Hide();

        foreach (var btn in this.skillButtons)
        {
            btn.SetActive(false);
        }

    }

    private void Start()
    {
        //inventoryManager = FindObjectOfType<InventoryManager>();
    }

    public void ConfigureButtons(int index, string skillName)
    {
        this.skillButtons[index].SetActive(true);
        this.skillButtonLabels[index].text = skillName;
    }
    public void ConfigureButtons(int index, string skillName, List<InventoryObjectID> itemsNeeded)
    {
        var hasItems = InventoryManager.instance == null ? true : InventoryManager.instance.HasItemInIventory(itemsNeeded);

        Debug.Log("ConfigureButtons - skill name + " + skillName + "  " + itemsNeeded.Count + " hasItems " + hasItems);


        //if (!hasItems)
        //    return;

        this.skillButtons[index].SetActive(true);
        this.skillButtons[index].GetComponent<Button>().interactable = hasItems;
        this.skillButtonLabels[index].text = skillName;
    }

    public void Show()
    {
        this.skillPanel.SetActive(true);
    }

    public void Hide()
    {
        this.skillPanel.SetActive(false);
    }

    internal void SetActive(bool v)
    {
        throw new NotImplementedException();
    }
}
