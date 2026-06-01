// Cambios: Reemplazo de suscripción a CharacterSwitcher por PartyManager.OnPartyChanged y agregado de wrapper.
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
    [SerializeField] private int partyIndex = 0;

    private PlayerFighter Fighter => ResolveFighter();

    private PlayerFighter ResolveFighter()
    {
        if (GameManager.Instance == null) return null;
        var party = GameManager.Instance.GetPartyMembers();
        if (party == null || partyIndex >= party.Count)
            return GameManager.Instance.character1;
        return party[partyIndex];
    }
    public CombatManager combatManager;
    public TextMeshProUGUI nameHero;
    public TextMeshProUGUI currentHealth;
    public TextMeshProUGUI maxHealth;
    public TextMeshProUGUI attack;
    public TextMeshProUGUI defense;
    public TextMeshProUGUI speed;
    public SkillUI[] skillsUI;
    public BodyStatusUI _boddyStatus;
    public PartyMemberSelectorUI memberSelector;

    // Ya no usamos isMainCharacterUI para decidir qué fighter usar, 
    // ahora se asigna directamente vía Inspector o mediante SetFighter()
    // public bool isMainCharacterUI; 

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

        // Si el Fighter no ha sido asignado aún (vía index), ResolveFighter ya maneja el fallback
        if (Fighter == null)
        {
            Debug.Log("[PlayerUI] Fighter todavía no asignado y no hay líder disponible");
            return;
        }

        var stats = Fighter.GetCurrentStats();

        if (nameHero != null)
            nameHero.text = Fighter.idName;

        if (currentHealth != null)
            currentHealth.text = "HP: " + stats.health;

        if (maxHealth != null)
        {
            float previewVal = isPreview ? Fighter.GetPreviewModifier(previewId, StatType.MaxHealth) : 0;
            maxHealth.text = stats.maxHealth.ToString() + (previewVal != 0 ? " (" + (previewVal > 0 ? "+" : "") + previewVal + ")" : "");
            maxHealth.color = previewVal > 0 ? Color.green : (previewVal < 0 ? Color.red : Color.white);
        }

        if (attack != null)
        {
            float previewVal = isPreview ? Fighter.GetPreviewModifier(previewId, StatType.Attack) : 0;
            attack.text = "Attack: " + stats.attack + (previewVal != 0 ? " (" + (previewVal > 0 ? "+" : "") + previewVal + ")" : "");
            attack.color = previewVal > 0 ? Color.green : (previewVal < 0 ? Color.red : Color.white);
        }

        if (defense != null)
        {
            float previewVal = isPreview ? Fighter.GetPreviewModifier(previewId, StatType.Defense) : 0;
            defense.text = "Defense: " + stats.deffense + (previewVal != 0 ? " (" + (previewVal > 0 ? "+" : "") + previewVal + ")" : "");
            defense.color = previewVal > 0 ? Color.green : (previewVal < 0 ? Color.red : Color.white);
        }

        if (speed != null)
        {
            float previewVal = isPreview ? Fighter.GetPreviewModifier(previewId, StatType.Speed) : 0;
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

            skillsUI[i].player = Fighter.gameObject;
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
        PartyManager.OnPartyChanged += OnPartyChanged;
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPlayerStatsUpdated += RefreshStats;
        }

        if (memberSelector != null)
        {
            memberSelector.OnMemberSelected += SetFighter;
            if (memberSelector.CurrentSelected != null)
            {
                SetFighter(memberSelector.CurrentSelected);
            }
        }
        
        UpdatePlayerStats();
    }

    private void OnPartyChanged() => UpdatePlayerStats(false, "");
    private void RefreshStats() => UpdatePlayerStats(false, "");

    /// <summary>
    /// Asigna dinámicamente el fighter a mostrar y actualiza la UI.
    /// </summary>
    public void SetFighter(PlayerFighter newFighter)
    {
        if (GameManager.Instance == null) return;
        var party = GameManager.Instance.GetPartyMembers();
        partyIndex = party != null ? party.IndexOf(newFighter) : 0;
        if (partyIndex < 0) partyIndex = 0;
        UpdatePlayerStats();
    }

    /// <summary>
    /// Unregisters runtime listeners when the component becomes inactive.
    /// </summary>
    void OnDisable()
    {
        PartyManager.OnPartyChanged -= OnPartyChanged;
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPlayerStatsUpdated -= RefreshStats;
        }

        if (memberSelector != null)
        {
            memberSelector.OnMemberSelected -= SetFighter;
        }
    }


}
