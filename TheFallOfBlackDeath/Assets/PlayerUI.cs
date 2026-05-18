using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

using InventoryNew;

/// <summary>
/// Handles player ui for the current project workflow.
/// </summary>
public class PlayerUI : MonoBehaviour
{
    // Start is called before the first frame update
    public PlayerFighter fighter;
    public CombatManager combatManager;
    public TextMeshProUGUI nameHero;
    public TextMeshProUGUI currentHealth;
    public TextMeshProUGUI maxHealth;
    public TextMeshProUGUI attack;
    public TextMeshProUGUI defense;
    public TextMeshProUGUI speed;
    public SkillUI[] skillsUI;
    public BodyStatusUI _boddyStatus;

    public bool isMainCharacterUI;

    public string previewItemId = ""; // ID de string para el nuevo sistema

    /// <summary>
    /// Initializes the component once the scene dependencies are ready.
    /// </summary>
    public void Start()
    {
        UpdatePlayerStats();
    }

    /// <summary>
    /// Updates the player stats.
    /// </summary>
    public void UpdatePlayerStats()
    {
        UpdatePlayerStats(false, "");
    }

    /// <summary>
    /// Updates the player stats with optional preview of an item.
    /// </summary>
    public void UpdatePlayerStats(bool isPreview, string previewId)
    {
        if (GameManager.Instance == null)
            return;

        fighter = isMainCharacterUI
            ? GameManager.Instance.character1
            : GameManager.Instance.character2;

        if (fighter == null)
        {
            Debug.Log($"[PlayerUI] Fighter todavía no asignado (isMain={isMainCharacterUI})");
            return;
        }

        var stats = fighter.GetCurrentStats();

        if (nameHero != null)
            nameHero.text = fighter.idName;

        if (currentHealth != null)
            currentHealth.text = "HP: " + stats.health;

        if (maxHealth != null)
        {
            float previewVal = isPreview ? fighter.GetPreviewModifier(previewId, StatType.MaxHealth) : 0;
            maxHealth.text = stats.maxHealth.ToString() + (previewVal != 0 ? " (" + (previewVal > 0 ? "+" : "") + previewVal + ")" : "");
            maxHealth.color = previewVal > 0 ? Color.green : (previewVal < 0 ? Color.red : Color.white);
        }

        if (attack != null)
        {
            float previewVal = isPreview ? fighter.GetPreviewModifier(previewId, StatType.Attack) : 0;
            attack.text = "Attack: " + stats.attack + (previewVal != 0 ? " (" + (previewVal > 0 ? "+" : "") + previewVal + ")" : "");
            attack.color = previewVal > 0 ? Color.green : (previewVal < 0 ? Color.red : Color.white);
        }

        if (defense != null)
        {
            float previewVal = isPreview ? fighter.GetPreviewModifier(previewId, StatType.Defense) : 0;
            defense.text = "Defense: " + stats.deffense + (previewVal != 0 ? " (" + (previewVal > 0 ? "+" : "") + previewVal + ")" : "");
            defense.color = previewVal > 0 ? Color.green : (previewVal < 0 ? Color.red : Color.white);
        }

        if (speed != null)
        {
            float previewVal = isPreview ? fighter.GetPreviewModifier(previewId, StatType.Speed) : 0;
            speed.text = "Speed: " + stats.speed + (previewVal != 0 ? " (" + (previewVal > 0 ? "+" : "") + previewVal + ")" : "");
            speed.color = previewVal > 0 ? Color.green : (previewVal < 0 ? Color.red : Color.white);
        }

        UpdateSkillUI();

        if (_boddyStatus != null)
            _boddyStatus.Refresh();
        
    }


    /// <summary>
    /// Updates the skill ui.
    /// </summary>
    private void UpdateSkillUI()
    {
        if (skillsUI == null) return;

        for (int i = 0; i < skillsUI.Length; i++)
        {
            // Comprobación de nulidad robusta para SkillUI
            if (skillsUI[i] == null) continue;

            skillsUI[i].player = fighter.gameObject;
            skillsUI[i].skill = null;
            skillsUI[i].UpdateUI();
        }
    }

    //Actualizo la UI en cada recarga para mostrar los cambios cuando equipo un item.
    /// <summary>
    /// Registers runtime listeners when the component becomes active.
    /// </summary>
    void OnEnable()
    {
        CharacterSwitcher.updateMainCharacterUI += UpdatePlayerStats;
        CharacterSwitcher.updateSecondaryCharacterUI += UpdatePlayerStats;
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPlayerStatsUpdated += RefreshStats;
        }
        
        UpdatePlayerStats();
    }

    private void RefreshStats() => UpdatePlayerStats(false, "");

    /// <summary>
    /// Unregisters runtime listeners when the component becomes inactive.
    /// </summary>
    void OnDisable()
    {
        CharacterSwitcher.updateMainCharacterUI -= UpdatePlayerStats;
        CharacterSwitcher.updateSecondaryCharacterUI -= UpdatePlayerStats;
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPlayerStatsUpdated -= RefreshStats;
        }
    }


}
