using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Serialization;

//TP2 FACUNDO FERREIRO
/// <summary>
/// Implements an AI-driven combatant that selects skills and targets through the enemy decision systems during battle.
/// </summary>
public class EnemyFighter : Fighter
{
    [Header("Narrative")]
    [Tooltip("Override narrative lines for this enemy. If null, NarrativeLogManager will look up by idName in its database.")]
    public EnemyNarrativeEntry narrativeData;
    public AIAttackPreference attackPreference = AIAttackPreference.Random;

    [FormerlySerializedAs("EnemyDateBase")] public globalDataBase globalDateBase;
    public int EnemyIndex;
    public IAEnemySimple _IAEnemySimple;

    [Header("Instance Modifiers")]
    [Tooltip("Modify this specific enemy's stats. Set to 0.5 to make it half as strong (e.g. for a tutorial).")]
    public float healthMultiplier = 1f;
    public float attackMultiplier = 1f;

    // PARTE 2 — Turn result tracking for combat memory
    private bool _lastWasParried;
    private bool _lastWasMiss;
    private BodyPart _lastHitPart;
    private bool _lastTargetUsedHeal;
    private Fighter _subscribedTarget;

    /// <summary>
    /// Initializes cached references and runtime state before the component starts running.
    /// </summary>
    void Awake()
    {
        if (bodyParts != null)
        {
            foreach (var part in bodyParts)
            {
                part.SetOwner(this);
            }
        }

        // Initialize enemy stats safely, falling back if DB is invalid
        Stats safeDefaults = new Stats(5, 30, 10, 8, 5, 5, 20, 0);
        if (globalDateBase != null && EnemyIndex >= 0 && EnemyIndex < globalDateBase.EnemyDB.Count)
        {
            var data = globalDateBase.EnemyDB[EnemyIndex];
            int level = data.level > 0 ? data.level : safeDefaults.level;
            float maxHp = data.maxHealth > 0 ? data.maxHealth : safeDefaults.maxHealth;
            float hp = data.hp > 0 ? data.hp : safeDefaults.health;
            float atk = data.attack > 0 ? data.attack : safeDefaults.attack;
            float def = data.deffense > 0 ? data.deffense : safeDefaults.deffense;
            float spr = data.spirit > 0 ? data.spirit : safeDefaults.spirit;
            float spd = data.speed > 0 ? data.speed : safeDefaults.speed;
            this.stats = new Stats(level, maxHp,  hp, atk,def, spr, spd, data.experience, data.experienceToNextLevel);
        }
        else
        {
            Debug.LogWarning($"[EnemyFighter] EnemyDateBase null or EnemyIndex out of range ({EnemyIndex}). Using safe defaults.");
            this.stats = safeDefaults;
        }

        // Apply unique instance modifiers
        this.stats.maxHealth = Mathf.Round(this.stats.maxHealth * healthMultiplier);
        this.stats.health = this.stats.maxHealth;
        this.stats.attack = Mathf.Round(this.stats.attack * attackMultiplier);

        // Ensure health is at least 1
        this.stats.health = Mathf.Clamp(this.stats.health, 1, this.stats.maxHealth);
    }

    /// <summary>
    /// Subscribes to IA phase change events.
    /// </summary>
    void Start()
    {
        if (_IAEnemySimple != null)
        {
            _IAEnemySimple.OnPhaseChanged += OnPhaseChanged;
        }
    }

    void OnDestroy()
    {
        if (_IAEnemySimple != null)
        {
            _IAEnemySimple.OnPhaseChanged -= OnPhaseChanged;
        }

        UnsubscribeFromTarget();
    }

    /// <summary>
    /// Handles phase change events from the IA.
    /// </summary>
    private void OnPhaseChanged(int phase)
    {
        Debug.Log($"[EnemyFighter] {idName} entró en fase {phase}");

        if (narrativeData != null)
        {
            try
            {
                string line = narrativeData.GetRandom(narrativeData.turnMessages);
                if (!string.IsNullOrEmpty(line))
                    Debug.Log($"[EnemyFighter] {idName} narrative: {line}");
            }
            catch
            {
                // fail silencioso
            }
        }
    }

    /// <summary>
    /// Subscribes to the target's OnDamageResolved to detect parries and misses.
    /// </summary>
    private void SubscribeToTarget(Fighter target)
    {
        if (target == _subscribedTarget) return;

        UnsubscribeFromTarget();

        if (target != null)
        {
            target.OnDamageResolved += OnTargetDamageResolved;
            _subscribedTarget = target;
        }
    }

    private void UnsubscribeFromTarget()
    {
        if (_subscribedTarget != null)
        {
            _subscribedTarget.OnDamageResolved -= OnTargetDamageResolved;
            _subscribedTarget = null;
        }
    }

    /// <summary>
    /// Handles damage resolved on the target to detect parries and misses.
    /// </summary>
    private void OnTargetDamageResolved(DamageResult result)
    {
        // Only track results from our own attacks
        if (result.attacker != this) return;

        if (result.isMiss)
        {
            _lastWasMiss = true;
        }
        else if (result.appliedAmount == 0f && !result.isMiss)
        {
            _lastWasParried = true;
        }

        _lastHitPart = result.targetPart;
    }

    /// <summary>
    /// Starts the enemy decision flow for the current combat turn.
    /// </summary>
    public override void InitTurn()
    {
        if (!isAlive)
        {
            Debug.Log($"[EnemyFighter] {idName} está muerto, no puede iniciar turno.");
            combatManager.combatStatus = CombatStatus.CHECK_FOR_VICTORY;
            return;
        }

        // Record previous turn result into combat memory
        if (_IAEnemySimple != null)
        {
            _IAEnemySimple.RecordTurnResult(_lastWasParried, _lastWasMiss, _lastHitPart, _lastTargetUsedHeal);
            _lastWasParried = false;
            _lastWasMiss = false;
            _lastHitPart = BodyPart.None;
            _lastTargetUsedHeal = false;
        }

        if (_IAEnemySimple != null)
            _IAEnemySimple.SetSkills(this.skills);

        StartCoroutine(IA());
    }

    /// <summary>
    /// Executes the ia workflow.
    /// </summary>
    IEnumerator IA()
    {
        // Wait a small delay and also ensure combatManager and teams are ready
        yield return new WaitForSeconds(0.5f);
        int safetyFrames = 20;
        while (safetyFrames-- > 0)
        {
            if (this.combatManager != null && this.combatManager.fighterIndex >= 0)
            {
                var opp = this.combatManager.GetOpposingTeam();
                if (opp != null && opp.Length > 0)
                    break;
            }
            yield return null; // wait a frame for registration
        }

        // Choose a skill
        Skill skill = null;
        if (_IAEnemySimple != null)
        {
            skill = _IAEnemySimple.ExecuteState();
        }
        if (skill == null)
        {
            if (this.skills != null && this.skills.Length > 0)
                skill = this.skills[Random.Range(0, this.skills.Length)];
            else
            {
                Debug.LogWarning("[EnemyFighter] No skills available for enemy. Skipping turn.");
                yield break;
            }
        }

        skill.SetEmitter(this);

        Fighter target = null;

        if (skill.needsManualTargeting)
        {
            Fighter[] targets = this.GetSkillTargets(skill);
            if (targets == null || targets.Length == 0)
            {
                Debug.LogWarning("[EnemyFighter] No manual targets available. Waiting one frame.");
                yield return null;
            }
            else
            {
                target = targets[Random.Range(0, targets.Length)];
                if (animator != null) animator.Play("Attack");
                skill.AddReceiver(target);
            }
        }
        else
        {
            this.AutoConfigureSkillTargeting(skill);
            Fighter[] possibleTargets = (this.combatManager != null) ? this.combatManager.GetOpposingTeam() : new Fighter[0];
            if (possibleTargets != null && possibleTargets.Length > 0)
            {
                target = possibleTargets[Random.Range(0, possibleTargets.Length)];
            }
        }

        if (target != null)
        {
            // Subscribe to target for parry/miss detection
            SubscribeToTarget(target);

            BodyPart chosenPart = (_IAEnemySimple != null)
                ? _IAEnemySimple.ChooseTargetableBodyPart(target, attackPreference, skill)
                : BodyPart.Torso;
            skill.BodyPartTarget = chosenPart;

            if (skill is HealthModSkill healthSkill)
            {
                Debug.Log($"[EnemyFighter] {this.idName} eligió atacar {target.idName}'s {chosenPart}");
            }
        }
        else
        {
            Debug.Log("[EnemyFighter] No target selected. Proceeding without a direct target.");
        }

        if (this.combatManager != null)
            this.combatManager.OnFighterSkill(skill);
    }
}
