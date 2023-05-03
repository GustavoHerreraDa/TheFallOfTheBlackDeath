using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stats
{
    //Salud
    public float health;
    //Salud Maxima
    public float maxHealth;

    public int level;
    public float attack;
    public float deffense;
    //Spirit = mana bug
    public float spirit;

    public Stats(int level, float maxHealth, float attack, float deffense, float spirit)
    {
        this.level = level;
        this.maxHealth = maxHealth;
        this.health = maxHealth;
        this.attack = attack;
        this.deffense = deffense;
        this.spirit = spirit;
    }
    public Stats Clone()
    {
        return new Stats(this.level, this.maxHealth, this.attack, this.deffense, this.spirit);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
