using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//TP2 GUSTAVO TORRES
/// <summary>
/// Supports the combat system by handling roulette skill.
/// </summary>
public class RouletteSkill : MonoBehaviour
{
    Dictionary<Skill, int> _dic = new Dictionary<Skill, int>();

    delegate void _actionDelegate();
    public Skill attack;
    public Skill speedUp;
    public Skill shield;
    public Skill electricity;


    /// <summary>
    /// Initializes the component once the scene dependencies are ready.
    /// </summary>
    private void Start()
    {
        _dic.Add(attack, 50);
        _dic.Add(speedUp, 20);
        _dic.Add(shield, 20);
        _dic.Add(electricity, 10);
    }

    /// <summary>
    /// Executes the value workflow.
    /// </summary>
    /// <returns>The resulting value.</returns>
    public Skill Execute()
    {
        int totalWeight = 0;
        foreach (var item in _dic)
        {
            totalWeight += item.Value;
        }
        int random = Random.Range(0, totalWeight);
        foreach (var item in _dic)
        {
            random -= item.Value;
            if (random < 0)
            {
                return item.Key;
            }
        }
        return speedUp;
    }
}
