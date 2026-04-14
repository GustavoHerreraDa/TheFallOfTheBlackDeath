//TP2 GUSTAVO TORRES
using UnityEngine;
/// <summary>
/// Supports the combat system by handling stats.
/// </summary>
public class Stats
{
    public float health;
    public float maxHealth;

    public int level;
    public float attack;
    public float deffense;
    public float spirit;
    public float speed;
    public int experience;
    public int experienceToNextLevel = 100;
    /// <summary>
    /// Initializes a new instance of the <see cref="Stats"/> class.
    /// </summary>
    /// <param name="_level">The level.</param>
    /// <param name="_maxhealth">The maxhealth.</param>
    /// <param name="_health">The health.</param>
    /// <param name="_attack">The attack.</param>
    /// <param name="_deffense">The deffense.</param>
    /// <param name="_spirit">The spirit.</param>
    /// <param name="_speed">The speed.</param>
    /// <param name="_exp">The exp.</param>
    /// <param name="_expNext">The exp next.</param>
    public Stats(int _level, float _maxhealth, float _health, float _attack, float _deffense, float _spirit, float _speed, int _exp = 0, int _expNext = 100)
    {
        level = _level;

        maxHealth = _maxhealth;
        health = _health;

        attack = _attack;
        deffense = _deffense;
        spirit = _spirit;
        speed = _speed;

        experience = _exp;
        experienceToNextLevel = _expNext;
        
    }
    
    

    /// <summary>
    /// Executes the clone workflow.
    /// </summary>
    /// <returns>The resulting value.</returns>
    public Stats Clone()
    {
        return new Stats(level, maxHealth, health, attack, deffense, spirit, speed, experience, experienceToNextLevel);
    }
}
