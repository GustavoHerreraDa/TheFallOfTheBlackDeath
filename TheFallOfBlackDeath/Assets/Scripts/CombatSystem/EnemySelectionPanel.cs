
using UnityEngine;
using UnityEngine.UI;

public class EnemySelectionPanel : MonoBehaviour
{
    public GameObject[] enemyButtons;
    public Text[] enemyButtonLabels;
    public GameObject enemySelectionPanel;
    private CombatManager combatManager;




    private void Awake()
    {
        this.Hide();
        combatManager = FindObjectOfType<CombatManager>();

        foreach (var btn in this.enemyButtons)
        {
            btn.SetActive(false);
        }
    }
    private void Start()
    {
        int index = 0;

        foreach (var enemy in combatManager.enemyFighters)
        {
            ConfigureButtons(index, enemy.idName, true);
            index = +1;
        }
    }

    public void EnableButtonsWithEnemy()
    {
        int index = 0;

        foreach (var enemy in combatManager.enemyFighters)
        {

            ConfigureButtons(index, enemy.idName, enemy.isAlive);

            index = +1;
        }
    }

    public void EnableButtonsWithPlayers()
    {
        int index = 0;

        foreach (var enemy in combatManager.playerFighters)
        {
            ConfigureButtons(index, enemy.idName, true);
            index = +1;
        }
    }

    public void ConfigureButtons(int index, string skillName, bool enableButton)
    {
        this.enemyButtons[index].SetActive(enableButton);
        this.enemyButtonLabels[index].text = skillName;
    }

    public void Show()
    {
        this.enemySelectionPanel.SetActive(true);
    }

    public void Hide()
    {
        this.enemySelectionPanel.SetActive(false);
    }

    private void GetSkillTypeFromCharacter()
    {

    }
}
