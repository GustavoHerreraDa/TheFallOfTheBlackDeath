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
                if (modedStats.attack <= 20) modedStats.attack = 20;
                if (modedStats.attack >= 80) modedStats.attack = 80;
                break;

            case StatusModType.DEFFENSE_MOD:
                modedStats.deffense += this.amount;
                if (modedStats.deffense <= 20) modedStats.deffense = 20;
                if (modedStats.deffense >= 80) modedStats.deffense = 80;
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