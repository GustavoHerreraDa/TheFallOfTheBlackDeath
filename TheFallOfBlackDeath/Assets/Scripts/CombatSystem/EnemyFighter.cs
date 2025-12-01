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
        var data = EnemyDateBase.EnemyDB[EnemyIndex];

        if (data.level != 0)
            this.stats = new Stats(data.level, data.maxHealth, data.attack, data.deffense,
                data.spirit, data.speed, data.experience, data.experienceToNextLevel);
    }

    public override void InitTurn()
    {
        StartCoroutine(IA());
        _IAEnemySimple.SetSkills(this.skills);
    }

    IEnumerator IA()
    {
        yield return new WaitForSeconds(1f);

        Skill skill = _IAEnemySimple.ExecuteState();
        if (skill == null)
            skill = this.skills[Random.Range(0, this.skills.Length)];

        skill.SetEmitter(this);

        Fighter target = null;

        if (skill.needsManualTargeting)
        {
            Fighter[] targets = this.GetSkillTargets(skill);
            target = targets[Random.Range(0, targets.Length)];
            animator.Play("Attack");
            skill.AddReceiver(target);
        }
        else
        {
            this.AutoConfigureSkillTargeting(skill);
            Fighter[] possibleTargets = this.combatManager.GetOpposingTeam();
            if (possibleTargets.Length > 0)
            {
                target = possibleTargets[Random.Range(0, possibleTargets.Length)];
            }
        }

        if (target != null)
        {
            
            BodyPart chosenPart = _IAEnemySimple.ChooseTargetableBodyPart(target, attackPreference);
            skill.BodyPartTarget = chosenPart;

            
            if (skill is HealthModSkill healthSkill)
            {
                float damage = healthSkill.GetModification(target);
                target.ModifyHealth(damage);
                Debug.Log($"{this.idName} eligió atacar {target.idName}'s {chosenPart}");

            }
        }

        this.combatManager.OnFighterSkill(skill);
    }
}
