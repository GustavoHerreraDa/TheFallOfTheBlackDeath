using UnityEngine;
//TP2 GUSTAVO TORRES/FACUNDO FERREIRO

public enum StatusModType
{
    ATTACK_MOD,
    DEFFENSE_MOD,
    SPEED_MOD // <-- ¡NUEVO! Agregamos velocidad
}

public class StatusMod : MonoBehaviour
{
    public StatusModType type;
    public float amount;

    public Stats Apply(Stats stats)
    {
        Stats modedStats = stats.Clone();

        switch (this.type)
        {
            case StatusModType.ATTACK_MOD:
                modedStats.attack += this.amount;
                if (modedStats.attack <= 1) modedStats.attack = 1;
                if (modedStats.attack >= 100) modedStats.attack = 100;
                break;

            case StatusModType.DEFFENSE_MOD:
                modedStats.deffense += this.amount;
                if (modedStats.deffense <= 1) modedStats.deffense = 1;
                if (modedStats.deffense >= 40) modedStats.deffense = 40;
                break;

            // <-- ¡NUEVO! Lógica para afectar la velocidad
            case StatusModType.SPEED_MOD:
                modedStats.speed += this.amount;
                if (modedStats.speed <= 5) modedStats.speed = 5; // Límite inferior de velocidad
                break;
        }

        return modedStats;
    }
}