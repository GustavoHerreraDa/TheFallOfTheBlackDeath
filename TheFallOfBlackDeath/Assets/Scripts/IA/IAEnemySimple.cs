using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


public enum EnemyStateSimple
{
    Attack,
    UseAbility,
    Heal,
}
public class IAEnemySimple : MonoBehaviour
{
    private EnemyStateSimple currentState;
    private Skill lastSkill;
    [SerializeField]
    private EnemyFighter Enemy;
    [SerializeField]
    private int MaxPhisicalAttacks;

    private int phisicalAttacks;
    public List<Skill> _skills;
    // Use this for initialization
    void Start()
    {
        if (MaxPhisicalAttacks == 0) MaxPhisicalAttacks = 2;
        Enemy = gameObject.GetComponent<EnemyFighter>();
    }

    // Update is called once per frame
    public Skill ExecuteState()
    {
        Skill execute_Skill = null;
        switch (currentState)
        {
            case EnemyStateSimple.Attack:
                execute_Skill = AttackState();
                // Comprobar las condiciones de transición
                if (phisicalAttacks > MaxPhisicalAttacks)
                {
                    phisicalAttacks = 0;
                    currentState = EnemyStateSimple.UseAbility;
                    execute_Skill = UseAbilityState();
                }
                else if (Enemy.GetCurrentStats().health * 100 / Enemy.GetCurrentStats().health < 50 && lastSkill.skillType != SkillType.Heal)
                {
                    currentState = EnemyStateSimple.Heal;
                    execute_Skill = HealState();
                }
                break;

            case EnemyStateSimple.UseAbility:
                UseAbilityState();
                // Comprobar las condiciones de transición
                if (lastSkill.skillType == SkillType.SpecialHability)
                {
                    currentState = EnemyStateSimple.Attack;
                    execute_Skill = AttackState();
                }
                if (Enemy.GetCurrentStats().health * 100 / Enemy.GetCurrentStats().health < 50 && lastSkill.skillType != SkillType.Heal)
                {
                    currentState = EnemyStateSimple.Heal;
                    execute_Skill = HealState();
                }
                break;
            default:
                break;
        }

        Debug.Log("_IAEnemySimple Skill " + currentState.ToString());

        lastSkill = execute_Skill;

        return execute_Skill;
    }

    private Skill AttackState()
    {
        phisicalAttacks += 1;
        var attackSkill = _skills.Where(x => x.skillType == SkillType.AttackSimple).FirstOrDefault();

        return attackSkill = attackSkill ? attackSkill : _skills[0];
    }

    private Skill UseAbilityState()
    {
        var specialHabilities = _skills.Where(x => x.skillType == SkillType.SpecialHability).ToList();

        Skill specialSkill = specialHabilities[Random.Range(0, specialHabilities.Count)];

        return specialSkill = specialSkill ? specialSkill : _skills[0];
    }

    private Skill HealState()
    {
        var healSkill = _skills.Where(x => x.skillType == SkillType.Heal).FirstOrDefault();

        return healSkill = healSkill ? healSkill : _skills[0];
    }

    public void SetSkills(Skill[] skills)
    {
        List<Skill> lista = new List<Skill>(skills);
        _skills = lista;
    }
}
