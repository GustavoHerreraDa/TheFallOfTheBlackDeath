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
        fighter = isMainCharacterUI ?
                  GameManager.Instance.character1 :
                  GameManager.Instance.character2;

        
        GameManager.Instance.ApplySavedStatusToFighter(fighter);

        // Leer stats actuales
        var stats = fighter.GetCurrentStats();

        nameHero.text = fighter.idName;
        currentHealth.text = "HP: " + stats.health.ToString();
        maxHealth.text = stats.maxHealth.ToString();
        attack.text = "Attack: " + stats.attack.ToString();
        defense.text = "Defense: " + stats.deffense.ToString();
        speed.text = "Speed: " + stats.speed.ToString();

        UpdateSkillUI();
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
        UpdatePlayerStats();
    }
}
