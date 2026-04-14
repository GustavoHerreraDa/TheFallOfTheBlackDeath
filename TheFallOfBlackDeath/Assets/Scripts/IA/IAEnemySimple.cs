using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
//ferreiro
/// <summary>
/// Defines the named values used by enemy state simple.
/// </summary>
public enum EnemyStateSimple
{
    Attack,
    UseAbility,
    Heal,
}

/// <summary>
/// Defines the named values used by ai attack preference.
/// </summary>
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

/// <summary>
/// Chooses enemy combat skills and target body parts using a lightweight state-based decision strategy.
/// </summary>
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

    /// <summary>
    /// Initializes the component once the scene dependencies are ready.
    /// </summary>
    void Start()
    {
        if (MaxPhisicalAttacks == 0) MaxPhisicalAttacks = 2;
        Enemy = gameObject.GetComponent<EnemyFighter>();
    }

    /// <summary>
    /// Determines whether the component can use skill.
    /// </summary>
    /// <param name="skill">The skill.</param>
    /// <returns>True when the requested condition is met; otherwise, false.</returns>
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

    /// <summary>
    /// Executes the state workflow.
    /// </summary>
    /// <returns>The resulting value.</returns>
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
        /// <summary>
        /// Executes the if workflow.
        /// </summary>
        /// <param name="0.5f">The 0.5f.</param>
        /// <returns>The resulting value.</returns>
                else if (Enemy.GetCurrentStats().health < Enemy.GetCurrentStats().maxHealth * 0.5f)
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

    /// <summary>
    /// Executes the attack state workflow.
    /// </summary>
    /// <returns>The resulting value.</returns>
    private Skill AttackState()
    {
        phisicalAttacks += 1;
        var attackSkills = _skills.Where(x => x.skillType == SkillType.AttackSimple && CanUseSkill(x)).ToList();

        if (attackSkills.Count == 0)
        {
            Debug.Log($"{Enemy.idName} no tiene ataques físicos utilizables.");
            return _skills.FirstOrDefault(s => CanUseSkill(s)); // busca otra habilidad posible
        }

        return attackSkills[Random.Range(0, attackSkills.Count)];
    }

    /// <summary>
    /// Executes the use ability state workflow.
    /// </summary>
    /// <returns>The resulting value.</returns>
    private Skill UseAbilityState()
    {
        var specialSkills = _skills.Where(x => x.skillType == SkillType.SpecialHability && CanUseSkill(x)).ToList();

        if (specialSkills.Count == 0)
        {
            Debug.Log($"{Enemy.idName} no puede usar habilidades especiales.");
            return AttackState(); // vuelve a atacar si no puede usar habilidades
        }

        return specialSkills[Random.Range(0, specialSkills.Count)];
    }

    /// <summary>
    /// Executes the heal state workflow.
    /// </summary>
    /// <returns>The resulting value.</returns>
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

    /// <summary>
    /// Sets the skills.
    /// </summary>
    /// <param name="skills">The skills.</param>
    public void SetSkills(Skill[] skills)
    {
        List<Skill> lista = new List<Skill>(skills);
        _skills = lista;
    }

    /// <summary>
    /// Executes the choose targetable body part workflow.
    /// </summary>
    /// <param name="target">The target.</param>
    /// <param name="pref">The pref.</param>
    /// <returns>The resulting value.</returns>
    public BodyPart ChooseTargetableBodyPart(Fighter target, AIAttackPreference pref)
    {
        if (target == null || target.bodyParts == null || target.bodyParts.Count == 0)
            return BodyPart.None;

        //lista de partes de partes que no estan destruidas
        var availableParts = target.bodyParts
            .Where(p => !p.IsDestroyed)      
            .ToList();

        if (availableParts.Count == 0)
            return BodyPart.None;

        //aggregate para encontrar la parte mas dañada
        Fighter.BodyPartData mostDamaged = null;

        // aggregate: devuelve la parte con menor ratio current/max
        mostDamaged = availableParts.Aggregate((best, next) =>
        {
            float bestRatio = best.currentHealth / best.maxHealth;
            float nextRatio = next.currentHealth / next.maxHealth;
            return nextRatio < bestRatio ? next : best;
        });

        // dependiendo de la preferencia devolvemos diferentes resultados
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

      
        // Priorizar la mostDamaged si existe si no elegir aleatorio.
        if (mostDamaged != null)
            return mostDamaged.part;

        return availableParts[Random.Range(0, availableParts.Count)].part;
    }
}
