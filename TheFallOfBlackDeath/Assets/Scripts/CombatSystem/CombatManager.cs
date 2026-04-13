
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

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
    [Header("Narrative (Optional)")]
    public NarrativeLogManager narrativeLogManager;
    public EnemiesPanel enemiesPanel;
    public PlayerSkillPanel skillPanel;
    public Transform mainCharacterPos;
    public Transform secondaryCharacterPos;
    public GameObject playerParent;
    [FormerlySerializedAs("enemyDataBase")] public globalDataBase globalDataBase;
    public EnemyFighter[] enemyFighters;
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
    public BodyPartPanel bodyPartPanel;

    public bool IsReady { get; private set; }

    private List<Fighter> returnBuffer;
    public TurnsDisplay turnsDisplay;

    //Cambio statsManager a list para poder agregar elementos.
    public List<StatsManager> statsManagers;

    public AudioSource audioSource;
    public AudioSource sonidoDeDerrota;


    void Start()
    {

        GameManager.Instance.SetGameState(GameManager.GameStates.BATTLE_STATE);


        if (isRadomEncounter == true)
        {
            EncuentrosAleatorios();
        }

        //Acá instanciaría los player?
        InstantiatePlayerFighters();
        

        this.returnBuffer = new List<Fighter>();
        this.fighters = GameObject.FindObjectsOfType<Fighter>();
        this.enemyFighters = GameObject.FindObjectsOfType<EnemyFighter>();
        this.player = GameObject.FindGameObjectWithTag("Charecter");
        this.SortFightersBySpeed();
        this.MakeTeams();
        DefineStatsManager();

        // Mark readiness after teams and fighters are set
        IsReady = (playerTeam != null && playerTeam.Length > 0) || (enemyTeam != null && enemyTeam.Length > 0);
        Debug.Log($"[CombatManager] Ready={IsReady} fighters={fighters.Length} players={playerTeam.Length} enemies={enemyTeam.Length}");

        LogPanel.Write("Battle initiated.");
        // Narrative: announce enemy encounter lines (optional)
        if (narrativeLogManager != null && enemyFighters != null)
        {
            foreach (var e in enemyFighters)
            {
                if (e != null) narrativeLogManager.EnemyEncounter(e);
            }
        }

        this.combatStatus = CombatStatus.NEXT_TURN;

        this.fighterIndex = -1;
        this.isCombatActive = true;

        StartCoroutine(this.CombatLoop());
    }

    public void EncuentrosAleatorios()
    {
        Cursor.lockState = CursorLockMode.None; 
        Cursor.visible = true; 

        for (int i = 0; i < GameManager.Instance.enemyAnount; i++)
        {
            GameObject NewEnemy = Instantiate(GameManager.Instance.enemyToBattle[i], spawnPoints[i].position, Quaternion.identity) as GameObject;
            NewEnemy.name = NewEnemy.GetComponent<EnemyFighter>().idName + "_" + (i + 1);
            NewEnemy.GetComponent<EnemyFighter>().idName = NewEnemy.name;
            
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
                        yield return new WaitForSeconds(1.5f);
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
                        int totalExp = 0;

                        // Para cada enemigo derrotado
                        foreach (var enemy in enemyTeam)
                        {
                            if (enemy == null) continue;

                            int enemyLevel = enemy.stats.level;
                            int exp = enemyLevel * 20;

                            // Diferencia de niveles
                            int playerLevel = playerTeam[0].stats.level;

                            if (enemyLevel > playerLevel)
                                exp = Mathf.FloorToInt(exp * 2);
                            else if (enemyLevel < playerLevel)
                                exp = Mathf.FloorToInt(exp * 0.6f);

                            totalExp += exp;
                        }

                        foreach (var fighter in playerTeam)
                        {
                            if (fighter is PlayerFighter player)
                            {
                                player.AddExperience(totalExp);
                            }
                        }

                        // Save all player fighters' state
                        if (playerTeam != null)
                        {
                            foreach (var f in playerTeam)
                            {
                                if (f is PlayerFighter pf)
                                {
                                    GameManager.Instance.SavePlayerState(pf);
                                }
                            }
                        }
                        audioSource.Play();
                        Animator[] playerAnimators = player.GetComponentsInChildren<Animator>();
                        foreach (Animator animator in playerAnimators)
                        {
                            Debug.Log("Reproduciendo animaci�n en: " + animator.gameObject.name);
                            animator.Play("Victory");
                        }
                        LogPanel.Write("Victory!");
                        this.isCombatActive = false;
                        GameManager.Instance.SetGameState(GameManager.GameStates.TOWN_STATE);
                        GameManager.Instance.enemyToBattle.Clear();
                        var realName = enemyTeam[0].GetComponent<EnemiesGroup>().GroupName;
                        ListEnemyDefeat.enemiesDefeat.Add(realName);
                        PlayerPrefs.SetString("GrupoEnemigo", realName);

                        Debug.Log("Guardando enemigo derrotado REAL: " + realName);
                        Debug.Log("se encontraron esto grupos" + groupEnemyName);
                        yield return new WaitForSeconds(1.5f);
                        SceneManager.LoadScene(1);

                    }

                    if (defeat)
                    {
                        // Save all player fighters' state on defeat as well
                        if (playerTeam != null)
                        {
                            foreach (var f in playerTeam)
                            {
                                if (f is PlayerFighter pf)
                                {
                                    GameManager.Instance.SavePlayerState(pf);
                                }
                            }
                        }
                        LogPanel.Write("Defeat!");
                        this.isCombatActive = false;
                        yield return new WaitForSeconds(2f);
                        SceneManager.LoadSceneAsync(6);
                        sonidoDeDerrota.Play();
                    }

                    if (this.isCombatActive)
                    {
                        this.combatStatus = CombatStatus.NEXT_TURN;
                    }

                    yield return null;
                    break;
                case CombatStatus.NEXT_TURN:
                    SortFightersBySpeed();
                    yield return new WaitForSeconds(1.5f);

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

                    foreach (var bodyPartCondition in currentFighter.GetCurrentBodyPartStatusConditions().ToArray())
                    {
                        bodyPartCondition.Apply();

                        while (true)
                        {
                            string nextBodyPartMessage = bodyPartCondition.GetNextMessage();
                            if (nextBodyPartMessage == null)
                            {
                                break;
                            }

                            LogPanel.Write(nextBodyPartMessage);
                            yield return new WaitForSeconds(1.5f);
                        }
                    }

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
                    // Narrative: contextual enemy turn line (optional)
                    if (narrativeLogManager != null && currentFighter is EnemyFighter ef)
                    {
                        narrativeLogManager.EnemyTurn(ef);
                    }
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

    private void DefineStatsManager()
    {
        foreach(Fighter fighter in fighters)
        {
            statsManagers.Add(fighter.GetComponent<StatsManager>());
        }
    }

    public void UpdateStatsUI()
    {
        for (int i = 0; i < statsManagers.Count; i++)
        {
            if (statsManagers[i] != null)
            {
                statsManagers[i].UpdateUI();
            }
        }
    }

    private void InstantiatePlayerFighters()
    {
        for (int i = 0; i < globalDataBase.EnemyDB.Count; i++)
        {
            if (globalDataBase.EnemyDB[i].isMainCharacter)
            {
                GameObject mainCharacter = Instantiate(
                    globalDataBase.EnemyDB[i].enemyPrefab,
                    mainCharacterPos.transform.position,
                    Quaternion.Euler(-0.4f, -90, 0),
                    playerParent.transform
                );

                var playerFighter = mainCharacter.GetComponent<PlayerFighter>();

                
                playerFighter.GetSkillPanel(
                    skillPanel,
                    playerFighter.statusPanel,
                    enemiesPanel,
                    bodyPartPanel
                );

                GameManager.Instance.SetMainCharacter(playerFighter);
                GameManager.Instance.ApplySavedStatusToFighter(playerFighter);
                FindObjectOfType<CombatStatusUIController>()
                    .RegisterPlayer(playerFighter);
            }
            else if (globalDataBase.EnemyDB[i].isSecondaryCharacter && GameManager.Instance.hasRecruitedSecondary)
            {
                GameObject secondaryCharacter = Instantiate(
                    globalDataBase.EnemyDB[i].enemyPrefab,
                    secondaryCharacterPos.transform.position,
                    Quaternion.Euler(-0.4f, -90, 0),
                    playerParent.transform
                );

                var playerFighter = secondaryCharacter.GetComponent<PlayerFighter>();

                playerFighter.GetSkillPanel(
                    skillPanel,
                    playerFighter.statusPanel,
                    enemiesPanel,
                    bodyPartPanel
                );

                GameManager.Instance.SetSecondaryCharacter(playerFighter);
                GameManager.Instance.ApplySavedStatusToFighter(playerFighter);
                FindObjectOfType<CombatStatusUIController>()
                    .RegisterPlayer(playerFighter);
            }
            
            
        }
    }

}
