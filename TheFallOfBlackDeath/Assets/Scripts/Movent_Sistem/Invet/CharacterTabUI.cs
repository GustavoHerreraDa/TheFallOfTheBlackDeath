using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Supports inventory and interaction flow by handling character tab ui.
/// </summary>
public class CharacterTabUI : MonoBehaviour
{

    [SerializeField] private CharacterSwitcher _characterSwitcher;

    /// <summary>
    /// Executes the main character btn workflow.
    /// </summary>
    /// <param name="characterIndex">The character index.</param>
    public void MainCharacterBTN(int characterIndex)
    {

        _characterSwitcher.SwitchMainCharacter(characterIndex, false);
        //_boddyStatus.Refresh();

    }

    /// <summary>
    /// Executes the secondary character btn workflow.
    /// </summary>
    /// <param name="characterIndex">The character index.</param>
    public void SecondaryCharacterBTN(int characterIndex)
    {
        _characterSwitcher.SwitchSecondaryCharacter(characterIndex, false);
    }
}
