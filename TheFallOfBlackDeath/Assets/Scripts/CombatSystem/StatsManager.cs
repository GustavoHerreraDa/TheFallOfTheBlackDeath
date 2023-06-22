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
        //Debug.Log("Figther" + fighter.stats.attack);
    }

    // Update is called once per frame
    public void SetDefense(float deffense)
    {
        if (actualDefense == null)
            return;

        actualDefense.text = deffense.ToString();
        if (deffense >= 80)
        {
            actualDefense.color = Color.yellow;
        }

        if (deffense <= 20)
        {
            actualDefense.color = Color.red;
        }
    }
    public void SetAttack(float attack)
    {
        if (actualAttack == null)
            return;

        actualAttack.text = attack.ToString();
        if (attack >= 80)
        {

            actualAttack.color = Color.yellow;

        }
        if (attack <= 20)
        {

            actualAttack.color = Color.red;

        }


    }
}
