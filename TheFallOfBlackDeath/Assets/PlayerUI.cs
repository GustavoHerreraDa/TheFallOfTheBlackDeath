using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerUI : MonoBehaviour
{
    // Start is called before the first frame update
    public Fighter fighter;
    public TextMeshProUGUI nameHero;
    public TextMeshProUGUI currentHealth;
    public TextMeshProUGUI maxHealth;
    public TextMeshProUGUI attack;
    public TextMeshProUGUI defense;
    public TextMeshProUGUI speed;
    public void Start()
    {
        if (fighter == null)
            return;

        nameHero.text = fighter.idName;
        currentHealth.text = "HP: " + fighter.GetCurrentStats().health.ToString();
        maxHealth.text = fighter.GetCurrentStats().maxHealth.ToString();
        attack.text = "Attack: " + fighter.GetCurrentStats().attack.ToString();
        defense.text = "Deffense: " + fighter.GetCurrentStats().deffense.ToString();
        speed.text = "Speed: " + fighter.GetCurrentStats().speed.ToString();
    }

    // Update is called once per frame
    void Update()
    {

    }
}
