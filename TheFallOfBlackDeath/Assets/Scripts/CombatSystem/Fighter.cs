using UnityEngine;
using System.Collections.Generic;

public abstract class Fighter : MonoBehaviour
{
    public string idName;
    public StatusPanel statusPanel;

    public CombatManager combatManager;

    public List<StatusMod> statusMods;

    protected Stats stats;


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
        this.statusPanel.SetStats(this.idName, this.stats);
        this.skills = this.GetComponentsInChildren<Skill>();
        this.statusMods = new List<StatusMod>();
    }
    public void Die()
    {
        this.statusPanel.gameObject.SetActive(false);
        this.gameObject.SetActive(false);
    }

    public void ModifyHealth(float amount)
    {
    
        this.stats.health = Mathf.Clamp(this.stats.health + amount, 0f, this.stats.maxHealth);

        this.stats.health = Mathf.Round(this.stats.health);
        this.statusPanel.SetHealth(this.stats.health, this.stats.maxHealth);


        if  (this.isAlive == false)
        {
            Invoke("Die", 0.75f);
        }
    }

    public Stats GetCurrentStats()
    {
        // TODO: Stats modifications
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

    public abstract void InitTurn();
}