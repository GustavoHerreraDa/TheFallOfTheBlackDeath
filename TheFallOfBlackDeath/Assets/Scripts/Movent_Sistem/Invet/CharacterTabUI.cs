// Cambios: Se reemplazó CharacterSwitcher por PartyManager y se deprecaron los métodos de switch de líder.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Supports inventory and interaction flow by handling character tab ui.
/// </summary>
public class CharacterTabUI : MonoBehaviour
{

    [SerializeField] private PartyManager _partyManager;

    /// <summary>
    /// Executes the main character btn workflow.
    /// </summary>
    /// <param name="characterIndex">The character index.</param>
    public void MainCharacterBTN(int characterIndex)
    {
        Debug.Log("Funcionalidad de Switch Main Character deprecada.");
    }

    /// <summary>
    /// Executes the secondary character btn workflow.
    /// </summary>
    /// <param name="characterIndex">The character index.</param>
    public void SecondaryCharacterBTN(int characterIndex)
    {
        Debug.Log("Funcionalidad de Switch Secondary Character deprecada.");
    }
}
