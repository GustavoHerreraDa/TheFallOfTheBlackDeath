using System.Collections;
using UnityEngine;


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
    private EnemyFighter Enemy;
    private int Attacks;
    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void ExecuteState()
    {
        switch (currentState)
        {
            case EnemyStateSimple.Attack:
                AttackState();
                // Comprobar las condiciones de transición
                if (Attacks > 2)
                {
                    Attacks = 0;
                    currentState = EnemyStateSimple.UseAbility;
                }
                else if (Enemy.GetCurrentStats().health * 100 / Enemy.GetCurrentStats().health < 50 && lastSkill.skillType != SkillType.Heal)
                {
                    currentState = EnemyStateSimple.Heal;
                }
                break;

            case EnemyStateSimple.UseAbility:
                UseAbilityState();
                // Comprobar las condiciones de transición
                if (lastSkill.skillType == SkillType.SpecialHability)
                {
                    currentState = EnemyStateSimple.Attack;
                }
                break;

            // Implementar los otros estados de manera similar

            default:
                break;
        }
    }

    private void AttackState()
    {
        // Lógica para seleccionar a qué personaje atacar
        // Lógica para realizar el ataque
    }

    private void UseAbilityState()
    {
        // Lógica para seleccionar y usar una habilidad
    }

    private void HealState()
    {
        // Lógica para curarse utilizando un objeto de curación
    }

    private void FleeState()
    {
        // Lógica para huir del combate
    }
}
