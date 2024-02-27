using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerUI : MonoBehaviour
{
    // Start is called before the first frame update
    public Fighter fighter;
    public CombatManager combatManager;
    public TextMeshProUGUI nameHero;
    public TextMeshProUGUI currentHealth;
    public TextMeshProUGUI maxHealth;
    public TextMeshProUGUI attack;
    public TextMeshProUGUI defense;
    public TextMeshProUGUI speed;
    public SkillUI[] skillsUI;

    public bool isMainCharacterUI;

    public void Awake()
    {
        GetPlayerFromCombatManager();
    }

    private void GetPlayerFromCombatManager()
    {
        if (combatManager == null)
            return;

        var _fighter = combatManager.fighters[combatManager.fighterIndex];

        if (_fighter.GetType() == typeof(PlayerFighter))
        {
            fighter = _fighter;
        }
    }

    public void Start()
    {
        UpdatePlayerStats();
    }

    public void UpdatePlayerStats()
    {
        //if (fighter == null)
        //GetPlayerFromCombatManager();
        if(isMainCharacterUI)
        {
            fighter = GameManager.Instance.character1;
        }
        else
        {
            fighter = GameManager.Instance.character2;
        }

        nameHero.text = fighter.idName;
        currentHealth.text = "HP: " + fighter.GetCurrentStats().health.ToString();
        maxHealth.text = fighter.GetCurrentStats().maxHealth.ToString();
        attack.text = "Attack: " + fighter.GetCurrentStats().attack.ToString();
        defense.text = "Defense: " + fighter.GetCurrentStats().deffense.ToString();
        speed.text = "Speed: " + fighter.GetCurrentStats().speed.ToString();
        UpdateSkillUI();
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
        UpdatePlayerStats();
    }
}
