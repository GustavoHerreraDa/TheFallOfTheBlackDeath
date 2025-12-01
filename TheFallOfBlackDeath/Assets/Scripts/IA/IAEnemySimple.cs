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

public enum AIAttackPreference
{
    HeadFocused,
    TorsoFocused,
    ArmsFocused,
    LegsFocused,
    Aggressive,
    Opportunist,
    Random
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

    void Start()
    {
        if (MaxPhisicalAttacks == 0) MaxPhisicalAttacks = 2;
        Enemy = gameObject.GetComponent<EnemyFighter>();
    }

    private bool CanUseSkill(Skill skill)
    {
        if (skill == null || Enemy == null)
            return false;

        if (skill.requiredParts == null || skill.requiredParts.Count == 0)
            return true;

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

    public Skill ExecuteState()
    {
        Skill execute_Skill = null;
        switch (currentState)
        {
            case EnemyStateSimple.Attack:
                execute_Skill = AttackState();

                if (phisicalAttacks > MaxPhisicalAttacks)
                {
                    phisicalAttacks = 0;
                    currentState = EnemyStateSimple.UseAbility;
                    execute_Skill = UseAbilityState();
                }
                else if (Enemy.GetCurrentStats().health * 100 / Enemy.GetCurrentStats().health < 50 && lastSkill != null && lastSkill.skillType != SkillType.Heal)
                {
                    currentState = EnemyStateSimple.Heal;
                    execute_Skill = HealState();
                }
                break;

            case EnemyStateSimple.UseAbility:
                execute_Skill = UseAbilityState();

                if (lastSkill != null && lastSkill.skillType == SkillType.SpecialHability)
                {
                    currentState = EnemyStateSimple.Attack;
                    execute_Skill = AttackState();
                }
                var stats = Enemy.GetCurrentStats();
                if (stats.health < stats.maxHealth * 0.5f && lastSkill != null && lastSkill.skillType != SkillType.Heal)
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
            Debug.Log($"{Enemy.idName} no tiene ataques físicos utilizables por daño corporal.");
            return _skills.FirstOrDefault(s => CanUseSkill(s)); // busca otra habilidad posible
        }

        return attackSkills[Random.Range(0, attackSkills.Count)];
    }

    private Skill UseAbilityState()
    {
        var specialSkills = _skills.Where(x => x.skillType == SkillType.SpecialHability && CanUseSkill(x)).ToList();

        if (specialSkills.Count == 0)
        {
            Debug.Log($"{Enemy.idName} no puede usar habilidades especiales ahora.");
            return AttackState(); // vuelve a atacar si no puede usar habilidades
        }

        return specialSkills[Random.Range(0, specialSkills.Count)];
    }

    private Skill HealState()
    {
        var healSkills = _skills.Where(x => x.skillType == SkillType.Heal && CanUseSkill(x)).ToList();

        if (healSkills.Count == 0)
        {
            Debug.Log($"{Enemy.idName} no puede usar curaciones .");
            return AttackState();
        }

        return healSkills[Random.Range(0, healSkills.Count)];
    }

    public void SetSkills(Skill[] skills)
    {
        List<Skill> lista = new List<Skill>(skills);
        _skills = lista;
    }

    public BodyPart ChooseTargetableBodyPart(Fighter target, AIAttackPreference pref)
    {
        if (target == null || target.bodyParts == null || target.bodyParts.Count == 0)
            return BodyPart.None;

        // construimos la lista de partes de partes que no estan destruidas
        var availableParts = target.bodyParts
            .Where(p => !p.IsDestroyed)          // Grupo 1: Where
            .ToList();                            // Grupo 3: ToList

        if (availableParts.Count == 0)
            return BodyPart.None;

        // usamos un aggregate para encontrar la parte mas dañada
        Fighter.BodyPartData mostDamaged = null;

        // aggregate: devuelve la parte con menor ratio current/max
        mostDamaged = availableParts.Aggregate((best, next) =>
        {
            float bestRatio = best.currentHealth / best.maxHealth;
            float nextRatio = next.currentHealth / next.maxHealth;
            return nextRatio < bestRatio ? next : best;
        });

        // Dependiendo de la preferencia devolvemos diferentes resultados
        switch (pref)
        {
            case AIAttackPreference.HeadFocused:
                {
                    var head = availableParts.FirstOrDefault(p => p.part == BodyPart.Head);
                    if (head != null) return BodyPart.Head;
                    break;
                }
            case AIAttackPreference.TorsoFocused:
                {
                    var torso = availableParts.FirstOrDefault(p => p.part == BodyPart.Torso);
                    if (torso != null) return BodyPart.Torso;
                    break;
                }
            case AIAttackPreference.ArmsFocused:
                {
                    var arms = availableParts.Where(p => p.part == BodyPart.LeftArm || p.part == BodyPart.RightArm).ToList();
                    if (arms.Count > 0) return arms[Random.Range(0, arms.Count)].part;
                    break;
                }
            case AIAttackPreference.LegsFocused:
                {
                    var legs = availableParts.Where(p => p.part == BodyPart.LeftLeg || p.part == BodyPart.RightLeg).ToList();
                    if (legs.Count > 0) return legs[Random.Range(0, legs.Count)].part;
                    break;
                }
            case AIAttackPreference.Aggressive:
                {
                    var aggressive = availableParts.Where(p => p.part == BodyPart.Torso || p.part == BodyPart.LeftArm || p.part == BodyPart.RightArm).ToList();
                    if (aggressive.Count > 0) return aggressive[Random.Range(0, aggressive.Count)].part;
                    break;
                }
            case AIAttackPreference.Opportunist:
                {
                    //prioriza la parte mas dañada
                    if (mostDamaged != null)
                        return mostDamaged.part;
                    break;
                }
            case AIAttackPreference.Random:
            default:
                {
                    // random entre disponibles
                    return availableParts[Random.Range(0, availableParts.Count)].part;
                }
        }

      
        // Priorizar la 'mostDamaged' si existe, si no elegir aleatorio.
        if (mostDamaged != null)
            return mostDamaged.part;

        return availableParts[Random.Range(0, availableParts.Count)].part;
    }
}
