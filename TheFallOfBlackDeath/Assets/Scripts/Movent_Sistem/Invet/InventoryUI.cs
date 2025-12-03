using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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

    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
        character1 = GameManager.Instance.character1;
        character2 = GameManager.Instance.character2;

        originalColor = buttonSprite.color;
    }

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
