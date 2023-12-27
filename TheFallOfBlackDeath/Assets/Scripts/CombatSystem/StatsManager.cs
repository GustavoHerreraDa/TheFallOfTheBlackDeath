using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
//TP2 GUSTAVO TORRES/FACUNDO FERREIRO
public class StatsManager : MonoBehaviour
{
    public Fighter fighter;

    public TextMeshProUGUI actualDefense;
    public TextMeshProUGUI actualAttack;

    private void Start()
    {
        if(fighter.team == Team.PLAYERS)
        {
            GameObject[] statPanels = GameObject.FindGameObjectsWithTag("StatPanel");

            foreach(GameObject statPanel in statPanels)
            {
                StatusPanel statusPanel = statPanel.GetComponent<StatusPanel>();

                if(statusPanel.nameLabel.text == fighter.idName)
                {
                    actualDefense = statusPanel.actualDefense;
                    actualAttack = statusPanel.actualAttack;
                    return;
                }
                else
                {
                    continue;
                }      
            }
        }
    

        this.SetDefense(fighter.GetCurrentStats().deffense, true);
        this.SetAttack(fighter.GetCurrentStats().attack, true);
    }
    void Update()
    {
        //UpdateUI();
        //Debug.Log("Figther" + fighter.stats.attack);
    }

    public void UpdateUI()
    {
        this.SetDefense(fighter.GetCurrentStats().deffense, false);
        this.SetAttack(fighter.GetCurrentStats().attack, false);
        Debug.Log("se recibio las estadisticas de estos player" + fighter);
    }

    public void SetDefense(float deffense, bool firstCheck)
    {
        if (actualDefense == null)
            return;

        int comparison = int.Parse(actualDefense.text);

        actualDefense.text = deffense.ToString();

        if(!firstCheck)
        {        
            if(deffense > comparison)
            {
                if (deffense >= 80)
                {
                    actualDefense.color = Color.yellow;
                }
                else
                {
                    actualDefense.color = Color.green;
                }
            }

            if(deffense < comparison)
            {
                if (deffense <= 20)
                {
                    Color bordeaux = new Color(0.6f, 0, 0.1f, 1);
                    actualDefense.color = bordeaux;
                }
                else
                {
                    actualDefense.color = Color.red;
                }
            } 
        }
    }
    public void SetAttack(float attack, bool firstCheck)
    {
        if (actualAttack == null)
            return;

        int comparison = int.Parse(actualAttack.text);

        actualAttack.text = attack.ToString();

        if(!firstCheck)
        {  
            if(attack > comparison)
            {
                if(attack >= 80)
                {
                    actualAttack.color = Color.yellow;
                }
                else
                {
                    actualAttack.color = Color.green;
                }
            }

            if(attack < comparison)
            {
                if (attack <= 20)
                {
                    Color bordeaux = new Color(0.6f, 0, 0.1f, 1);
                    actualAttack.color = bordeaux;
                }
                else
                {
                    actualAttack.color = Color.red;
                }
            }
        }
    }
}
