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

    private bool CanUseSkill(Skill skill)
    {
        if (skill == null || Enemy == null)
            return false;

        // Si no requiere partes específicas, puede usarla
        if (skill.requiredParts == null || skill.requiredParts.Count == 0)
            return true;

        // Si alguna de las partes requeridas está destruida, no puede usarla
        foreach (var part in skill.requiredParts)
        {
            var bodyPart = Enemy.GetBodyPart(part);
            if (bodyPart == null || bodyPart.IsDestroyed)
            {
                Debug.Log($"{Enemy.idName} no puede usar {skill.skillName} porque tiene {part} destruido.");
                return false;
            }
        }

        return true;
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
        var attackSkills = _skills.Where(x => x.skillType == SkillType.AttackSimple && CanUseSkill(x)).ToList();

        if (attackSkills.Count == 0)
        {
            Debug.LogWarning($"{Enemy.idName} no tiene ataques físicos utilizables por daño corporal.");
            return _skills.FirstOrDefault(s => CanUseSkill(s)); // busca otra habilidad posible
        }

        return attackSkills[Random.Range(0, attackSkills.Count)];
    }
    private Skill UseAbilityState()
    {
        var specialSkills = _skills.Where(x => x.skillType == SkillType.SpecialHability && CanUseSkill(x)).ToList();

        if (specialSkills.Count == 0)
        {
            Debug.LogWarning($"{Enemy.idName} no puede usar habilidades especiales ahora.");
            return AttackState(); // vuelve a atacar si no puede usar habilidades
        }

        return specialSkills[Random.Range(0, specialSkills.Count)];
    }
    private Skill HealState()
    {
        var healSkills = _skills.Where(x => x.skillType == SkillType.Heal && CanUseSkill(x)).ToList();

        if (healSkills.Count == 0)
        {
            Debug.LogWarning($"{Enemy.idName} no puede usar curaciones por daño corporal.");
            return AttackState();
        }

        return healSkills[Random.Range(0, healSkills.Count)];
    }
    public void SetSkills(Skill[] skills)
    {
        List<Skill> lista = new List<Skill>(skills);
        _skills = lista;
    }
}
