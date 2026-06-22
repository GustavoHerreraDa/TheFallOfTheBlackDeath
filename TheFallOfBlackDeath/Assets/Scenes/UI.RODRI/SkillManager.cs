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
    [SerializeField] public PlayerSkillPanel skillPanel;
    [SerializeField] public EnemiesPanel enemySelection;
    [SerializeField] public BodyPartPanel bodyPartPanel;


    /// <summary>
    /// Initializes cached references and runtime state before the component starts running.
    /// </summary>
    private void Awake()
    {
        if (fighter == null) fighter = FindObjectOfType<PlayerFighter>();
        if (combatManager == null) combatManager = FindObjectOfType<CombatManager>();
        
        // Prefer references already set in inspector. If null, try to find them (but warn if they might be inactive)
        if (enemySelection == null) enemySelection = FindObjectOfType<EnemiesPanel>(true);
        if (skillPanel == null) skillPanel = FindObjectOfType<PlayerSkillPanel>(true);
        if (bodyPartPanel == null) bodyPartPanel = FindObjectOfType<BodyPartPanel>(true);
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
       
        if (Panel != null)
        {
            Panel.SetActive(true);
        }
        else
        {
            Debug.LogWarning("[SkillManager.OpenPanel] Panel is null. Check Inspector references.");
        }

        
        if (enemySelection != null) enemySelection.Hide();
        if (bodyPartPanel != null) bodyPartPanel.Hide();
        if (skillPanel != null) skillPanel.Hide();

        if (combatManager == null) combatManager = FindObjectOfType<CombatManager>();
        if (combatManager == null)
        {
            Debug.LogError("[SkillManager.OpenPanel] CombatManager not found.");
            return;
        }

        Fighter currentFighter = combatManager.CurrentFighter;
    
        if (currentFighter is PlayerFighter playerFighter)
        {
            playerFighter.Return();
            Debug.Log("Panel de habilidades refrescado vía PlayerFighter.Return() para: " + playerFighter.idName);
        }
        else if (currentFighter == null)
        {
            Debug.LogWarning("[SkillManager.OpenPanel] CurrentFighter is null.");
        }
    }


}
