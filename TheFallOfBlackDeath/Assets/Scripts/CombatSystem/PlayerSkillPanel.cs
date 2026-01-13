using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static InventoryManager;
using TMPro;
//TP2 GUSTAVO TORRES/FACUNDO FERREIRO
public class PlayerSkillPanel : MonoBehaviour
{
    public GameObject[] skillButtons;
    public Text[] skillButtonLabels;

    private PlayerFighter targetFigther;

    void Awake()
    {
        this.Hide();
    }

    public void ConfigureButton(int index, string skillName)
    {
        var skill = targetFigther.skills[index];

        bool isUsable = skill.IsUsable(targetFigther);

        this.skillButtons[index].SetActive(true);
        this.skillButtons[index].GetComponent<Button>().interactable = isUsable;
        this.skillButtonLabels[index].text = skillName;
    }

    public void ConfigureButton(int index, string skillName, List<InventoryObjectID> itemsNeeded)
{
    var skill = targetFigther.skills[index];

    bool hasItems = InventoryManager.instance == null ? true : InventoryManager.instance.HasItemInIventory(itemsNeeded);
    bool hasBodyParts = skill.IsUsable(targetFigther);

    bool interactable = hasItems && hasBodyParts;

    this.skillButtons[index].SetActive(true);
    this.skillButtons[index].GetComponent<Button>().interactable = interactable;
    this.skillButtonLabels[index].text = skillName;
}



    public void OnSkillButtonClick(int index)
    {
        this.targetFigther.ExecuteSkill(index);
    }

    public void ShowForPlayer(PlayerFighter newTarget)
    {
        this.gameObject.SetActive(true);

        this.targetFigther = newTarget;
    }

    public void Hide()
    {
        this.gameObject.SetActive(false);

        foreach (var btn in this.skillButtons)
        {
            btn.SetActive(false);
        }
    }

    public void Show()
    {
        this.gameObject.SetActive(true);

        foreach (var btn in this.skillButtons)
        {
            btn.SetActive(true);
        }
    }



}