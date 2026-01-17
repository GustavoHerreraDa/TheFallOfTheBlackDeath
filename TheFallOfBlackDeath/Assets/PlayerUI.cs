using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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

    public void Start()
    {
        UpdatePlayerStats();

    }

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


    private void UpdateSkillUI()
    {
        for (int i = 0; i < skillsUI.Length; i++)
        {
            skillsUI[i].player = fighter.gameObject;
            skillsUI[i].skill = null;
            skillsUI[i].UpdateUI();
        }
    }

    //Actualizo la UI en cada recarga para mostrar los cambios cuando equipo un item.
    void OnEnable()
    {
        CharacterSwitcher.updateMainCharacterUI += UpdatePlayerStats;
        CharacterSwitcher.updateSecondaryCharacterUI += UpdatePlayerStats;
    }

    void OnDisable()
    {
        CharacterSwitcher.updateMainCharacterUI -= UpdatePlayerStats;
        CharacterSwitcher.updateSecondaryCharacterUI -= UpdatePlayerStats;
    }


}
