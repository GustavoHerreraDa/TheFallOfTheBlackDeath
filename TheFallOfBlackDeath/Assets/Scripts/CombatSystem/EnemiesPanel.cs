using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemiesPanel : MonoBehaviour
{
    public GameObject enemyButtonPrefab;
    public Transform buttonContainer;
    public BodyPartPanel bodyPartPanel;

    private PlayerFighter currentPlayer;
    private Skill currentSkill;


    public void Show(PlayerFighter player, Skill skill, Fighter[] enemies)
    {
        gameObject.SetActive(true);
        enemyButtonPrefab.SetActive(true);
        currentPlayer = player;
        currentSkill = skill;

        // Limpiar botones previos
        foreach (Transform child in buttonContainer)
            Destroy(child.gameObject);

        // Crear botones por cada enemigo
        foreach (Fighter enemy in enemies)
        {
            if (!enemy.isAlive) continue;

            GameObject btnObj = Instantiate(enemyButtonPrefab, buttonContainer);
            Button btn = btnObj.GetComponent<Button>();
            Text btnText = btnObj.GetComponentInChildren<Text>();
            btnText.text = enemy.idName;

            btn.onClick.AddListener(() => OnEnemySelected(enemy));
            {
                enemyButtonPrefab.SetActive(false);
                Debug.Log("Enemy selected: " + enemy.idName);
                bodyPartPanel.Show(currentPlayer, enemy, currentSkill); // <- acá se abre el panel de partes del cuerpo
            }
        }
    }

    private void OnEnemySelected(Fighter target)
    {
        // Caso: skill requiere elegir parte del cuerpo
        if (currentSkill.targeting == SkillTargeting.SINGLE_OPPONENT)
        {
            Hide();
            bodyPartPanel.Show(currentPlayer, target, currentSkill);
        }

        else
        {
            // Skill normal: se ejecuta directo
            currentSkill.AddReceiver(target);
            currentPlayer.combatManager.OnFighterSkill(currentSkill);
            Hide();
        }
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
