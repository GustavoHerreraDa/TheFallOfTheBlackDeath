using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
//TP2 FACUNDO FERREIRO
public class EnemyFighter : Fighter
{
    public AIAttackPreference attackPreference = AIAttackPreference.Random;

    public EnemyDataBase EnemyDateBase;
    public int EnemyIndex;
    public IAEnemySimple _IAEnemySimple;

    void Awake()
    {
        // Initialize enemy stats safely, falling back if DB is invalid
        Stats safeDefaults = new Stats(5, 30, 10, 8, 5, 5);
        if (EnemyDateBase != null && EnemyIndex >= 0 && EnemyIndex < EnemyDateBase.EnemyDB.Count)
        {
            var data = EnemyDateBase.EnemyDB[EnemyIndex];
            int level = data.level > 0 ? data.level : safeDefaults.level;
            float maxHp = data.maxHealth > 0 ? data.maxHealth : safeDefaults.maxHealth;
            float atk = data.attack > 0 ? data.attack : safeDefaults.attack;
            float def = data.deffense > 0 ? data.deffense : safeDefaults.deffense;
            float spr = data.spirit > 0 ? data.spirit : safeDefaults.spirit;
            float spd = data.speed > 0 ? data.speed : safeDefaults.speed;
            this.stats = new Stats(level, maxHp, atk, def, spr, spd, data.experience, data.experienceToNextLevel);
        }
        else
        {
            Debug.LogWarning($"[EnemyFighter.Awake] EnemyDateBase null or EnemyIndex out of range ({EnemyIndex}). Using safe defaults.");
            this.stats = safeDefaults;
        }
        // Ensure health is at least 1
        this.stats.health = Mathf.Clamp(this.stats.health, 1, this.stats.maxHealth);
    }

    public override void InitTurn()
    {
        StartCoroutine(IA());
        if (_IAEnemySimple != null)
            _IAEnemySimple.SetSkills(this.skills);
    }

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
                Debug.LogWarning("[EnemyFighter.IA] No skills available for enemy. Skipping turn.");
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
                Debug.LogWarning("[EnemyFighter.IA] No manual targets available. Waiting one frame.");
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
            BodyPart chosenPart = (_IAEnemySimple != null) ? _IAEnemySimple.ChooseTargetableBodyPart(target, attackPreference) : BodyPart.Torso;
            skill.BodyPartTarget = chosenPart;

            if (skill is HealthModSkill healthSkill)
            {
                float damage = healthSkill.GetModification(target);
                target.ModifyHealth(damage);
                Debug.Log($"{this.idName} eligió atacar {target.idName}'s {chosenPart}");
            }
        }
        else
        {
            Debug.Log("[EnemyFighter.IA] No target selected. Proceeding without a direct target.");
        }

        if (this.combatManager != null)
            this.combatManager.OnFighterSkill(skill);
    }
}
