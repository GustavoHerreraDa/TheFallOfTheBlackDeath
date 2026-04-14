using System.Net.Mime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

/// <summary>
/// Handles characters ui for the current project workflow.
/// </summary>
public class CharactersUI : MonoBehaviour
{
    public globalDataBase fightersDateBase;

    [Header("Main Character")]
    [SerializeField] private Image _mainCharacterImage;
    [SerializeField] private TextMeshProUGUI _mainCharacterName;

    [Header("Secondary Character")]
    [SerializeField] private Image _secondaryCharacterImage;
    [SerializeField] private TextMeshProUGUI _secondaryCharacterName;

    /// <summary>
    /// Initializes the component once the scene dependencies are ready.
    /// </summary>
    void Start()
    {
        UpdateMainCharacterUI();
        UpdateSecondaryCharacterUI();

        CharacterSwitcher.updateMainCharacterUI += UpdateMainCharacterUI;
        CharacterSwitcher.updateSecondaryCharacterUI += UpdateSecondaryCharacterUI;
    }

    /// <summary>
    /// Updates the main character ui.
    /// </summary>
    private void UpdateMainCharacterUI()
    {
        _mainCharacterName.text = fightersDateBase.EnemyDB[GameManager.Instance.character1.figherIndex].Name;
        _mainCharacterImage.sprite = fightersDateBase.EnemyDB[GameManager.Instance.character1.figherIndex].characterImage;
    }

    /// <summary>
    /// Updates the secondary character ui.
    /// </summary>
    private void UpdateSecondaryCharacterUI()
    {
        _secondaryCharacterName.text = fightersDateBase.EnemyDB[GameManager.Instance.character2.figherIndex].Name;
        _secondaryCharacterImage.sprite = fightersDateBase.EnemyDB[GameManager.Instance.character2.figherIndex].characterImage;
    }

    /// <summary>
    /// Unregisters runtime listeners when the component becomes inactive.
    /// </summary>
    void OnDisable()
    {
        CharacterSwitcher.updateMainCharacterUI -= UpdateMainCharacterUI;
        CharacterSwitcher.updateSecondaryCharacterUI -= UpdateSecondaryCharacterUI;
    }
}
