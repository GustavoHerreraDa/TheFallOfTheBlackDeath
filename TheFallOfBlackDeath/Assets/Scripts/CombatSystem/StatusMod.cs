using UnityEngine;
//TP2 GUSTAVO TORRES/FACUNDO FERREIRO

/// <summary>
/// Defines the named values used by status mod type.
/// </summary>
public enum StatusModType
{
    ATTACK_MOD,
    DEFFENSE_MOD,
    SPEED_MOD // <-- Â¡NUEVO! Agregamos velocidad
}

/// <summary>
/// Supports the combat system by handling status mod.
/// </summary>
public class StatusMod : MonoBehaviour
{
    public StatusModType type;
    public float amount;

    /// <summary>
    /// Applies the value.
    /// </summary>
    /// <param name="stats">The stats.</param>
    /// <returns>The resulting value.</returns>
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

            // <-- Â¡NUEVO! LÃ³gica para afectar la velocidad
            case StatusModType.SPEED_MOD:
                modedStats.speed += this.amount;
                if (modedStats.speed <= 5) modedStats.speed = 5; // LÃ­mite inferior de velocidad
                break;
        }

        return modedStats;
    }
}
