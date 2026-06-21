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

    [Header("New Main Menu")]
    public GameObject mainSkillsMenuButton;
    public GameObject scanButton;
    public GameObject backButton;

    [Header("Escape")]
    public GameObject runButton;
    public TextMeshProUGUI runButtonLabel;
    [SerializeField] private string runButtonText = "RUN";

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

    public void OnRunButtonClick()
    {
        if (targetFigther == null)
        {
            Debug.LogWarning("[PlayerSkillPanel.OnRunButtonClick] targetFigther is null");
            return;
        }

        targetFigther.AttemptRun();
    }

    public void InitializeTurnUI(PlayerFighter fighter)
    {
        this.targetFigther = fighter;
        this.gameObject.SetActive(true);

        // Estado Inicial: Mostrar menú de opciones (Skills, Run, Scan)
        // Ocultar el panel de habilidades individual y el botón de Volver
        foreach (var btn in skillButtons)
        {
            if (btn != null) btn.SetActive(false);
        }
        
        if (backButton != null) backButton.SetActive(false);

        // Configurar y mostrar botones raíz
        if (mainSkillsMenuButton != null)
        {
            mainSkillsMenuButton.SetActive(true);
            var button = mainSkillsMenuButton.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() =>
                {
                    ShowForPlayer(targetFigther);
                });
            }
        }
        
        if (runButton != null)
        {
            runButton.SetActive(true);
            var button = runButton.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(OnRunButtonClick);
            }
        }

        if (scanButton != null)
        {
            scanButton.SetActive(true);
            // El listener suele estar en CombatScannerButtonLinker, 
            // pero nos aseguramos que sea visible.
        }

        if (targetFigther != null && targetFigther.uiAnchor != null)
        {
            Vector3 targetPosition = targetFigther.uiAnchor.position;
            targetPosition.y = this.transform.position.y;
            this.transform.position = targetPosition;
            this.transform.rotation = targetFigther.uiAnchor.rotation;
        }
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

        if (targetFigther != null && targetFigther.combatManager != null)
        {
            targetFigther.combatManager.InvokeOnSkillMenuOpened();
        }
        
        if (CameraDirector.Instance != null)
            CameraDirector.Instance.FocusSkillPanelOn(targetFigther);

        // Ocultar botones raíz
        if (mainSkillsMenuButton != null) mainSkillsMenuButton.SetActive(false);
        if (runButton != null) runButton.SetActive(false);
        if (scanButton != null) scanButton.SetActive(false);

        // Mostrar botón de volver
        if (backButton != null)
        {
            backButton.SetActive(true);
            var btn = backButton.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => InitializeTurnUI(targetFigther));
            }
        }

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

        ConfigureRunButton(shown);
    }

    private void ConfigureRunButton(int firstFreeButtonIndex)
    {
        GameObject buttonObject = runButton;
        TextMeshProUGUI label = runButtonLabel;

        if (buttonObject == null &&
            firstFreeButtonIndex >= 0 &&
            firstFreeButtonIndex < skillButtons.Length)
        {
            buttonObject = skillButtons[firstFreeButtonIndex];
            if (firstFreeButtonIndex < skillButtonLabels.Length)
                label = skillButtonLabels[firstFreeButtonIndex];
        }

        if (buttonObject == null)
            return;

        bool canRun = targetFigther != null && targetFigther.combatManager != null;
        buttonObject.SetActive(canRun);

        var button = buttonObject.GetComponent<Button>();
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.interactable = canRun;
            button.onClick.AddListener(OnRunButtonClick);

            var image = button.GetComponent<Image>();
            if (image != null)
                image.color = normalColor;
        }

        if (label == null)
            label = buttonObject.GetComponentInChildren<TextMeshProUGUI>();

        if (label != null)
            label.text = runButtonText;
    }

    /// <summary>
    /// Hides the value.
    /// </summary>
    public void Hide()
    {
        if (targetFigther != null && targetFigther.combatManager != null)
        {
            targetFigther.combatManager.InvokeOnSkillMenuClosed();
        }

        if (CameraDirector.Instance != null &&
            CameraDirector.Instance.CurrentState == CameraState.SkillPanel)
        {
            CameraDirector.Instance.ChangeState(CameraDirector.Instance.StateBeforeUi);
        }

        this.gameObject.SetActive(false);

        if (mainSkillsMenuButton != null)
        {
            var button = mainSkillsMenuButton.GetComponent<Button>();
            if (button != null)
                button.onClick.RemoveAllListeners();

            mainSkillsMenuButton.SetActive(false);
        }

        if (scanButton != null) scanButton.SetActive(false);

        if (backButton != null)
        {
            var button = backButton.GetComponent<Button>();
            if (button != null)
                button.onClick.RemoveAllListeners();

            backButton.SetActive(false);
        }

        foreach (var btn in this.skillButtons)
        {
            if (btn == null) continue;

            var button = btn.GetComponent<Button>();
            if (button != null)
                button.onClick.RemoveAllListeners();

            btn.SetActive(false);
        }

        if (runButton != null)
        {
            var button = runButton.GetComponent<Button>();
            if (button != null)
                button.onClick.RemoveAllListeners();

            runButton.SetActive(false);
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
