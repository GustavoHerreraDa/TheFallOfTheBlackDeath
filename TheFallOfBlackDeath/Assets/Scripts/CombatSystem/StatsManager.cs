using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class StatsManager : MonoBehaviour
{
    public Fighter fighter;
    public TextMeshProUGUI actualDefense;
    public TextMeshProUGUI actualAttack;

    void Update()
    {
        this.SetDefense(fighter.GetCurrentStats().deffense);
        this.SetAttack(fighter.GetCurrentStats().attack);
        Debug.Log("Figther" + fighter.stats.attack);
    }

    // Update is called once per frame
    public void SetDefense(float deffense)
    {
        actualDefense.text = deffense.ToString();

    }
    public void SetAttack(float attack)
    {
        actualAttack.text = attack.ToString();
    }
    
}
