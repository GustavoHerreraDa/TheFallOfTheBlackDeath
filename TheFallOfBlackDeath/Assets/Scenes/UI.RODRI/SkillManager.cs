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
    public EnemiesPanel enemySelection;


    private void Awake()
    {
        combatManager = FindObjectOfType<CombatManager>();
        enemySelection = FindObjectOfType<EnemiesPanel>();
        skillPanel = FindObjectOfType<PlayerSkillPanel>();
    }
    void Start()
    {
        //currentCharacterIndex = combatManager.FighterIndex;
        //currentCharacterObj = combatManager.fighters[currentCharacterIndex].gameObject;
    }

    public void SetExecuteSkill(int index)
    {

        currentCharacterIndex = combatManager.fighterIndex;

        currentCharacterObj = combatManager.fighters[currentCharacterIndex].gameObject;

        var Skills = currentCharacterObj.GetComponentsInChildren<Skill>();
        SetSkill = index;



    }
    public string GetSkillDescription(int Skillindex)
    {
        currentCharacterIndex = combatManager.fighterIndex;
        currentCharacterObj = combatManager.fighters[currentCharacterIndex].gameObject;
        var Skills = currentCharacterObj.GetComponentsInChildren<Skill>();
        var selfInflicted = Skills[Skillindex];
        return selfInflicted.SkillDesc;

    }
    public void OpenPanel(GameObject Panel)
    {

        Panel.SetActive(true);
        enemySelection.Hide();
        skillPanel.Show();
        Debug.Log("fuiste para atras");
    }


}
