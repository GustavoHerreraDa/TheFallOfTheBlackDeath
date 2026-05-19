using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
//TP2 GUSTAVO TORRES/FACUNDO FERREIRO
/// <summary>
/// Supports the combat system by handling player skill panel.
/// </summary>
public class PlayerSkillPanel : MonoBehaviour
{
    public GameObject[] skillButtons;
    public TextMeshProUGUI[] skillButtonLabels;

    [Header("Synergy Feedback")]
    public Color synergyColor = Color.yellow;
    public Color normalColor = Color.chartreuse;

    [Header("Rarity Colors")]
    public Color commonColor = Color.white;
    public Color rareColor = Color.chartreuse;
    public Color epicColor = new Color(0.6f, 0f, 1f);

    private PlayerFighter targetFigther;

    /// <summary>
    /// Initializes cached references and runtime state before the component starts running.
    /// </summary>
    void Awake()
    {
        this.Hide();
    }

    /// <summary>
    /// Executes the configure button workflow.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <param name="skillName">The skill name.</param>
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

            // DetecciÃ³n de sinergia
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
                Color rarityColor = GetRarityColor(skill.rarity);
                if (synergyAvailable)
                {
                    image.color = synergyColor;
                }
                else
                {
                    image.color = rarityColor;
                }
            }
        }
        this.skillButtonLabels[index].text = skillName;
    }

    /// <summary>
    /// Executes the configure button workflow.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <param name="skillName">The skill name.</param>
    /// <param name="itemsNeeded">The items needed.</param>
    public void ConfigureButton(int index, string skillName, List<Skill.ItemRequirement> itemsNeeded)
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
        // Verificar inventario nuevo a través de la propia skill
        bool hasItems = skill.HasRequiredItems();
        // `IsUsable` ya incluye chequeo de partes del cuerpo y de inventario
        bool interactable = skill.IsUsable(targetFigther) && hasItems;

        this.skillButtons[index].SetActive(true);
        var button = this.skillButtons[index].GetComponent<Button>();
        if (button != null)
        {
            button.interactable = interactable;

            // DetecciÃ³n de sinergia
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
                Color rarityColor = GetRarityColor(skill.rarity);
                if (synergyAvailable)
                {
                    image.color = synergyColor;
                }
                else
                {
                    image.color = rarityColor;
                }
            }
        }
        this.skillButtonLabels[index].text = skillName;
    }

    /// <summary>
    /// Executes the on skill button click workflow.
    /// </summary>
    /// <param name="index">The index.</param>
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

    /// <summary>
    /// Gets the rarity color.
    /// </summary>
    /// <param name="rarity">The rarity.</param>
    /// <returns>The resulting value.</returns>
    private Color GetRarityColor(SkillRarity rarity)
    {
        switch (rarity)
        {
            case SkillRarity.Common:
                return commonColor;
            case SkillRarity.Rare:
                return rareColor;
            case SkillRarity.Epic:
                return epicColor;
            default:
                return normalColor;
        }
    }

    /// <summary>
    /// Shows the for player.
    /// </summary>
    /// <param name="newTarget">The new target.</param>
    public void ShowForPlayer(PlayerFighter newTarget)
    {
        
        this.gameObject.SetActive(true);

        this.targetFigther = newTarget;

        if (newTarget.uiAnchor != null)
        {
            Vector3 targetPosition = newTarget.uiAnchor.position;
            targetPosition.y = this.transform.position.y;
            this.transform.position = targetPosition;
            this.transform.rotation = newTarget.uiAnchor.rotation;
        }

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

    /// <summary>
    /// Hides the value.
    /// </summary>
    public void Hide()
    {
        this.gameObject.SetActive(false);

        foreach (var btn in this.skillButtons)
        {
            if (btn != null) btn.SetActive(false);
        }

        Tooltip.HideTooltip_static();
    }

    /// <summary>
    /// Shows the value.
    /// </summary>
    public void Show()
    {
        // Only show the panel; buttons are managed by ShowForPlayer
        this.gameObject.SetActive(true);
    }
}
