using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using InventoryNew;
//TP2 GUSTAVO TORRES/FACUNDO FERREIRO

/// <summary>
/// Supports the combat system by handling player skill panel with full support for dynamic skill loadouts,
/// equipment-granted skills, and persistent skill restoration.
/// </summary>
public class PlayerSkillPanel : MonoBehaviour
{
    public GameObject[] skillButtons;
    public TextMeshProUGUI[] skillButtonLabels;

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
    
    // NUEVO: Escuchar cambios en el equipo para refrescar el panel
    private EquipmentHandler currentEquipmentHandler;

    /// <summary>
    /// Initializes cached references and runtime state before the component starts running.
    /// </summary>
    void Awake()
    {
        this.Hide();
    }

    /// <summary>
    /// Cleanup on destroy to avoid listener leaks
    /// </summary>
    private void OnDestroy()
    {
        UnsubscribeFromEquipmentChanges();
    }

    /// <summary>
    /// Executes the configure button workflow for a single skill.
    /// </summary>
    /// <param name="index">The button index.</param>
    /// <param name="skill">The skill to configure.</param>
    private void ConfigureButton(int index, Skill skill)
    {
        // VALIDACIÓN: Verificar que el índice está en rango
        if (index < 0 || index >= skillButtons.Length || index >= skillButtonLabels.Length)
        {
            Debug.LogWarning($"[PlayerSkillPanel.ConfigureButton] index {index} out of range");
            return;
        }

        // VALIDACIÓN: skill nulo
        if (skill == null)
        {
            Debug.LogWarning($"[PlayerSkillPanel.ConfigureButton] skill at {index} is null");
            return;
        }

        // VALIDACIÓN: target nulo
        if (targetFigther == null)
        {
            Debug.LogWarning("[PlayerSkillPanel.ConfigureButton] targetFigther is null");
            return;
        }

        // 1. Verificar si la skill es usable
        bool isUsable = skill.IsUsable(targetFigther);

        // 2. Activar botón
        this.skillButtons[index].SetActive(true);
        var button = this.skillButtons[index].GetComponent<Button>();
        
        if (button != null)
        {
            button.interactable = isUsable;

            // 3. Detectar sinergia disponible
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

            // 4. Aplicar feedback visual (color)
            var image = button.GetComponent<Image>();
            if (image != null)
            {
                Color rarityColor = GetRarityColor(skill.rarity);
                image.color = synergyAvailable ? synergyColor : rarityColor;
            }
        }

        // 5. Configurar etiqueta del botón
        this.skillButtonLabels[index].text = skill.skillName;
    }

    /// <summary>
    /// Gets the rarity color for a skill.
    /// </summary>
    /// <param name="rarity">The rarity.</param>
    /// <returns>The corresponding color.</returns>
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

        // VALIDACIÓN: Verificar que el índice es válido en el array actual
        if (targetFigther.skills == null || index < 0 || index >= targetFigther.skills.Length)
        {
            Debug.LogError($"[PlayerSkillPanel.OnSkillButtonClick] Invalid skill index {index}. Current skills count: {(targetFigther.skills != null ? targetFigther.skills.Length : 0)}");
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

    /// <summary>
    /// Shows the skill panel for a specific player fighter with full dynamic configuration.
    /// Handles equipment-granted skills, persistent loadouts, and synergy detection.
    /// </summary>
    /// <param name="newTarget">The player fighter to display skills for.</param>
    public void ShowForPlayer(PlayerFighter newTarget)
    {
        if (newTarget == null)
        {
            Debug.LogWarning("[PlayerSkillPanel.ShowForPlayer] newTarget is null");
            return;
        }

        this.gameObject.SetActive(true);
        this.targetFigther = newTarget;

        // NUEVO: Suscribirse a cambios de equipo para refrescar el panel
        SubscribeToEquipmentChanges();

        if (targetFigther.combatManager != null)
        {
            targetFigther.combatManager.InvokeOnSkillMenuOpened();
        }

        // Posicionar el panel junto al personaje
        if (newTarget.uiAnchor != null)
        {
            Vector3 targetPosition = newTarget.uiAnchor.position;
            targetPosition.y = this.transform.position.y;
            this.transform.position = targetPosition;
            this.transform.rotation = newTarget.uiAnchor.rotation;
        }

        // CRÍTICO: Usar el pool actual de skills activas
        int skillsCount = (targetFigther.skills != null) ? targetFigther.skills.Length : 0;
        Debug.Log($"[PlayerSkillPanel.ShowForPlayer] Showing {skillsCount} active skills for {newTarget.idName}");

        // 1. Limpiar todos los botones
        for (int i = 0; i < skillButtons.Length; i++)
        {
            if (skillButtons[i] != null)
            {
                var btnComp = skillButtons[i].GetComponent<Button>();
                if (btnComp != null) btnComp.onClick.RemoveAllListeners();
                skillButtons[i].SetActive(false);
            }
        }

        // 2. Configurar botones para las skills activas disponibles
        int shown = Mathf.Min(skillsCount, Mathf.Min(skillButtons.Length, skillButtonLabels.Length));
        
        for (int i = 0; i < shown; i++)
        {
            Skill skill = targetFigther.skills[i];
            
            if (skill == null)
            {
                Debug.LogWarning($"[PlayerSkillPanel.ShowForPlayer] Skill at active index {i} is null");
                continue;
            }

            // Configurar el botón con la skill
            ConfigureButton(i, skill);

            // Configurar el listener del botón (captura segura del índice)
            int capturedIndex = i;
            var button = skillButtons[i].GetComponent<Button>();
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => OnSkillButtonClick(capturedIndex));
            }
        }

        // 3. Configurar botón de escape
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
    /// Hides the skill panel and cleans up event listeners.
    /// </summary>
    public void Hide()
    {
        UnsubscribeFromEquipmentChanges();

        if (targetFigther != null && targetFigther.combatManager != null)
        {
            targetFigther.combatManager.InvokeOnSkillMenuClosed();
        }

        this.gameObject.SetActive(false);

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
    /// Shows the skill panel (buttons already configured by ShowForPlayer).
    /// </summary>
    public void Show()
    {
        this.gameObject.SetActive(true);
    }

    /// <summary>
    /// NUEVO: Se suscribe a los cambios de equipo para refrescar el panel cuando cambian las skills otorgadas.
    /// </summary>
    private void SubscribeToEquipmentChanges()
    {
        UnsubscribeFromEquipmentChanges();

        if (targetFigther == null || targetFigther.equipmentHandler == null)
            return;

        currentEquipmentHandler = targetFigther.equipmentHandler;
        currentEquipmentHandler.OnEquipChanged += RefreshSkillPanel;
    }

    /// <summary>
    /// NUEVO: Se desuscribe de los cambios de equipo.
    /// </summary>
    private void UnsubscribeFromEquipmentChanges()
    {
        if (currentEquipmentHandler != null)
        {
            currentEquipmentHandler.OnEquipChanged -= RefreshSkillPanel;
            currentEquipmentHandler = null;
        }
    }

    /// <summary>
    /// NUEVO: Refresca el panel de skills cuando se equipan/desequipan items.
    /// </summary>
    private void RefreshSkillPanel()
    {
        Debug.Log("[PlayerSkillPanel.RefreshSkillPanel] Equipo cambió, refresca el panel de skills");
        
        if (targetFigther != null && this.gameObject.activeSelf)
        {
            ShowForPlayer(targetFigther);
        }
    }
}
