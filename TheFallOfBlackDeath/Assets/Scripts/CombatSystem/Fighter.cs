using System.Collections.Generic;

using UnityEngine;
//TP2 AUGUSTO NANINI/FACUNDO FERREIRO

public abstract class Fighter : MonoBehaviour
{
    [System.Serializable]
    public class BodyPartData
    {
        public BodyPart part;
        public Transform hitPoint;
        public float maxHealth = 100f;
        public float currentHealth;
      


        public BodyPartData(BodyPart part, float health)
        {
            this.part = part;
            this.maxHealth = health;
            this.currentHealth = health;
        }

        public bool IsDestroyed => currentHealth <= 0;
    }
    public List<BodyPartData> bodyParts;



    public event System.Action<BodyPart> OnBodyPartDestroyedEvent;

    public Team team;
    public string idName;
    public StatusPanel statusPanel;
    public Animator animator;
    public CombatManager combatManager;
    public AudioSource audioSource;
    public delegate void HealthModificationDelegate(float amount);
    public HealthModificationDelegate healthModificationDelegate;

    public List<StatusMod> statusMods;
    public bool legBroken;
    public Stats stats;
    public Stats modedStats;
    public Skill[] skills;
    public StatusCondition statusCondition;

    [SerializeField]
    public Transform CameraPivot;

    [SerializeField]
    public Transform DamagePivot;

    public BodyPartData GetBodyPart(BodyPart part)
    {
        return bodyParts.Find(p => p.part == part);
    }

    public bool isAlive
    {
        get => this.stats.health > 0;
    }

    protected virtual void Start()
    {
        if (this.statusPanel != null)
            this.statusPanel.SetStats(this.idName, this.stats);

        this.skills = this.GetComponentsInChildren<Skill>();
        this.modedStats = stats;
        this.statusMods = new List<StatusMod>();
        legBroken = false;
    }

    protected void AutoConfigureSkillTargeting(Skill skill)
    {
        skill.SetEmitter(this);

        switch (skill.targeting)
        {
            case SkillTargeting.AUTO:
                skill.AddReceiver(this);
                break;
            case SkillTargeting.ALL_ALLIES:
                Fighter[] allies = this.combatManager.GetAllyTeam();
                foreach (var receiver in allies)
                {
                    skill.AddReceiver(receiver);
                }
                break;
            case SkillTargeting.ALL_OPPONENTS:
                {
                    Fighter[] enemies = this.combatManager.GetOpposingTeam();

                    foreach (var receiver in enemies)
                    {
                        
                        skill.AddReceiver(receiver);

                       
                       
                        foreach (var partData in receiver.bodyParts)
                        {
                            if (!partData.IsDestroyed)
                            {
                                if (skill is HealthModSkill healthSkill)
                                {
                                    float amount = healthSkill.GetModification(receiver);
                                    receiver.ModifyBodyPartHealth(partData.part, amount);

                                }
                            }
                        }
                    }
                    break;
                }

            case SkillTargeting.SINGLE_ALLY:
            case SkillTargeting.SINGLE_OPPONENT:
                throw new System.InvalidOperationException("Unimplemented! This skill needs manual targeting.");
        }
    }

    protected Fighter[] GetSkillTargets(Skill skill)
    {
        switch (skill.targeting)
        {
            case SkillTargeting.AUTO:
            case SkillTargeting.ALL_ALLIES:
            case SkillTargeting.ALL_OPPONENTS:
                throw new System.InvalidOperationException("Unimplemented! This skill doesn't need manual targeting.");
            case SkillTargeting.SINGLE_ALLY:
                return this.combatManager.GetAllyTeam();
            case SkillTargeting.SINGLE_OPPONENT:
                return this.combatManager.GetOpposingTeam();
        }

        // Esto no deberia ejecutarse nunca pero hay que ponerlo para hacer al compilador feliz.
        throw new System.InvalidOperationException("Fighter::GetSkillTargets. Unreachable!");
    }

    protected void Die()
    {
        this.statusPanel.gameObject.SetActive(false);
        this.gameObject.SetActive(false);
    }

    public void ModifyHealth(float amount)
    {
        float previousHealth = this.stats.health;

        this.stats.health = Mathf.Clamp(this.stats.health + amount, 0f, this.stats.maxHealth);
        this.stats.health = Mathf.Round(this.stats.health);

  
        if (healthModificationDelegate != null)
        {
            float modifiedAmount = this.stats.health - previousHealth;
            healthModificationDelegate(modifiedAmount);
        }

        this.statusPanel.SetHealth(this.stats.health, this.stats.maxHealth);

     
        if (amount > 0f)
        {
            this.animator.Play("Heal");
        }
        else
        {
            this.animator.Play("Damages");
        }

        if (this.isAlive == false)
        {
            audioSource.Play();
            animator.Play("Death");
            Invoke("Die", 2f);
        }
    }

    public void ModifyBodyPartHealth(BodyPart part, float amount)
    {
        BodyPartData target = bodyParts.Find(p => p.part == part);
        if (target == null) return;

        float prev = target.currentHealth;
        target.currentHealth = Mathf.Clamp(target.currentHealth + amount, 0, target.maxHealth);

        Debug.Log($"{part} recibió {amount}. Salud actual: {target.currentHealth}");
        this.animator.Play("Damages");

        if (prev > 0 && target.IsDestroyed)
        {
            OnBodyPartDestroyed(target);
        }

        if (target.currentHealth == 0)
        {
            Vector3 textPos = transform.position + Vector3.up * 3f;
            FloatingTextManager.Instance.ShowText($"{part} destroyed!", textPos, Color.magenta);

            //Animator.SetTrigger("LoseLimb"); o un ParticleSystem
        }
    }




    public Stats GetCurrentStats()
    {
        Stats modedStats = this.stats;

        foreach (var mod in this.statusMods)
        {
            modedStats = mod.Apply(modedStats);
        }

        return modedStats;
    }

    public StatusCondition GetCurrentStatusCondition()
    {
        if (this.statusCondition != null && this.statusCondition.hasExpired)
        {
            Destroy(this.statusCondition.gameObject);
            this.statusCondition = null;
        }

        return this.statusCondition;
    }

    public void SetModStats(Stats stats)
    {
        modedStats = stats;
    }

    private void OnBodyPartDestroyed(BodyPartData part)
    {
        
        OnBodyPartDestroyedEvent?.Invoke(part.part);

        switch (part.part)
        {
            case BodyPart.Head:
                Debug.Log("Cabeza destruida → muerte instantánea");
                ModifyHealth(-stats.health);
                break;
            case BodyPart.Torso:
                Debug.Log("Torso destruido → muerte instantánea");
                ModifyHealth(-stats.health);
                break;
            case BodyPart.LeftLeg:
                Debug.Log("Piernas destruidas → no puede moverse");
                legBroken = true;
                //modedStats.speed -= 5;
                break;
            case BodyPart.RightLeg:
                Debug.Log("Piernas destruidas → no puede moverse");
                legBroken = true;
                //modedStats.speed -= 5;
                break;
            case BodyPart.RightArm:
                Debug.Log("Brazos destruidos → no puede atacar");
                modedStats.attack -= 10;
                break;
            case BodyPart.LeftArm:
                Debug.Log("Brazos destruidos → no puede atacar");
                modedStats.attack -= 10;
                break;
                
        }
    }

    public Transform GetHitPoint(BodyPart part)
    {
        if (part == BodyPart.None)
            return DamagePivot;

        foreach (var bp in bodyParts)
        {
            if (bp.part == part && bp.hitPoint != null)
                return bp.hitPoint;
        }

        // Fallback de seguridad
        return DamagePivot;
    }

    public abstract void InitTurn();
}