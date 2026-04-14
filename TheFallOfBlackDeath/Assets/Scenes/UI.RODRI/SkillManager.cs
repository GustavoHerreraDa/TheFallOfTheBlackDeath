using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles skill manager for the current project workflow.
/// </summary>
public class SkillManager : MonoBehaviour
{
    private CombatManager combatManager;
    private PlayerFighter fighter;
    public int currentCharacterIndex;
    public GameObject currentCharacterObj;

    public int SetSkill;

    [Header("UI")]
    public PlayerSkillPanel skillPanel;
    public EnemiesPanel enemySelection;
    public BodyPartPanel bodyPartPanel;


    /// <summary>
    /// Initializes cached references and runtime state before the component starts running.
    /// </summary>
    private void Awake()
    {
        fighter = FindObjectOfType<PlayerFighter>();
        combatManager = FindObjectOfType<CombatManager>();
        enemySelection = FindObjectOfType<EnemiesPanel>();
        skillPanel = FindObjectOfType<PlayerSkillPanel>();
        bodyPartPanel = FindObjectOfType<BodyPartPanel>();
    }
    /// <summary>
    /// Initializes the component once the scene dependencies are ready.
    /// </summary>
    void Start()
    {
        //currentCharacterIndex = combatManager.FighterIndex;
        //currentCharacterObj = combatManager.fighters[currentCharacterIndex].gameObject;
    }

    /// <summary>
    /// Sets the execute skill.
    /// </summary>
    /// <param name="index">The index.</param>
    public void SetExecuteSkill(int index)
    {

        currentCharacterIndex = combatManager.fighterIndex;

        currentCharacterObj = combatManager.fighters[currentCharacterIndex].gameObject;

        var Skills = currentCharacterObj.GetComponentsInChildren<Skill>();
        SetSkill = index;
    }
    /// <summary>
    /// Gets the skill description.
    /// </summary>
    /// <param name="Skillindex">The skillindex.</param>
    /// <returns>The resulting value.</returns>
    public string GetSkillDescription(int Skillindex)
    {
        currentCharacterIndex = combatManager.fighterIndex;
        currentCharacterObj = combatManager.fighters[currentCharacterIndex].gameObject;
        var Skills = currentCharacterObj.GetComponentsInChildren<Skill>();
        var selfInflicted = Skills[Skillindex];
        return selfInflicted.SkillDesc;

    }
    /// <summary>
    /// Executes the open panel workflow.
    /// </summary>
    /// <param name="Panel">The panel.</param>
    public void OpenPanel(GameObject Panel)
    {

        Panel.SetActive(true);
        enemySelection.Hide();
        skillPanel.Show();
        Debug.Log("fuiste para atras");
    }


}
