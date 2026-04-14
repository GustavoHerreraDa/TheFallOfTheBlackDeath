using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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

   /* public void Awake()
    {
        GetPlayerFromCombatManager();
    }*/

    /*private void GetPlayerFromCombatManager()
    {
        if (combatManager == null)
            return;

        var _fighter = combatManager.fighters[combatManager.fighterIndex];

        if (_fighter.GetType() == typeof(PlayerFighter))
        {
            fighter = _fighter;
        }
    }*/

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
            maxHealth.text = stats.maxHealth.ToString();

        if (attack != null)
            attack.text = "Attack: " + stats.attack;

        if (defense != null)
            defense.text = "Defense: " + stats.deffense;

        if (speed != null)
            speed.text = "Speed: " + stats.speed;

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
        // 1. Si cambias de personaje (Tu código actual)
        CharacterSwitcher.updateMainCharacterUI += UpdatePlayerStats;
        CharacterSwitcher.updateSecondaryCharacterUI += UpdatePlayerStats;
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPlayerStatsUpdated += UpdatePlayerStats;
        }
        
        UpdatePlayerStats();
    }

    /// <summary>
    /// Unregisters runtime listeners when the component becomes inactive.
    /// </summary>
    void OnDisable()
    {
        CharacterSwitcher.updateMainCharacterUI -= UpdatePlayerStats;
        CharacterSwitcher.updateSecondaryCharacterUI -= UpdatePlayerStats;
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPlayerStatsUpdated -= UpdatePlayerStats;
        }
    }


}
