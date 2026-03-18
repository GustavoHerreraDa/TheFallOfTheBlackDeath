using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSwitcher : MonoBehaviour
{
    public List<GameObject> characters;
    public globalDataBase fightersDateBase;
    public int currentMainCharacterIndex;
    public int currentSecondaryCharacterIndex;

    public delegate void UpdateMainCharacterUI();
    public static event UpdateMainCharacterUI updateMainCharacterUI;
    public delegate void UpdateSecondaryCharacterUI();
    public static event UpdateSecondaryCharacterUI updateSecondaryCharacterUI;

    void Start()
    {
        SetIndex();
        SwitchMainCharacter(currentMainCharacterIndex, true);
        SwitchSecondaryCharacter(currentSecondaryCharacterIndex, true);

        if (characters == null)
            Debug.LogError("characters es null");
        else if (characters.Count == 0)
            Debug.LogError("characters est  vac o!");
    }

    public void SwitchMainCharacter(int characterIndex, bool isFirstTime)
    {
        if (fightersDateBase == null || characters == null || characterIndex < 0 || characterIndex >= characters.Count)
            return;

        // Clear previous main flag without relying on GameManager
        for (int i = 0; i < fightersDateBase.EnemyDB.Count; i++)
        {
            if (fightersDateBase.EnemyDB[i].isMainCharacter)
            {
                fightersDateBase.SetMainCharacter(fightersDateBase.EnemyDB[i].CharacterSwitcherIndex, false);
            }
        }

        currentMainCharacterIndex = characterIndex;
        var pf = characters[characterIndex]?.GetComponent<PlayerFighter>();
        if (pf != null)
        {
            GameManager.Instance?.SetMainCharacter(pf);
            fightersDateBase.SetMainCharacter(pf.figherIndex, true);
        }

        updateMainCharacterUI?.Invoke();
    }

    public void SwitchSecondaryCharacter(int characterIndex, bool isFirstTime)
    {
        if (fightersDateBase == null || characters == null || characterIndex < 0 || characterIndex >= characters.Count)
            return;

        // Clear previous secondary flag without relying on GameManager
        for (int i = 0; i < fightersDateBase.EnemyDB.Count; i++)
        {
            if (fightersDateBase.EnemyDB[i].isSecondaryCharacter)
            {
                fightersDateBase.SetSecondaryCharacter(fightersDateBase.EnemyDB[i].CharacterSwitcherIndex, false);
            }
        }

        currentSecondaryCharacterIndex = characterIndex;
        var pf = characters[characterIndex]?.GetComponent<PlayerFighter>();
        if (pf != null)
        {
            GameManager.Instance?.SetSecondaryCharacter(pf);
            fightersDateBase.SetSecondaryCharacter(pf.figherIndex, true);
        }

        updateSecondaryCharacterUI?.Invoke();
    }

    private void SetIndex()
    {
        for (int i = 0; i < fightersDateBase.EnemyDB.Count; i++)
        {
            if (fightersDateBase.EnemyDB[i].isMainCharacter)
            {
                currentMainCharacterIndex = fightersDateBase.EnemyDB[i].CharacterSwitcherIndex;
                Debug.Log("Main Character es " + fightersDateBase.EnemyDB[i].Name);
            }
        }

        for (int i = 0; i < fightersDateBase.EnemyDB.Count; i++)
        {
            if (fightersDateBase.EnemyDB[i].isSecondaryCharacter)
            {
                currentSecondaryCharacterIndex = fightersDateBase.EnemyDB[i].CharacterSwitcherIndex;
                Debug.Log("Secondary Character es " + fightersDateBase.EnemyDB[i].Name);
            }
        }
    }
}