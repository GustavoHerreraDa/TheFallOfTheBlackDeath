using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static InventoryManager;
using TMPro;
//TP2 GUSTAVO TORRES/FACUNDO FERREIRO
public class PlayerSkillPanel : MonoBehaviour
{
    public GameObject[] skillButtons;
    public TextMeshProUGUI[] skillButtonLabels;

    [Header("Synergy Feedback")]
    public Color synergyColor = Color.yellow;
    public Color normalColor = Color.green;

    private PlayerFighter targetFigther;

    void Awake()
    {
        this.Hide();
    }

    public void ConfigureButton(int index, string skillName)
    {
        if (targetFigther == null || targetFigther.skills == null)
        {
            Debug.LogWarning("[PlayerSkillPanel.ConfigureButton] target or skills null");
            return;
        }
        if (index < 0 || index >= targetFigther.skills.Length || index >= skillButtons.Length || index >= skillButtonLabels.Length)
        {
            Debug.LogWarning($"[PlayerSkillPanel.ConfigureButton] index {index} out of range");
            return;
        }

        var skill = targetFigther.skills[index];
        if (skill == null)
        {
            Debug.LogWarning($"[PlayerSkillPanel.ConfigureButton] skill at {index} is null");
            return;
        }

        bool isUsable = skill.IsUsable(targetFigther);

        this.skillButtons[index].SetActive(true);
        var button = this.skillButtons[index].GetComponent<Button>();
        if (button != null)
        {
            button.interactable = isUsable;

            // Detección de sinergia
            bool synergyAvailable = false;
            if (targetFigther != null && targetFigther.combatManager != null && targetFigther.combatManager.enemyTeam != null)
            {
                foreach (var enemy in targetFigther.combatManager.enemyTeam)
                {
                    if (enemy != null && enemy.isAlive && skill.CanTriggerSynergy(enemy))
                    {
                        synergyAvailable = true;
                        break;
                    }
                }
            }

            // Aplicar feedback visual
            var image = button.GetComponent<Image>();
            if (image != null)
            {
                image.color = synergyAvailable ? synergyColor : normalColor;
            }
        }
        this.skillButtonLabels[index].text = skillName;
    }

    public void ConfigureButton(int index, string skillName, List<InventoryObjectID> itemsNeeded)
    {
        if (targetFigther == null || targetFigther.skills == null)
        {
            Debug.LogWarning("[PlayerSkillPanel.ConfigureButton(items)] target or skills null");
            return;
        }
        if (index < 0 || index >= targetFigther.skills.Length || index >= skillButtons.Length || index >= skillButtonLabels.Length)
        {
            Debug.LogWarning($"[PlayerSkillPanel.ConfigureButton(items)] index {index} out of range");
            return;
        }

        var skill = targetFigther.skills[index];
        if (skill == null)
        {
            Debug.LogWarning($"[PlayerSkillPanel.ConfigureButton(items)] skill at {index} is null");
            return;
        }

        bool hasItems = InventoryManager.instance == null ? true : InventoryManager.instance.HasItemInIventory(itemsNeeded);
        bool hasBodyParts = skill.IsUsable(targetFigther);

        bool interactable = hasItems && hasBodyParts;

        this.skillButtons[index].SetActive(true);
        var button = this.skillButtons[index].GetComponent<Button>();
        if (button != null)
        {
            button.interactable = interactable;

            // Detección de sinergia
            bool synergyAvailable = false;
            if (targetFigther != null && targetFigther.combatManager != null && targetFigther.combatManager.enemyTeam != null)
            {
                foreach (var enemy in targetFigther.combatManager.enemyTeam)
                {
                    if (enemy != null && enemy.isAlive && skill.CanTriggerSynergy(enemy))
                    {
                        synergyAvailable = true;
                        break;
                    }
                }
            }

            // Aplicar feedback visual
            var image = button.GetComponent<Image>();
            if (image != null)
            {
                image.color = synergyAvailable ? synergyColor : normalColor;
            }
        }
        this.skillButtonLabels[index].text = skillName;
    }

    public void OnSkillButtonClick(int index)
    {
        Debug.Log($"[PlayerSkillPanel.OnSkillButtonClick] index={index}");
        if (targetFigther == null)
        {
            Debug.LogWarning("[PlayerSkillPanel.OnSkillButtonClick] targetFigther is null");
            return;
        }
        targetFigther.ExecuteSkill(index);
    }

    public void ShowForPlayer(PlayerFighter newTarget)
    {
        this.gameObject.SetActive(true);

        this.targetFigther = newTarget;

        // Prepare buttons dynamically based on available skills
        int skillsCount = (targetFigther != null && targetFigther.skills != null) ? targetFigther.skills.Length : 0;
        Debug.Log($"[PlayerSkillPanel.ShowForPlayer] skillsCount={skillsCount}");

        // First, deactivate all
        for (int i = 0; i < skillButtons.Length; i++)
        {
            if (skillButtons[i] != null)
            {
                var btnComp = skillButtons[i].GetComponent<Button>();
                if (btnComp != null) btnComp.onClick.RemoveAllListeners();
                skillButtons[i].SetActive(false);
            }
        }

        // Then, activate and configure only the ones we have skills for (up to available UI buttons)
        int shown = Mathf.Min(skillsCount, Mathf.Min(skillButtons.Length, skillButtonLabels.Length));
        for (int i = 0; i < shown; i++)
        {
            // Set label and interactable state via ConfigureButton with ItemsNeeded when available
            var skill = targetFigther.skills[i];
            if (skill != null)
            {
                ConfigureButton(i, skill.skillName, skill.ItemsNeeded);
            }
            else
            {
                ConfigureButton(i, "-?");
            }

            // Closure-safe index capture for button
            int captured = i;
            var button = skillButtons[i].GetComponent<Button>();
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => OnSkillButtonClick(captured));
            }
        }
    }

    public void Hide()
    {
        this.gameObject.SetActive(false);

        foreach (var btn in this.skillButtons)
        {
            if (btn != null) btn.SetActive(false);
        }
    }

    public void Show()
    {
        // Only show the panel; buttons are managed by ShowForPlayer
        this.gameObject.SetActive(true);
    }
}