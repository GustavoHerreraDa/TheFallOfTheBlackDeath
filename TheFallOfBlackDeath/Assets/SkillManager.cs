using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillManager : MonoBehaviour
{
    private CombatManager combatManager;

    public int currentCharacterIndex;
    public GameObject currentCharacterObj;

    public int SetSkill;

    [Header("UI")]
    public PlayerSkillPanel skillPanel;
    public EnemySelectionPanel enemySelection;


    private void Awake()
    {
        combatManager = FindObjectOfType<CombatManager>();
        enemySelection = FindObjectOfType<EnemySelectionPanel>();
        skillPanel = FindObjectOfType<PlayerSkillPanel>();
    }
    void Start()
    {
        //currentCharacterIndex = combatManager.FighterIndex;
        //currentCharacterObj = combatManager.fighters[currentCharacterIndex].gameObject;
    }

    public void SetExecuteSkill(int index)
    {
        Debug.Log("SetExecuteSkill " + index);

        currentCharacterIndex = combatManager.FighterIndex;

        currentCharacterObj = combatManager.fighters[currentCharacterIndex].gameObject;

        var Skills = currentCharacterObj.GetComponentsInChildren<Skill>();
        var selfInflicted = Skills[index];

        Debug.Log(selfInflicted.skillName);
        if (selfInflicted.selfInflicted)
        {
            enemySelection.EnableButtonsWithPlayers();
        }
        else
        {
            enemySelection.EnableButtonsWithEnemy();
        }
        
        skillPanel.Hide();
        enemySelection.Show();

        SetSkill = index;

    
    }

    public void ExecuteSkill(int EnemyIndex)
    {
        combatManager.SetOpposingEnemy(EnemyIndex);

        currentCharacterObj.GetComponent<PlayerFighter>().ExecuteSkill(SetSkill);

        enemySelection.Hide();

    }

}
