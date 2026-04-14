using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Supports inventory and interaction flow by handling inventory ui.
/// </summary>
public class InventoryUI : MonoBehaviour
{
    public PlayerFighter character1;
    public PlayerFighter character2;
    public TMP_Text amount;
    public TMP_Text itemName;
    public TMP_Text itemDescripcion;
    public Image sprite;

    public Image buttonSprite;

    public string statAffected;
    public float amountAffected;

    private bool _isCharacter1Equipped;
    private bool _isCharacter2Equipped;

    public AudioSource audioSource;
    public AudioClip equipSfx;
    public AudioClip unequipSfx;

    private Color originalColor;
    public Color equippedColor = Color.green;
    public Color unequippedColor = Color.red;

    /// <summary>
    /// Initializes cached references and runtime state before the component starts running.
    /// </summary>
    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
        if (GameManager.Instance != null)
        {
            character1 = GameManager.Instance.character1;
            character2 = GameManager.Instance.character2;
        }
        originalColor = buttonSprite != null ? buttonSprite.color : Color.white;
    }

    /// <summary>
    /// Registers runtime listeners when the component becomes active.
    /// </summary>
    private void OnEnable()
    {
        InventoryManager.OnCharacterChanged += OnCharacterChanged;
    }

    /// <summary>
    /// Unregisters runtime listeners when the component becomes inactive.
    /// </summary>
    private void OnDisable()
    {
        InventoryManager.OnCharacterChanged -= OnCharacterChanged;
    }

    /// <summary>
    /// Executes the on character changed workflow.
    /// </summary>
    /// <param name="fighter">The fighter.</param>
    private void OnCharacterChanged(PlayerFighter fighter)
    {
        if (GameManager.Instance != null)
        {
            character1 = GameManager.Instance.character1;
            character2 = GameManager.Instance.character2;
        }
    }

    /// <summary>
    /// Executes the character1 btn workflow.
    /// </summary>
    public void Character1BTN()
    {
        
        if (_isCharacter2Equipped)
        {
            character2.UpdateStats(statAffected, -amountAffected);
            _isCharacter2Equipped = false;

            audioSource.PlayOneShot(unequipSfx);
            GameManager.Instance.SavePlayerState(character2);

            buttonSprite.color = originalColor;
        }

        
        if (!_isCharacter1Equipped)
        {

            character1.UpdateStats(statAffected, amountAffected);
            _isCharacter1Equipped = true;
            GameManager.Instance.SavePlayerState(character1);
            audioSource.PlayOneShot(equipSfx);

            buttonSprite.color = equippedColor;
            Debug.Log("Equipamos al character 1");
        }
    }


    /// <summary>
    /// Executes the character2 btn workflow.
    /// </summary>
    public void Character2BTN()
    {
        if (_isCharacter1Equipped)
        {
            character1.UpdateStats(statAffected, -amountAffected);
            _isCharacter1Equipped = false;

            audioSource.PlayOneShot(unequipSfx);
            GameManager.Instance.SavePlayerState(character1);

            buttonSprite.color = originalColor; // Normal
        }

        if (!_isCharacter2Equipped)
        {
            character2.UpdateStats(statAffected, amountAffected);
            _isCharacter2Equipped = true;

            audioSource.PlayOneShot(equipSfx);
            GameManager.Instance.SavePlayerState(character1);

            buttonSprite.color = equippedColor; // Verde
            Debug.Log("Equipamos al character 2");
        }
    }

}
