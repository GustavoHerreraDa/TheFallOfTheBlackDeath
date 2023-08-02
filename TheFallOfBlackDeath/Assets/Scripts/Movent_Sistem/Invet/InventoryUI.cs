using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryUI : MonoBehaviour
{
    public Fighter badDoctor;
    public Fighter assassin;
    public TMP_Text amount;
    public TMP_Text itemName;
    public TMP_Text itemDescripcion;
    public Image sprite;

    public string statAffected;
    public float amountAffected;

    private bool _isDoctorEquipped;
    private bool _isAssassinEquipped;

    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
        badDoctor = GameManager.Instance.badDoctor;
        assassin = GameManager.Instance.assassin;
    }

    public void BadDoctorBTN()
    {
        if(_isAssassinEquipped)
        {
            assassin.UpdateStats(statAffected, -amountAffected);
            _isAssassinEquipped = false;
        }

        if(!_isDoctorEquipped)
        {
            badDoctor.UpdateStats(statAffected, amountAffected);
            _isDoctorEquipped = true;
            Debug.Log("Equipamos al doctor");
        } 
        //Mas UI
    }

    public void AssassinBTN()
    {
        if(_isDoctorEquipped)
        {
            badDoctor.UpdateStats(statAffected, -amountAffected);
            _isDoctorEquipped = false;
        } 

        if(!_isAssassinEquipped)
        {
            assassin.UpdateStats(statAffected, amountAffected);
            _isAssassinEquipped = true;
            Debug.Log("Equipamos al asesino");
        }
        //Mas UI
    }
}
