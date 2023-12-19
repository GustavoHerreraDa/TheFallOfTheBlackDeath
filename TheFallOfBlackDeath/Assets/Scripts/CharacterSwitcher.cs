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
        SwitchSecondaryCharacter(currentSecondaryCharacterIndex);
    }

    public void SwitchMainCharacter(int characterIndex, bool isFirstTime)
    {
        //Si selecciona como principal el personaje que ya es principal, retornamos.
        if (currentMainCharacterIndex == characterIndex && !isFirstTime)
        {
            return;
        }

        //Si selecciona como principal el personaje que es secundario, return.
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

    public void SwitchSecondaryCharacter(int characterIndex)
    {
        //Si selecciona como secundario el personaje que ya es secundario, retornamos.
        if(currentSecondaryCharacterIndex == characterIndex)
        {
            return;
        } 

        //Si selecciona como secundario el personaje que es principal, return.
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
            }
        }

        for (int i = 0; i < fightersDateBase.EnemyDB.Count; i++)
        {
            if(fightersDateBase.EnemyDB[i].isSecondaryCharacter)
            {
                currentSecondaryCharacterIndex = fightersDateBase.EnemyDB[i].CharacterSwitcherIndex;
            }
        }
    }
}
