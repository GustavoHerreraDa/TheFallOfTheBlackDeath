using System.Net.Mime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class CharactersUI : MonoBehaviour
{
    public EnemyDataBase fightersDateBase;

    [Header("Main Character")]
    [SerializeField] private Image _mainCharacterImage;
    [SerializeField] private TextMeshProUGUI _mainCharacterName;

    [Header("Secondary Character")]
    [SerializeField] private Image _secondaryCharacterImage;
    [SerializeField] private TextMeshProUGUI _secondaryCharacterName;

    void Start()
    {
        UpdateMainCharacterUI();
        UpdateSecondaryCharacterUI();

        CharacterSwitcher.updateMainCharacterUI += UpdateMainCharacterUI;
        CharacterSwitcher.updateSecondaryCharacterUI += UpdateSecondaryCharacterUI;
    }

    private void UpdateMainCharacterUI()
    {
        _mainCharacterName.text = fightersDateBase.EnemyDB[GameManager.Instance.character1.figherIndex].Name;
        _mainCharacterImage.sprite = fightersDateBase.EnemyDB[GameManager.Instance.character1.figherIndex].characterImage;
    }

    private void UpdateSecondaryCharacterUI()
    {
        _secondaryCharacterName.text = fightersDateBase.EnemyDB[GameManager.Instance.character2.figherIndex].Name;
        _secondaryCharacterImage.sprite = fightersDateBase.EnemyDB[GameManager.Instance.character2.figherIndex].characterImage;
    }

    void OnDisable()
    {
        CharacterSwitcher.updateMainCharacterUI -= UpdateMainCharacterUI;
        CharacterSwitcher.updateSecondaryCharacterUI -= UpdateSecondaryCharacterUI;
    }
}
