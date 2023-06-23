using UnityEngine;

public class PlayerFighter : Fighter
{
    [Header("UI")]
    public PlayerSkillPanel skillPanel;
    public EnemiesPanel enemiesPanel;

    public EnemyDataBase fightersDateBase;
    public int figherIndex;

    private Skill skillToBeExecuted;

    void Awake()
    {
        var data = fightersDateBase.EnemyDB[figherIndex];
        //_IAEnemySimple = gameObject.GetComponent<IAEnemySimple>();
        //

        if (data.level != 0)
            this.stats = new Stats(data.level, data.maxHealth, data.attack, data.deffense, data.spirit, data.speed);
        else
            this.stats = new Stats(21, 60, 50, 45, 20, 20);

    }

    public override void InitTurn()
    {
        this.skillPanel.ShowForPlayer(this);

        for (int i = 0; i < this.skills.Length; i++)
        {
            this.skillPanel.ConfigureButton(i, this.skills[i].skillName);
        }
    }

    /// ================================================
    /// <summary>
    /// Se llama desde EnemiesPanel.
    /// </summary>
    /// <param name="index"></param>

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
}