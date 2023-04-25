using UnityEngine;
using System.Collections;

public class EnemyFighter : Fighter
{
    void Awake()
    {
        this.stats = new Stats(20, 50, 40, 30, 60);
    }

    public override void InitTurn()
    {
        if (stats.health > 0)
            StartCoroutine(this.IA());
        //else
        //    StartCoroutine(this.Die());

    }

    IEnumerator IA()
    {
        yield return new WaitForSeconds(1f);

        Skill skill = this.skills[Random.Range(0, this.skills.Length)];

        skill.SetEmitterAndReceiver(
            this, this.combatManager.GetOpposingCharacter());

        this.combatManager.OnFighterSkill(skill);
    }

}