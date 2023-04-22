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

    public void ModifyHealth(float amount)
    {
        this.stats.health = Mathf.Clamp(this.stats.health + amount, 0f, this.stats.maxHealth);

        this.stats.health = Mathf.Round(this.stats.health);
        this.statusPanel.SetHealth(this.stats.health, this.stats.maxHealth);
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

    public abstract void Death();

    public abstract void InitTurn();
}