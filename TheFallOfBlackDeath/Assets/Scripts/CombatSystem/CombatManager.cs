
using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

/// <summary>
/// Defines the named values used by combat status.
/// </summary>
public enum CombatStatus
{
    WAITING_FOR_FIGHTER,
    FIGHTER_ACTION,
    CHECK_ACTION_MESSAGES,
    CHECK_FOR_VICTORY,
    NEXT_TURN,
    CHECK_FIGHTER_STATUS_CONDITION
}

/// <summary>
/// Orchestrates the turn-based combat loop, team creation, skill execution, and victory or defeat resolution for battle scenes.
/// </summary>
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
    public LootPanel lootPanel;
    public bool IsReady { get; private set; }

    private List<Fighter> returnBuffer;
    public TurnsDisplay turnsDisplay;

    //Cambio statsManager a list para poder agregar elementos.
    public List<StatsManager> statsManagers;

    public AudioSource audioSource;
    public AudioSource sonidoDeDerrota;

    public Fighter CurrentFighter =>
        (fighters != null && fighterIndex >= 0 && fighterIndex < fighters.Length)
            ? fighters[fighterIndex]
            : null;


    /// <summary>
    /// Initializes the component once the scene dependencies are ready.
    /// </summary>
    void Start()
    {
        if (GetComponent<CombatScannerSystem>() == null)
            gameObject.AddComponent<CombatScannerSystem>();

        GameManager.Instance.SetGameState(GameManager.GameStates.BATTLE_STATE);


        if (isRadomEncounter == true)
        {
            EncuentrosAleatorios();
        }

        //AcÃ¡ instanciarÃ­a los player?
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

    /// <summary>
    /// Executes the encuentros aleatorios workflow.
    /// </summary>
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

    /// <summary>
    /// Executes the sort fighters by speed workflow.
    /// </summary>
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

    /// <summary>
    /// Executes the make teams workflow.
    /// </summary>
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
        /// <summary>
        /// Executes the if workflow.
        /// </summary>
        /// <param name="fgtr.team">The fgtr.team.</param>
        /// <returns>The resulting value.</returns>
            else if (fgtr.team == Team.ENEMIES)
            {
                enemiesBuffer.Add(fgtr);
            }

            fgtr.combatManager = this;
        }

        this.playerTeam = playersBuffer.ToArray();
        this.enemyTeam = enemiesBuffer.ToArray();
    }

    /// <summary>
    /// Executes the combat loop workflow.
    /// </summary>
    /// <returns>An enumerator that drives the coroutine sequence.</returns>
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
                    // Anunciar el uso de la habilidad
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
                    // Filtro de Mensajes: Solo si la habilidad es de tipo StatusModSkill, debe procesar los mensajes
                    if (this.currentFighterSkill is StatusModSkill)
                    {
                        while (true)
                        {
                            string nextMessage = this.currentFighterSkill.GetNextMessage();
                            if (nextMessage == null) break;
                            LogPanel.Write(nextMessage);
                        }
                    }

                    this.currentFighterSkill = null;
                    this.combatStatus = CombatStatus.CHECK_FOR_VICTORY;
                    yield return null;
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
                        // ── 1. Experience ──────────────────────────────────────────────
                        int totalExp = 0;
                        foreach (var enemy in enemyTeam)
                        {
                            if (enemy == null) continue;
                     
                            int enemyLevel  = enemy.stats.level;
                            int exp         = enemyLevel * 20;
                            int playerLevel = playerTeam[0].stats.level;
                     
                            if      (enemyLevel > playerLevel) exp = Mathf.FloorToInt(exp * 2f);
                            else if (enemyLevel < playerLevel) exp = Mathf.FloorToInt(exp * 0.6f);
                     
                            totalExp += exp;
                        }
                     
                        foreach (var fighter in playerTeam)
                        {
                            if (fighter is PlayerFighter pf)
                                pf.AddExperience(totalExp);
                        }
                     
                        // ── 2. Save player state ────────────────────────────────────────
                        if (playerTeam != null)
                        {
                            foreach (var f in playerTeam)
                            {
                                if (f is PlayerFighter pf)
                                    GameManager.Instance.SavePlayerState(pf);
                            }
                        }
                     
                        // ── 3. Victory audio + animation ───────────────────────────────
                        audioSource.Play();
                        Animator[] playerAnimators = player.GetComponentsInChildren<Animator>();
                        foreach (Animator anim in playerAnimators)
                            anim.Play("Victory");
                     
                        LogPanel.Write("Victory!");
                        this.isCombatActive = false;
                     
                        // ── 4. Resolve body-part loot from every defeated enemy ─────────
                        var allLoot = new System.Collections.Generic.List<BodyPartLootTable.LootEntry>();
                        foreach (var enemy in enemyTeam)
                        {
                            if (enemy == null) continue;
                            var resolver = enemy.GetComponent<LootResolver>();
                            if (resolver == null) continue;
                     
                            var drops = resolver.Resolve(enemy);
                            allLoot.AddRange(drops);
                        }
                     
                        // ── 5. Grant items to inventory ────────────────────────────────
                        if (InventoryManager.instance != null)
                        {
                            foreach (var entry in allLoot)
                                InventoryManager.instance.AddItem(entry.itemId, entry.amount, entry.uso);
                        }
                     
                        // ── 6. Show loot panel and WAIT for player input ────────────────
                        bool playerContinued = false;
                     
                        if (lootPanel != null)
                        {
                            lootPanel.OnContinue = () => playerContinued = true;
                            lootPanel.Show(allLoot);
                     
                            // Pause here until the player presses Continue
                            yield return new WaitUntil(() => playerContinued);
                        }
                        else
                        {
                            // No panel assigned: fall back to a short delay
                            yield return new WaitForSeconds(2f);
                        }
                     
                        // ── 7. Post-loot cleanup and scene transition ───────────────────
                        GameManager.Instance.SetGameState(GameManager.GameStates.TOWN_STATE);
                        GameManager.Instance.enemyToBattle.Clear();
                     
                        var realName = enemyTeam[0].GetComponent<EnemiesGroup>().GroupName;
                        ListEnemyDefeat.enemiesDefeat.Add(realName);
                        PlayerPrefs.SetString("GrupoEnemigo", realName);
                     
                        Debug.Log("Guardando enemigo derrotado REAL: " + realName);
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
                        SceneManager.LoadSceneAsync(5);
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
                    yield return new WaitForSeconds(0.1f);

                    Fighter current = null;

                    do
                    {
                        this.fighterIndex = (this.fighterIndex + 1) % this.fighters.Length;

                        current = this.fighters[this.fighterIndex];
                    } while (current.isAlive == false);

                    this.combatStatus = CombatStatus.CHECK_FIGHTER_STATUS_CONDITION;

    var currentFighter = this.fighters[this.fighterIndex];

    // 1. Aplicar condiciones de partes del cuerpo (ej: Sangrado)
    foreach (var bodyPartCondition in currentFighter.GetCurrentBodyPartStatusConditions().ToArray())
    {
        bodyPartCondition.Apply();

        while (true)
        {
            string nextBodyPartMessage = bodyPartCondition.GetNextMessage();
            if (nextBodyPartMessage == null) break;

            LogPanel.Write(nextBodyPartMessage);
        }
    }

    // 2. Aplicar condición de estado general (ej: Veneno)
    var statusCondition = currentFighter.GetCurrentStatusCondition();

    if (statusCondition != null)
    {
        statusCondition.Apply();

        while (true)
        {
            string nextSCMessage = statusCondition.GetNextMessage();
            if (nextSCMessage == null) break;

            LogPanel.Write(nextSCMessage);
        }

        // --- CAMBIO CLAVE: Verificar si murió por el daño de estado ---
        if (!currentFighter.isAlive)
        {
            this.combatStatus = CombatStatus.CHECK_FOR_VICTORY;
            yield return new WaitForSeconds(1f); // Tiempo para ver la animación de muerte
            break; 
        }
        // -------------------------------------------------------------

        if (statusCondition.BlocksTurn())
        {
            this.combatStatus = CombatStatus.CHECK_FOR_VICTORY;
            break;
        }
    }

    // 3. Solo iniciar turno si sigue con vida
    if (currentFighter.isAlive && currentFighter.gameObject.activeInHierarchy)
    {
        LogPanel.Write($"{currentFighter.idName} has the turn.");
        if (narrativeLogManager != null && currentFighter is EnemyFighter ef)
        {
            narrativeLogManager.EnemyTurn(ef);
        }
        
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
    /// <summary>
    /// Returns only the fighters that are currently alive from the provided team.
    /// </summary>
    /// <param name="team">The team.</param>
    /// <returns>The resulting collection.</returns>
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

    /// <summary>
    /// Returns the living opponents for the fighter whose turn is currently active.
    /// </summary>
    /// <returns>The resulting collection.</returns>
    public Fighter[] GetOpposingTeam()
    {
        Fighter currentFighter = this.fighters[this.fighterIndex];

        Fighter[] team = null;
        if (currentFighter.team == Team.PLAYERS)
        {
            team = this.enemyTeam;
        }
        /// <summary>
        /// Executes the if workflow.
        /// </summary>
        /// <param name="currentFighter.team">The current fighter.team.</param>
        /// <returns>The resulting value.</returns>
        else if (currentFighter.team == Team.ENEMIES)
        {
            team = this.playerTeam;
        }

        return this.FilterJustAlive(team);
    }

    /// <summary>
    /// Returns the living allies for the fighter whose turn is currently active.
    /// </summary>
    /// <returns>The resulting collection.</returns>
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

    /// <summary>
    /// Queues the selected skill as the next combat action to execute in the battle loop.
    /// </summary>
    /// <param name="skill">The skill.</param>
    public void OnFighterSkill(Skill skill)
    {
        this.currentFighterSkill = skill;
        this.combatStatus = CombatStatus.FIGHTER_ACTION;

        // Limpiar el foco del EventSystem para evitar que los botones mantengan el estado de "highlighted" o "selected"
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }

        // Ocultar la tooltip explícitamente al iniciar la acción
        Tooltip.HideTooltip_static();
    }

    /// <summary>
    /// Executes the define stats manager workflow.
    /// </summary>
    private void DefineStatsManager()
    {
        foreach(Fighter fighter in fighters)
        {
            statsManagers.Add(fighter.GetComponent<StatsManager>());
        }
    }

    /// <summary>
    /// Refreshes every registered combat stats panel after gameplay state changes.
    /// </summary>
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

    /// <summary>
    /// Spawns the selected party members for the battle scene and wires their UI references.
    /// </summary>
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
        /// <summary>
        /// Executes the if workflow.
        /// </summary>
        /// <param name="GameManager.Instance.hasRecruitedSecondary">The game manager.instance.has recruited secondary.</param>
        /// <returns>The resulting value.</returns>
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
