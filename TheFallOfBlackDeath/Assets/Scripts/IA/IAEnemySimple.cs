using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
//ferreiro

/// <summary>
/// Stores combat memory data for enemy AI decision-making across turns.
/// </summary>
public struct EnemyCombatMemory
{
    public int timesParried;
    public int consecutiveMisses;
    public BodyPart lastHitPart;
    public bool targetUsedHealLastTurn;
    public int turnCount;

    public void Reset()
    {
        timesParried = 0;
        consecutiveMisses = 0;
        lastHitPart = BodyPart.None;
        targetUsedHealLastTurn = false;
        turnCount = 0;
    }
}

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

    // PARTE 1 — Pending state transition
    private EnemyStateSimple nextState;
    private bool pendingStateChange;

    // PARTE 2 — Combat memory
    public EnemyCombatMemory Memory;

    // PARTE 3 — Phase tracking
    private int currentPhase = 1;
    public event System.Action<int> OnPhaseChanged;

    // PARTE 6 — Personality weights
    [SerializeField] [Range(0f, 1f)] public float aggression = 0.5f;
    [SerializeField] [Range(0f, 1f)] public float synergyFocus = 0f;

    /// <summary>
    /// Initializes the component once the scene dependencies are ready.
    /// </summary>
    void Start()
    {
        if (MaxPhisicalAttacks == 0) MaxPhisicalAttacks = 2;
        Enemy = gameObject.GetComponent<EnemyFighter>();
        Memory.Reset();
    }

    /// <summary>
    /// Records the result of the previous turn into combat memory.
    /// </summary>
    public void RecordTurnResult(bool wasParried, bool wasMiss, BodyPart hitPart, bool targetUsedHeal)
    {
        if (wasParried)
            Memory.timesParried++;

        if (wasMiss)
            Memory.consecutiveMisses++;
        else
            Memory.consecutiveMisses = 0;

        Memory.lastHitPart = hitPart;
        Memory.targetUsedHealLastTurn = targetUsedHeal;
        Memory.turnCount++;
    }

    /// <summary>
    /// Schedules a state transition for the next ExecuteState call.
    /// </summary>
    private void ScheduleTransition(EnemyStateSimple state)
    {
        nextState = state;
        pendingStateChange = true;
    }

    /// <summary>
    /// Evaluates and returns the current combat phase based on enemy HP ratio.
    /// </summary>
    private int EvaluatePhase()
    {
        if (Enemy == null) return 1;
        var s = Enemy.GetCurrentStats();
        float ratio = s.maxHealth > 0 ? s.health / s.maxHealth : 1f;

        if (ratio > 0.66f) return 1;
        if (ratio > 0.33f) return 2;
        return 3;
    }

    /// <summary>
    /// Returns the heal threshold based on aggression and current phase.
    /// </summary>
    private float GetHealThreshold()
    {
        float threshold = Mathf.Lerp(0.65f, 0.20f, aggression);
        if (currentPhase == 2)
            threshold += 0.10f;
        return threshold;
    }

    /// <summary>
    /// Returns the effective max physical attacks for the current phase.
    /// </summary>
    private int GetEffectiveMaxPhysicalAttacks()
    {
        if (currentPhase == 2)
            return Mathf.Max(1, MaxPhisicalAttacks / 2);
        return MaxPhisicalAttacks;
    }

    /// <summary>
    /// Determines whether the component can use skill.
    /// </summary>
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
                Debug.Log($"[IAEnemySimple] {Enemy.idName} no puede usar {skill.skillName} porque tiene {part} destruido.");
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Scores a skill for selection, applying contextual modifiers and variance.
    /// </summary>
    private float ScoreSkill(Skill skill, Fighter target)
    {
        float score = 1.0f;

        // +0.4f if skill can exploit an active status condition on the target
        if (target != null)
        {
            var conditions = target.GetCurrentBodyPartStatusConditions();
            if (conditions != null && conditions.Count > 0)
                score += 0.4f;
        }

        // +0.3f if consecutiveMisses >= 1 and this skill has lower missChance than lastSkill
        if (Memory.consecutiveMisses >= 1)
        {
            float currentMiss = (skill as HealthModSkill)?.missChance ?? 0f;
            float lastMiss = (lastSkill as HealthModSkill)?.missChance ?? 0f;
            if (currentMiss < lastMiss)
                score += 0.3f;
        }

        // +0.2f * synergyFocus if ApplySCSkill and target has parts without status
        if (skill is ApplySCSkill && target != null)
        {
            bool hasCleanParts = target.bodyParts.Any(p => !p.IsDestroyed && p.currentStatus == PartStatus.None);
            if (hasCleanParts)
                score += 0.2f * synergyFocus;
        }

        // -0.3f if same skill as last turn
        if (lastSkill != null && skill == lastSkill)
            score -= 0.3f;

        // -0.5f if LethalQTESkill and timesParried >= 1
        if (skill is LethalQTESkill && Memory.timesParried >= 1)
            score -= 0.5f;

        // Variance
        score *= Random.Range(0.75f, 1.25f);

        return score;
    }

    /// <summary>
    /// Selects the best skill from a list using scoring.
    /// </summary>
    private Skill SelectBestSkill(List<Skill> candidates, Fighter target)
    {
        if (candidates == null || candidates.Count == 0)
            return null;

        Skill best = null;
        float bestScore = float.MinValue;

        foreach (var skill in candidates)
        {
            float s = ScoreSkill(skill, target);
            if (s > bestScore)
            {
                bestScore = s;
                best = skill;
            }
        }

        return best;
    }

    /// <summary>
    /// Gets the current target from the opposing team.
    /// </summary>
    private Fighter GetTarget()
    {
        if (Enemy == null || Enemy.combatManager == null) return null;
        var opposing = Enemy.combatManager.GetOpposingTeam();
        if (opposing == null || opposing.Length == 0) return null;
        return opposing[0];
    }

    /// <summary>
    /// Executes the state workflow.
    /// </summary>
    public Skill ExecuteState()
    {
        // Apply pending state transition
        if (pendingStateChange)
        {
            currentState = nextState;
            pendingStateChange = false;
        }

        // Evaluate phase
        int newPhase = EvaluatePhase();
        if (newPhase != currentPhase)
        {
            currentPhase = newPhase;
            OnPhaseChanged?.Invoke(currentPhase);
        }

        Fighter target = GetTarget();
        var stats = Enemy.GetCurrentStats();
        float hpRatio = stats.maxHealth > 0 ? stats.health / stats.maxHealth : 1f;
        float healThreshold = GetHealThreshold();

        Skill execute_Skill = null;

        switch (currentState)
        {
            case EnemyStateSimple.Attack:
                execute_Skill = AttackState(target);

                if (phisicalAttacks > GetEffectiveMaxPhysicalAttacks())
                {
                    phisicalAttacks = 0;
                    ScheduleTransition(EnemyStateSimple.UseAbility);
                }
                else if (hpRatio < healThreshold)
                {
                    // Phase 3 + high aggression: never heal
                    if (!(currentPhase == 3 && aggression > 0.7f))
                        ScheduleTransition(EnemyStateSimple.Heal);
                }
                break;

            case EnemyStateSimple.UseAbility:
                execute_Skill = UseAbilityState(target);

                if (lastSkill != null && lastSkill.skillType == SkillType.SpecialHability)
                {
                    ScheduleTransition(EnemyStateSimple.Attack);
                }
                if (hpRatio < healThreshold && (lastSkill == null || lastSkill.skillType != SkillType.Heal))
                {
                    if (!(currentPhase == 3 && aggression > 0.7f))
                        ScheduleTransition(EnemyStateSimple.Heal);
                }
                break;

            case EnemyStateSimple.Heal:
                execute_Skill = HealState(target);
                ScheduleTransition(EnemyStateSimple.Attack);
                break;

            default:
                break;
        }

        Debug.Log($"[IAEnemySimple] Skill {currentState} phase={currentPhase}");

        lastSkill = execute_Skill;

        return execute_Skill;
    }

    /// <summary>
    /// Executes the attack state workflow.
    /// </summary>
    private Skill AttackState(Fighter target)
    {
        phisicalAttacks += 1;

        // Phase 3 desperate mode: prioritize best heal skill if available and not high aggression
        if (currentPhase == 3 && aggression <= 0.7f)
        {
            var healSkills = _skills.Where(x => x.skillType == SkillType.Heal && CanUseSkill(x)).ToList();
            if (healSkills.Count > 0)
            {
                // Pick the HealthModSkill with highest amount
                var bestHeal = healSkills
                    .OrderByDescending(s => (s as HealthModSkill)?.amount ?? 0f)
                    .First();
                return bestHeal;
            }
        }

        var attackSkills = _skills.Where(x => x.skillType == SkillType.AttackSimple && CanUseSkill(x)).ToList();

        if (attackSkills.Count == 0)
        {
            Debug.Log($"[IAEnemySimple] {Enemy.idName} no tiene ataques físicos utilizables.");
            return _skills.FirstOrDefault(s => CanUseSkill(s));
        }

        return SelectBestSkill(attackSkills, target);
    }

    /// <summary>
    /// Executes the use ability state workflow.
    /// </summary>
    private Skill UseAbilityState(Fighter target)
    {
        // Phase 3 desperate mode: prioritize best heal skill if available and not high aggression
        if (currentPhase == 3 && aggression <= 0.7f)
        {
            var healSkills = _skills.Where(x => x.skillType == SkillType.Heal && CanUseSkill(x)).ToList();
            if (healSkills.Count > 0)
            {
                var bestHeal = healSkills
                    .OrderByDescending(s => (s as HealthModSkill)?.amount ?? 0f)
                    .First();
                return bestHeal;
            }
        }

        var specialSkills = _skills.Where(x => x.skillType == SkillType.SpecialHability && CanUseSkill(x)).ToList();

        if (specialSkills.Count == 0)
        {
            Debug.Log($"[IAEnemySimple] {Enemy.idName} no puede usar habilidades especiales.");
            return AttackState(target);
        }

        return SelectBestSkill(specialSkills, target);
    }

    /// <summary>
    /// Executes the heal state workflow.
    /// </summary>
    private Skill HealState(Fighter target)
    {
        var healSkills = _skills.Where(x => x.skillType == SkillType.Heal && CanUseSkill(x)).ToList();

        if (healSkills.Count == 0)
        {
            Debug.Log($"[IAEnemySimple] {Enemy.idName} no puede usar curaciones.");
            return AttackState(target);
        }

        // In phase 3, pick the heal with highest amount
        if (currentPhase == 3)
        {
            return healSkills
                .OrderByDescending(s => (s as HealthModSkill)?.amount ?? 0f)
                .First();
        }

        return SelectBestSkill(healSkills, target);
    }

    /// <summary>
    /// Sets the skills.
    /// </summary>
    public void SetSkills(Skill[] skills)
    {
        List<Skill> lista = new List<Skill>(skills);
        _skills = lista;
    }

    /// <summary>
    /// Chooses the best body part to target on the given fighter.
    /// </summary>
    public BodyPart ChooseTargetableBodyPart(Fighter target, AIAttackPreference pref, Skill chosenSkill = null)
    {
        if (target == null || target.bodyParts == null || target.bodyParts.Count == 0)
            return BodyPart.None;

        var availableParts = target.bodyParts
            .Where(p => !p.IsDestroyed)
            .ToList();

        if (availableParts.Count == 0)
            return BodyPart.None;

        // Aggregate: most damaged part (lowest currentHealth/maxHealth ratio)
        Fighter.BodyPartData mostDamaged = availableParts.Aggregate((best, next) =>
        {
            float bMax = best.maxHealth;
            float nMax = next.maxHealth;
            float bestRatio = bMax > 0 ? best.currentHealth / bMax : 1f;
            float nextRatio = nMax > 0 ? next.currentHealth / nMax : 1f;
            return nextRatio < bestRatio ? next : best;
        });

        // --- Override layers (in order) ---

        // Layer 1: Critical part — if any part has < 15% HP, finish it off
        foreach (var p in availableParts)
        {
            float pMax = p.maxHealth;
            if (pMax > 0 && p.currentHealth / pMax < 0.15f)
                return p.part;
        }

        // Layer 2: Adjust for consecutive misses — target highest HP part if zero-miss skill exists
        if (Memory.consecutiveMisses >= 2)
        {
            bool hasZeroMissSkill = _skills != null && _skills.Any(s =>
                s is HealthModSkill hms && hms.missChance == 0f && CanUseSkill(s));
            if (hasZeroMissSkill)
            {
                var highestHp = availableParts.Aggregate((best, next) =>
                    next.currentHealth > best.currentHealth ? next : best);
                return highestHp.part;
            }
        }

        // Layer 3: Adjust for parries — switch to Opportunist
        if (Memory.timesParried >= 2)
        {
            pref = AIAttackPreference.Opportunist;
        }

        // Layer 4: Synergy — ApplySCSkill targets parts with no status
        if (chosenSkill is ApplySCSkill && synergyFocus > 0.4f)
        {
            var cleanParts = availableParts.Where(p => p.currentStatus == PartStatus.None).ToList();
            if (cleanParts.Count > 0)
                return cleanParts[Random.Range(0, cleanParts.Count)].part;
        }

        // Layer 5: Synergy — HealthModSkill targets parts with active status to exploit debuffs
        if (chosenSkill is HealthModSkill && synergyFocus > 0.6f)
        {
            var debuffedParts = availableParts.Where(p => p.currentStatus != PartStatus.None).ToList();
            if (debuffedParts.Count > 0)
                return debuffedParts[Random.Range(0, debuffedParts.Count)].part;
        }

        // --- Default: switch on preference ---
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
                    if (mostDamaged != null)
                        return mostDamaged.part;
                    break;
                }
            case AIAttackPreference.Random:
            default:
                {
                    return availableParts[Random.Range(0, availableParts.Count)].part;
                }
        }

        if (mostDamaged != null)
            return mostDamaged.part;

        return availableParts[Random.Range(0, availableParts.Count)].part;
    }
}
