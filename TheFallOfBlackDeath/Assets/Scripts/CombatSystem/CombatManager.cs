
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public enum CombatStatus
{
    WAITING_FOR_FIGHTER,
    FIGHTER_ACTION,
    CHECK_ACTION_MESSAGES,
    CHECK_FOR_VICTORY,
    NEXT_TURN,
    CHECK_FIGHTER_STATUS_CONDITION
}

public class CombatManager : MonoBehaviour
{
    public List<GameObject> enemyFighters = new List<GameObject>();
    public string groupEnemyName;
    public Fighter[] playerTeam;
    public Fighter[] enemyTeam;
    public Fighter[] fighters;
    public int fighterIndex;
    private GameObject player;
    public bool isCombatActive;
    public CombatStatus combatStatus;
    private Skill currentFighterSkill;
    public int enemyAmount;
    //SPAWN POINTS
    public bool isRadomEncounter = false;
    public List<Transform> spawnPoints = new List<Transform>();




    private List<Fighter> returnBuffer;
    public TurnsDisplay turnsDisplay;
    public StatsManager[] statsManagers;

    public AudioSource audioSource;
    public AudioSource sonidoDeDerrota;

    void Start()
    {
        this.returnBuffer = new List<Fighter>();
        this.fighters = GameObject.FindObjectsOfType<Fighter>();
        this.player = GameObject.FindGameObjectWithTag("Charecter");
        this.SortFightersBySpeed();
        this.MakeTeams();

        LogPanel.Write("Battle initiated.");

        this.combatStatus = CombatStatus.NEXT_TURN;

        this.fighterIndex = -1;
        this.isCombatActive = true;

        if (isRadomEncounter == true)
        {
            EncuentrosAleatorios();
        }

        StartCoroutine(this.CombatLoop());
    }

    public void EncuentrosAleatorios()
    {

        for (int i = 0; i < GameManager.Instance.enemyAnount; i++)
        {
            GameObject NewEnemy = Instantiate(GameManager.Instance.enemyToBattle[i], spawnPoints[i].position, Quaternion.identity) as GameObject;
            NewEnemy.name = NewEnemy.GetComponent<EnemyFighter>().idName + "_" + (i + 1);
            NewEnemy.GetComponent<EnemyFighter>().idName = NewEnemy.name;
            enemyFighters.Add(NewEnemy);
        }

    }

    private void SortFightersBySpeed()
    {
        bool sorted = false;
        while (!sorted)
        {
            sorted = true;

            for (int i = 0; i < this.fighters.Length - 1; i++)
            {
                Fighter a = this.fighters[i];
                Fighter b = this.fighters[i + 1];

                float aSpeed = a.GetCurrentStats().speed;
                float bSpeed = b.GetCurrentStats().speed;

                if (bSpeed > aSpeed)
                {
                    this.fighters[i] = b;
                    this.fighters[i + 1] = a;

                    sorted = false;
                }
            }
        }

        if (turnsDisplay != null)
            turnsDisplay.SetText(this.fighters);
    }

    private void MakeTeams()
    {
        List<Fighter> playersBuffer = new List<Fighter>();
        List<Fighter> enemiesBuffer = new List<Fighter>();

        foreach (var fgtr in this.fighters)
        {
            if (fgtr.team == Team.PLAYERS)
            {
                playersBuffer.Add(fgtr);
            }
            else if (fgtr.team == Team.ENEMIES)
            {
                enemiesBuffer.Add(fgtr);
            }

            fgtr.combatManager = this;
        }

        this.playerTeam = playersBuffer.ToArray();
        this.enemyTeam = enemiesBuffer.ToArray();
    }

    IEnumerator CombatLoop()
    {
        while (this.isCombatActive)
        {
            switch (this.combatStatus)
            {
                case CombatStatus.WAITING_FOR_FIGHTER:
                    yield return null;
                    break;

                case CombatStatus.FIGHTER_ACTION:
                    LogPanel.Write($"{this.fighters[this.fighterIndex].idName} uses {currentFighterSkill.skillName}.");

                    yield return null;

                    // Executing fighter skill
                    currentFighterSkill.Run();

                    // Wait for fighter skill animation
                    yield return new WaitForSeconds(currentFighterSkill.animationDuration);

                    this.combatStatus = CombatStatus.CHECK_ACTION_MESSAGES;
                    Debug.Log("Se ejecuta la def atta");
                    this.UpdateStatsUI();
                    break;
                case CombatStatus.CHECK_ACTION_MESSAGES:
                    string nextMessage = this.currentFighterSkill.GetNextMessage();

                    if (nextMessage != null)
                    {
                        LogPanel.Write(nextMessage);
                        yield return new WaitForSeconds(0.2f);
                    }
                    else
                    {
                        this.currentFighterSkill = null;
                        this.combatStatus = CombatStatus.CHECK_FOR_VICTORY;
                        yield return null;
                    }
                    break;

                case CombatStatus.CHECK_FOR_VICTORY:
                    bool arePlayersAlive = false;
                    foreach (var figther in this.playerTeam)
                    {
                        arePlayersAlive |= figther.isAlive;
                    }

                    // if (this.playerTeam[0].isAlive OR this.playerTeam[1].isAlive)

                    bool areEnemiesAlive = false;
                    foreach (var figther in this.enemyTeam)
                    {
                        areEnemiesAlive |= figther.isAlive;
                    }

                    bool victory = areEnemiesAlive == false;
                    bool defeat = arePlayersAlive == false;

                    if (victory)
                    {
                        audioSource.Play();
                        Animator[] playerAnimators = player.GetComponentsInChildren<Animator>();
                        foreach (Animator animator in playerAnimators)
                        {
                            Debug.Log("Reproduciendo animación en: " + animator.gameObject.name);
                            animator.Play("Victory");
                        }
                        LogPanel.Write("Victory!");
                        this.isCombatActive = false;
                        ListEnemyDefeat.enemiesDefeat.Add(groupEnemyName);
                        PlayerPrefs.SetString("GrupoEnemigo", groupEnemyName);
                        yield return new WaitForSeconds(2f);
                        SceneManager.LoadScene(1);

                    }

                    if (defeat)
                    {
                        LogPanel.Write("Derrota!");
                        this.isCombatActive = false;
                        yield return new WaitForSeconds(2f);
                        SceneManager.LoadSceneAsync(7);
                        sonidoDeDerrota.Play();
                    }

                    if (this.isCombatActive)
                    {
                        this.combatStatus = CombatStatus.NEXT_TURN;
                    }

                    yield return null;
                    break;
                case CombatStatus.NEXT_TURN:
                    yield return new WaitForSeconds(0.2f);

                    Fighter current = null;

                    do
                    {
                        this.fighterIndex = (this.fighterIndex + 1) % this.fighters.Length;

                        current = this.fighters[this.fighterIndex];
                    } while (current.isAlive == false);

                    this.combatStatus = CombatStatus.CHECK_FIGHTER_STATUS_CONDITION;

                    break;

                case CombatStatus.CHECK_FIGHTER_STATUS_CONDITION:
                    var currentFighter = this.fighters[this.fighterIndex];

                    var statusCondition = currentFighter.GetCurrentStatusCondition();

                    if (statusCondition != null)
                    {
                        statusCondition.Apply();

                        while (true)
                        {
                            string nextSCMessage = statusCondition.GetNextMessage();
                            if (nextSCMessage == null)
                            {
                                break;
                            }

                            LogPanel.Write(nextSCMessage);
                            yield return new WaitForSeconds(2f);
                        }

                        if (statusCondition.BlocksTurn())
                        {
                            this.combatStatus = CombatStatus.CHECK_FOR_VICTORY;
                            break;
                        }
                    }

                    LogPanel.Write($"{currentFighter.idName} has the turn.");
                    if (currentFighter.gameObject.activeInHierarchy)
                    {
                        currentFighter.InitTurn();
                        this.combatStatus = CombatStatus.WAITING_FOR_FIGHTER;
                    }
                    else
                    {
                        this.combatStatus = CombatStatus.CHECK_FOR_VICTORY;

                    }
                    break;
            }
        }
    }

    /*public Fighter[] FilterJustAlive(Fighter[] team)
    {
        this.returnBuffer.Clear();

        foreach (var fgtr in team)
        {
            if (fgtr.isAlive)
            {
                this.returnBuffer.Add(fgtr);
            }
        }

        return this.returnBuffer.ToArray();
    }*/
    public Fighter[] FilterJustAlive(Fighter[] team)
    {
        this.returnBuffer.Clear();

        foreach (var fgtr in team)
        {
            if (fgtr != null && fgtr.isAlive)
            {
                this.returnBuffer.Add(fgtr);
            }
        }

        return this.returnBuffer.ToArray();
    }

    public Fighter[] GetOpposingTeam()
    {
        Fighter currentFighter = this.fighters[this.fighterIndex];

        Fighter[] team = null;
        if (currentFighter.team == Team.PLAYERS)
        {
            team = this.enemyTeam;
        }
        else if (currentFighter.team == Team.ENEMIES)
        {
            team = this.playerTeam;
        }

        return this.FilterJustAlive(team);
    }

    public Fighter[] GetAllyTeam()
    {
        Fighter currentFighter = this.fighters[this.fighterIndex];

        Fighter[] team = null;
        if (currentFighter.team == Team.PLAYERS)
        {
            team = this.playerTeam;
        }
        else
        {
            team = this.enemyTeam;
        }

        return this.FilterJustAlive(team);
    }

    public void OnFighterSkill(Skill skill)
    {
        this.currentFighterSkill = skill;
        this.combatStatus = CombatStatus.FIGHTER_ACTION;
    }
    public void UpdateStatsUI()
    {
        for (int i = 0; i < statsManagers.Length; i++)
        {
            if (statsManagers[i] != null)
            {
                statsManagers[i].UpdateUI();
            }
        }
    }

}