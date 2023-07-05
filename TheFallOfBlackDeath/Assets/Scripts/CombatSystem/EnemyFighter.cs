using UnityEngine;
using System.Collections;
//TP2 FACUNDO FERREIRO
public class EnemyFighter : Fighter
{
    public EnemyDataBase EnemyDateBase;
    public int EnemyIndex;
    public IAEnemySimple _IAEnemySimple;
    void Awake()
    {
        var data = EnemyDateBase.EnemyDB[EnemyIndex];
        //_IAEnemySimple = gameObject.GetComponent<IAEnemySimple>();
        //

        if (data.level != 0)
            this.stats = new Stats(data.level, data.maxHealth, data.attack, data.deffense, data.spirit, data.speed);
        else
            this.stats = new Stats(20, 50, 40, 30, 60, 15);
    }

    public override void InitTurn()
    {
         StartCoroutine(IA());
        _IAEnemySimple.SetSkills(this.skills);
    }

    IEnumerator IA()
    {
        yield return new WaitForSeconds(1f);
        
        
            //Skill skill = this.skills[Random.Range(0, this.skills.Length)];
            Skill skill = _IAEnemySimple.ExecuteState();
            if (skill == null) skill = this.skills[Random.Range(0, this.skills.Length)];

            skill.SetEmitter(this);

            if (skill.needsManualTargeting)
            {
                animator.Play("Attack");
                Fighter[] targets = this.GetSkillTargets(skill);

                Fighter target = targets[Random.Range(0, targets.Length)];

                skill.AddReceiver(target);
            }
            else
            {
                this.AutoConfigureSkillTargeting(skill);
            }

            this.combatManager.OnFighterSkill(skill);
        

    }
}