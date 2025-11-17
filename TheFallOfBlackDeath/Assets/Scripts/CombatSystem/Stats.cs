//TP2 GUSTAVO TORRES
using UnityEngine;
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
    public Stats(int _level, float _maxhealth, float _attack, float _deffense, float _spirit, float _speed, int _exp = 0, int _expNext = 100)
    {
        level = _level;

        maxHealth = _maxhealth;
        health = _maxhealth;

        attack = _attack;
        deffense = _deffense;
        spirit = _spirit;
        speed = _speed;

        experience = _exp;
        experienceToNextLevel = _expNext;
    }


    public Stats Clone()
    {
        return new Stats(level, maxHealth, attack, deffense, spirit, speed, experience, experienceToNextLevel);
    }
}