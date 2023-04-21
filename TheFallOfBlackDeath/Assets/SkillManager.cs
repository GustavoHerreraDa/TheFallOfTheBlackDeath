using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillManager : MonoBehaviour
{
    private CombatManager combatManager;

    public int currentCharacterIndex;
    public GameObject currentCharacterObj;

    private void Awake()
    {
        combatManager = FindObjectOfType<CombatManager>();
    }
    void Start()
    {
        currentCharacterIndex = combatManager.FighterIndex;
        currentCharacterObj = combatManager.fighters[currentCharacterIndex].gameObject;
    }

    public void ExecuteSkill(int index)
    {
        currentCharacterIndex = combatManager.FighterIndex;

        currentCharacterObj = combatManager.fighters[currentCharacterIndex].gameObject;

        currentCharacterObj.GetComponent<PlayerFighter>().ExecuteSkill(index);
    }
}
