using UnityEngine;
using System.Collections.Generic;
//TP2 AUGUSTO NANINI/FACUNDO FERREIRO

public abstract class Fighter : MonoBehaviour
{
    public Team team;
    public string idName;
    public StatusPanel statusPanel;
    public Animator animator;
    public CombatManager combatManager;
    public AudioSource audioSource;
    public delegate void HealthModificationDelegate(float amount);
    public HealthModificationDelegate healthModificationDelegate;

    public List<StatusMod> statusMods;

    public Stats stats;
    public Stats modedStats;
    protected Skill[] skills;
    public StatusCondition statusCondition;

    [SerializeField]
    public Transform CameraPivot;

    [SerializeField]
    public Transform DamagePivot;

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
                Fighter[] enemies = this.combatManager.GetOpposingTeam();
                foreach (var receiver in enemies)
                {
                    skill.AddReceiver(receiver);
                }
                break;
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

        // Esto no debería ejecutarse nunca pero hay que ponerlo para hacer al compilador feliz.
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

        // Llamada al delegate antes de actualizar el panel de estado
        if (healthModificationDelegate != null)
        {
            float modifiedAmount = this.stats.health - previousHealth;
            healthModificationDelegate(modifiedAmount);
        }

        this.statusPanel.SetHealth(this.stats.health, this.stats.maxHealth);

        // Resto de la lógica existente
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

    public abstract void InitTurn();
}