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
        //_IAEnemySimple = gameObject.GetComponent<IAEnemySimple>();
        //

        if (data.level != 0)
            this.stats = new Stats(data.level, data.maxHealth, data.attack, data.deffense, data.spirit, data.speed, data.experience, data.experienceToNextLevel);

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

        //Si la skill requiere un objetivo manual
        if (skill.needsManualTargeting)
        {
            Fighter[] targets = this.GetSkillTargets(skill);
            target = targets[Random.Range(0, targets.Length)];
            animator.Play("Attack");
            skill.AddReceiver(target);
        }
        else
        {
            //Si la skill no necesita targeting manual
            this.AutoConfigureSkillTargeting(skill);
            Fighter[] possibleTargets = this.combatManager.GetOpposingTeam();
            if (possibleTargets.Length > 0)
            {
                target = possibleTargets[Random.Range(0, possibleTargets.Length)];
            }
        }

        //Si tenemos un objetivo, elegimos parte del cuerpo y aplicamos daño
        if (target != null)
        {
            // Elegimos una parte del cuerpo al azar
            BodyPart part = GetRandomTargetableBodyPart(target);
            skill.BodyPartTarget = part;

            //Aplica daño a la parte del cuerpo
            if (skill is HealthModSkill healthSkill)
            {
                float damageToPart = healthSkill.GetModification(target);
                target.ModifyBodyPartHealth(part, damageToPart);

                
                float baseDamage = damageToPart * 1f;
                target.ModifyHealth(baseDamage);

                Debug.Log($"{this.idName} hit {target.idName}'s {part} for {damageToPart} (plus {baseDamage} base damage)");
            }
        }

        
        this.combatManager.OnFighterSkill(skill);
    }


    private BodyPart GetRandomTargetableBodyPart(Fighter target)
    {
        // Obtener partes disponibles
        var availableParts = new List<BodyPartData>();
        foreach (var partData in target.bodyParts)
        {
            if (!partData.IsDestroyed)
                availableParts.Add(partData);
        }

        if (availableParts.Count == 0)
            return BodyPart.None;

        
        List<BodyPartData> preferredParts = new List<BodyPartData>();

        switch (attackPreference)
        {
            case AIAttackPreference.HeadFocused:
                preferredParts = availableParts.Where(p => p.part == BodyPart.Head).ToList();
                break;

            case AIAttackPreference.TorsoFocused:
                preferredParts = availableParts.Where(p => p.part == BodyPart.Torso).ToList();
                break;

            case AIAttackPreference.ArmsFocused:
                preferredParts = availableParts.Where(p => p.part == BodyPart.LeftArm || p.part == BodyPart.RightArm).ToList();
                break;

            case AIAttackPreference.LegsFocused:
                preferredParts = availableParts.Where(p => p.part == BodyPart.LeftLeg || p.part == BodyPart.RightLeg).ToList();
                break;

            case AIAttackPreference.Aggressive:
                preferredParts = availableParts.Where(p => p.part == BodyPart.Torso || p.part == BodyPart.LeftArm || p.part == BodyPart.RightArm).ToList();
                break;

            case AIAttackPreference.Opportunist:
                // Ataca partes más dañadas
                float minHealth = availableParts.Min(p => p.currentHealth / p.maxHealth);
                preferredParts = availableParts.Where(p => (p.currentHealth / p.maxHealth) <= minHealth + 0.2f).ToList();
                break;

            case AIAttackPreference.Random:
            default:
                preferredParts = availableParts;
                break;

        }

            // Si no hay partes preferidas disponibles, elige cualquiera
            if (preferredParts.Count == 0)
                preferredParts = availableParts;

            // Probabilidad de priorizar la parte preferida
            bool usePreferred = Random.value < 0.7f;
            if (usePreferred)
                return preferredParts[Random.Range(0, preferredParts.Count)].part;
            else
                return availableParts[Random.Range(0, availableParts.Count)].part;
        
    }

    }
    

