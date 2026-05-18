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

    [Header("Mutilation Feedback")]
    public AudioClip mutilationErrorSound;

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
        if (targetFigther == null || targetFigther.skills == null || index < 0 || index >= targetFigther.skills.Length) return;
        var skill = targetFigther.skills[index];
        if (skill == null) return;

        // Unificamos llamando a la sobrecarga completa
        ConfigureButton(index, skill.skillName, skill.ItemsNeeded);
    }

    /// <summary>
    /// Executes the configure button workflow.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <param name="skillName">The skill name.</param>
    /// <param name="itemsNeeded">The items needed.</param>
    public void ConfigureButton(int index, string skillName, List<Skill.ItemRequirement> itemsNeeded)
    {
        // 1. Validación de seguridad y slot vacío
        if (targetFigther == null || targetFigther.skills == null || index < 0 || 
            index >= targetFigther.skills.Length || index >= skillButtons.Length || index >= skillButtonLabels.Length)
        {
            return;
        }

        var skill = targetFigther.skills[index];
        
        // Si el slot está vacío o no hay skill, reseteamos visualmente y salimos
        if (skill == null)
        {
            this.skillButtons[index].SetActive(true);
            var btn = this.skillButtons[index].GetComponent<Button>();
            if (btn != null) btn.interactable = false;
            this.skillButtonLabels[index].text = "-?";
            return;
        }

        // 2. Recuperar nombre nativo limpio (ignorando el parámetro que pueda venir sucio)
        string cleanSkillName = skill.skillName;
        bool isUsable = skill.IsUsable(targetFigther);
        
        // Determinar si la falta de usabilidad es específicamente por mutilación
        bool isDestroyed = false;
        foreach (var part in skill.requiredParts)
        {
            var bp = targetFigther.GetBodyPart(part);
            if (bp == null || bp.IsDestroyed)
            {
                isDestroyed = true;
                break;
            }
        }

        this.skillButtons[index].SetActive(true);
        var button = this.skillButtons[index].GetComponent<Button>();
        
        if (button != null)
        {
            button.interactable = isUsable;

            // Detección de sinergia
            bool synergyAvailable = false;
            if (targetFigther.combatManager != null && targetFigther.combatManager.enemyTeam != null)
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

            // Feedback visual de la imagen
            var image = button.GetComponent<Image>();
            if (image != null)
            {
                if (isDestroyed)
                {
                    image.color = GetRarityColor(skill.rarity) * 0.5f; 
                }
                else if (synergyAvailable)
                {
                    image.color = synergyColor;
                }
                else
                {
                    image.color = GetRarityColor(skill.rarity);
                }
            }
        }

        // 3. Condicional Visual Estricto: Limpiar o aplicar formato de mutilación
        if (isDestroyed)
        {
            this.skillButtonLabels[index].text = $"<s>{cleanSkillName}</s> <color=red>[MUTILADO]</color>";
        }
        else
        {
            this.skillButtonLabels[index].text = cleanSkillName;
        }
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
            // --- CAPTURA SEGURA DEL ÍNDICE PARA EL CIERRE (CLOSURE) ---
            int captured = i;

            // Set label and interactable state via ConfigureButton
            var skill = targetFigther.skills[captured];
            if (skill != null)
            {
                ConfigureButton(captured, skill.skillName, skill.ItemsNeeded);
            }
            else
            {
                ConfigureButton(captured, "-?");
            }

            var button = skillButtons[captured].GetComponent<Button>();
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => OnSkillButtonClick(captured));
                
                // --- InyecciÃ³n dinÃ¡mica de EventTrigger para Feedback de MutilaciÃ³n ---
                UnityEngine.EventSystems.EventTrigger trigger = skillButtons[captured].GetComponent<UnityEngine.EventSystems.EventTrigger>();
                if (trigger == null) trigger = skillButtons[captured].AddComponent<UnityEngine.EventSystems.EventTrigger>();
                trigger.triggers.Clear();

                // PointerEnter
                UnityEngine.EventSystems.EventTrigger.Entry entryEnter = new UnityEngine.EventSystems.EventTrigger.Entry();
                entryEnter.eventID = UnityEngine.EventSystems.EventTriggerType.PointerEnter;
                entryEnter.callback.AddListener((data) => { OnSkillButtonEnter(captured); });
                trigger.triggers.Add(entryEnter);

                // PointerExit
                UnityEngine.EventSystems.EventTrigger.Entry entryExit = new UnityEngine.EventSystems.EventTrigger.Entry();
                entryExit.eventID = UnityEngine.EventSystems.EventTriggerType.PointerExit;
                entryExit.callback.AddListener((data) => { OnSkillButtonExit(); });
                trigger.triggers.Add(entryExit);
            }
        }
    }

    private void OnSkillButtonEnter(int index)
    {
        if (targetFigther == null || targetFigther.skills == null || index >= targetFigther.skills.Length) return;

        var skill = targetFigther.skills[index];
        if (skill == null) return;

        bool isDestroyed = false;
        string missingPartName = "";
        foreach (var part in skill.requiredParts)
        {
            var bp = targetFigther.GetBodyPart(part);
            if (bp == null || bp.IsDestroyed)
            {
                isDestroyed = true;
                missingPartName = part.ToString();
                break;
            }
        }

        if (isDestroyed)
        {
            if (AudioManager.Instance != null && mutilationErrorSound != null)
            {
                AudioManager.Instance.PlaySFX(mutilationErrorSound, 0.6f, false);
            }

            string warning = $"<color=red>ADVERTENCIA: FALTA EXTREMIDAD ({missingPartName})</color>\n\n";
            Tooltip.ShowTooltip_static(warning + skill.SkillDesc);
        }
        else
        {
            Tooltip.ShowTooltip_static(skill.SkillDesc);
        }
    }

    private void OnSkillButtonExit()
    {
        Tooltip.HideTooltip_static();
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
