using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterTabUI : MonoBehaviour
{
    [SerializeField] private CharacterSwitcher _characterSwitcher;

    public void MainCharacterBTN(int characterIndex)
    {
        _characterSwitcher.SwitchMainCharacter(characterIndex);
    }

    public void SecondaryCharacterBTN(int characterIndex)
    {
        _characterSwitcher.SwitchSecondaryCharacter(characterIndex);
    }
}
