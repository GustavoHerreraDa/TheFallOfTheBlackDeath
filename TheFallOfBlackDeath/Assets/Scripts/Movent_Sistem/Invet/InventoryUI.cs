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

    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
        character1 = GameManager.Instance.character1;
        character2 = GameManager.Instance.character2;
    }

    public void Character1BTN()
    {
        if(_isCharacter2Equipped)
        {
            character2.UpdateStats(statAffected, -amountAffected);
            _isCharacter2Equipped = false;
        }

        if(!_isCharacter1Equipped)
        {
            character1.UpdateStats(statAffected, amountAffected);
            _isCharacter1Equipped = true;
            Debug.Log("Equipamos al character 1");
        } 
        //Mas UI
    }

    public void Character2BTN()
    {
        if(_isCharacter1Equipped)
        {
            character1.UpdateStats(statAffected, -amountAffected);
            _isCharacter1Equipped = false;
        } 

        if(!_isCharacter2Equipped)
        {
            character2.UpdateStats(statAffected, amountAffected);
            _isCharacter2Equipped = true;
            Debug.Log("Equipamos al character 2");
        }
        //Mas UI
    }
}
