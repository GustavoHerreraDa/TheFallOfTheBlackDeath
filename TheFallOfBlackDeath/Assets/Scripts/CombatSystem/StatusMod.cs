using UnityEngine;

public enum StatusModType
{
    ATTACK_MOD,
    DEFFENSE_MOD
}

public class StatusMod : MonoBehaviour
{
    public StatusModType type;
    public float amount;

    


    public Stats Apply(Stats stats)
    {
        Stats modedStats = stats.Clone();
        if (modedStats.attack > 0)
        {
            
        }
        
        

        switch (this.type)
        {
            case StatusModType.ATTACK_MOD:
                modedStats.attack += this.amount;
                if (modedStats.attack <= 20)
                {
                    modedStats.attack = 20;

                }
                if (modedStats.attack >= 80)
                {
                    modedStats.attack = 80;

                }
                
                break;

            case StatusModType.DEFFENSE_MOD:
                modedStats.deffense += this.amount;
                if (modedStats.deffense <= 20)
                {
                    modedStats.deffense = 20;
                    

                }
                if (modedStats.deffense >= 80)
                {
                    modedStats.deffense = 80;

                }
                
                break;

        }

        return modedStats;
    }


}