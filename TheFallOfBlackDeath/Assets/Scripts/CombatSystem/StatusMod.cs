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
    private StatsManager statsManager;
    public Fighter fighter;

    public Stats Apply(Stats stats)
    {
        Stats modedStats = stats.Clone();
        
        

        switch (this.type)
        {
            case StatusModType.ATTACK_MOD:
                modedStats.attack += this.amount;
                if (modedStats.attack <= 25)
                {
                    modedStats.attack = 25;
                    
                    LogPanel.Write("You can't lower the attack any further. " + modedStats.attack);
                    
                }
                if (modedStats.attack >= 80)
                {
                    modedStats.attack = 80;
                    LogPanel.Write("You can't raise the attack any higher. " + modedStats.attack);
                }
                
                break;

            case StatusModType.DEFFENSE_MOD:
                modedStats.deffense += this.amount;
                if (modedStats.deffense <= 20)
                {
                    modedStats.deffense = 20;
                    LogPanel.Write("You can't lower the defense any further. " + modedStats.deffense);
                }
                if (modedStats.deffense >= 80)
                {
                    modedStats.deffense = 80;
                    LogPanel.Write("The defense cannot be lowered further" + modedStats.deffense);
                }
                
                break;

        }

        return modedStats;
    }
}