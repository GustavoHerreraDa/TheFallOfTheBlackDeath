using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSwitcher : MonoBehaviour
{
    public List<GameObject> characters;
    public EnemyDataBase fightersDateBase;
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
    }

    public void SwitchMainCharacter(int characterIndex, bool isFirstTime)
    {
        if(characterIndex == currentSecondaryCharacterIndex)
        {
            return;
        }

        fightersDateBase.SetMainCharacter(GameManager.Instance.character1.figherIndex, false);
        
        currentMainCharacterIndex = characterIndex;

        GameManager.Instance.character1 = characters[characterIndex].GetComponent<PlayerFighter>();

        fightersDateBase.SetMainCharacter(GameManager.Instance.character1.figherIndex, true);

        updateMainCharacterUI();
    }

    public void SwitchSecondaryCharacter(int characterIndex, bool isFirstTime)
    {
        if(characterIndex == currentMainCharacterIndex)
        {
            return;
        }

        fightersDateBase.SetSecondaryCharacter(GameManager.Instance.character2.figherIndex, false);

        currentSecondaryCharacterIndex = characterIndex;

        GameManager.Instance.character2 = characters[characterIndex].GetComponent<PlayerFighter>();

        fightersDateBase.SetSecondaryCharacter(GameManager.Instance.character2.figherIndex, true);

        updateSecondaryCharacterUI();
    }

    private void SetIndex()
    {
        for (int i = 0; i < fightersDateBase.EnemyDB.Count; i++)
        {
            if(fightersDateBase.EnemyDB[i].isMainCharacter)
            {
                currentMainCharacterIndex = fightersDateBase.EnemyDB[i].CharacterSwitcherIndex;
                Debug.Log("Main Character es " + fightersDateBase.EnemyDB[i].Name);
            }
        }

        for (int i = 0; i < fightersDateBase.EnemyDB.Count; i++)
        {
            if(fightersDateBase.EnemyDB[i].isSecondaryCharacter)
            {
                currentSecondaryCharacterIndex = fightersDateBase.EnemyDB[i].CharacterSwitcherIndex;
                Debug.Log("Secondary Character es " + fightersDateBase.EnemyDB[i].Name);
            }
        }
    }
}
