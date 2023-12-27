using UnityEngine;
using System.Collections.Generic;
//TP2 FACUNDO FERREIRO/GUSTAVO TORRES
public class PlayerFighter : Fighter
{
    [Header("UI")]
    public PlayerSkillPanel skillPanel;
    public EnemiesPanel enemiesPanel;

    public EnemyDataBase fightersDateBase;
    public int figherIndex;
    private int activeAllyIndex;
    public Fighter ally1;
    public Fighter ally2;


    private Skill skillToBeExecuted;

    private List<Fighter> allies;

    void Awake()
    {
        var data = fightersDateBase.EnemyDB[figherIndex];
        //_IAEnemySimple = gameObject.GetComponent<IAEnemySimple>();
        //

        if (data.level != 0)
            this.stats = new Stats(data.level, data.maxHealth, data.attack, data.deffense, data.spirit, data.speed);
        else
            this.stats = new Stats(21, 60, 50, 45, 20, 20);

        allies = new List<Fighter>();
        allies.Add(this); // Agregar al jugador actual como el primer aliado activo
        activeAllyIndex = 0; // Establecer el jugador actual como el aliado activo inicialmente
    }

    public override void InitTurn()
    {
        this.skillPanel.ShowForPlayer(this);

        for (int i = 0; i < this.skills.Length; i++)
        {
            this.skillPanel.ConfigureButton(i, this.skills[i].skillName,this.skills[i].ItemsNeeded);
        }

        // Mostrar informaci�n del aliado activo en el panel de estado
        Fighter activeAlly = allies[activeAllyIndex];
        statusPanel.SetStats(activeAlly.idName, activeAlly.stats);

    }

    /// ================================================
    /// <summary>
    /// Se llama desde EnemiesPanel.
    /// </summary>
    /// <param name="index"></param>
    public void ChangeAlly(int newIndex)
    {
        if (newIndex < 0 || newIndex >= allies.Count)
        {
            Debug.LogError("Invalid ally index");
            return;
        }

        activeAllyIndex = newIndex;

        // Actualizar la informaci�n del nuevo aliado activo en el panel de estado
        Fighter activeAlly = allies[activeAllyIndex];
        statusPanel.SetStats(activeAlly.idName, activeAlly.stats);

        // Realizar cualquier otra l�gica necesaria al cambiar de aliado
    }

    public void ExecuteSkill(int index)
    {

        this.skillToBeExecuted = this.skills[index];
        this.skillToBeExecuted.SetEmitter(this);

        if (this.skillToBeExecuted.needsManualTargeting)
        {

            Fighter[] receivers = this.GetSkillTargets(this.skillToBeExecuted);
            this.enemiesPanel.Show(this, receivers);
            this.skillPanel.Hide();

        }
        else
        {
            this.AutoConfigureSkillTargeting(this.skillToBeExecuted);
            this.combatManager.OnFighterSkill(this.skillToBeExecuted);
            this.skillPanel.Hide();

        }
    }

    public void UpdateStats(string statAffected, float amountAffected)
    {
        var data = fightersDateBase.EnemyDB[figherIndex];

        switch (statAffected)
        {
            case "Attack":
                fightersDateBase.UpdateFighterStats(figherIndex, amountAffected, statAffected);
                stats.attack += amountAffected;
                break;
            case "Defense":
                fightersDateBase.UpdateFighterStats(figherIndex, amountAffected, statAffected);
                stats.deffense += amountAffected;
                break;
            default:
                Debug.Log("Stat no válido");
                break;
        }
    }

    public void SetTargetAndAttack(Fighter enemyFigther)
    {
        this.skillToBeExecuted.AddReceiver(enemyFigther);

        this.combatManager.OnFighterSkill(this.skillToBeExecuted);

        this.skillPanel.Hide();
        this.enemiesPanel.Hide();
        this.combatManager.UpdateStatsUI();
    }
    public void Return()
    {
        this.skillPanel.Show();
        this.enemiesPanel.Hide();
    }

    private void AddAlliesToTeam()
    {
        allies.Clear();
        allies.Add(ally1);
        allies.Add(ally2);
        // Agrega aqu� el resto de los aliados a la lista allies
    }

    private void SwitchActiveAlly()
    {
        activeAllyIndex++;
        if (activeAllyIndex >= allies.Count)
        {
            activeAllyIndex = 0;
        }

        Fighter activeAlly = allies[activeAllyIndex];
        // Realizar las acciones necesarias con el aliado activo
    }

    public PlayerFighter GetSkillPanel(PlayerSkillPanel newSkillPanel, StatusPanel newStatusPanel, EnemiesPanel newEnemiesPanel)
    {
        skillPanel = newSkillPanel;
        statusPanel = newStatusPanel;
        enemiesPanel = newEnemiesPanel;
        return this;
    }
}
