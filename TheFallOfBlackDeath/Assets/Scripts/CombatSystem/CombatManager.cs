
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
    private readonly Queue<IEnumerator> pendingReactions = new Queue<IEnumerator>();
    public int enemyAmount;
    //SPAWN POINTS
    public bool isRadomEncounter = false;
    public List<Transform> spawnPoints = new List<Transform>();
    public BodyPartPanel bodyPartPanel;
    public LootPanel lootPanel;
    public bool IsReady { get; private set; }

    [Header("Escape")]
    [SerializeField, Range(0f, 1f)] private float baseEscapeChance = 0.65f;
    [SerializeField] private float speedEscapeChancePerPoint = 0.03f;
    [SerializeField] private float brokenLegEscapePenalty = 0.25f;
    [SerializeField] private float escapeResultDelay = 0.6f;
    [SerializeField] private float escapedEnemyStunDuration = 4f;
    [SerializeField] private int explorationSceneIndex = 1;
    [SerializeField] private UnityEngine.Rendering.Volume postProcessVolume;
    [SerializeField] private float glitchDuration = 0.2f;
    private URPGlitch.AnalogGlitchVolume analogGlitch;
    private URPGlitch.DigitalGlitchVolume digitalGlitch;
    private bool escapeInProgress;

    // --- EVENTOS PARA EL SISTEMA DE CÁMARAS ---
    public event System.Action<Fighter> OnTurnStarted;
    public event System.Action<Fighter, Fighter> OnActionExecuted;
    public event System.Action OnSkillMenuOpened;
    public event System.Action OnSkillMenuClosed;

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

    public bool IsProcessingReaction { get; private set; }


    /// <summary>
    /// Initializes the component once the scene dependencies are ready.
    /// </summary>
    void Start()
    {
        CursorManager.Instance?.RequestCursor(this);

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

        SetupGlitchComponents();

        StartCoroutine(this.CombatLoop());
    }

    private void SetupGlitchComponents()
    {
        if (postProcessVolume == null)
        {
            postProcessVolume = FindObjectOfType<UnityEngine.Rendering.Volume>();
        }

        if (postProcessVolume != null && postProcessVolume.profile != null)
        {
            postProcessVolume.profile.TryGet(out analogGlitch);
            postProcessVolume.profile.TryGet(out digitalGlitch);
        }
    }

    /// <summary>
    /// Executes the encuentros aleatorios workflow.
    /// </summary>
    public void EncuentrosAleatorios()
    {
        CursorManager.Instance?.RequestCursor(this);

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
                    // Si ya no quedan enemigos o jugadores vivos tras una reacción/contraataque o estado, resolver victoria/derrota de inmediato
                    if (!AreEnemiesAlive() || !ArePlayersAlive())
                    {
                        if (skillPanel != null)
                            skillPanel.Hide();
                        this.combatStatus = CombatStatus.CHECK_FOR_VICTORY;
                        break;
                    }
                    yield return null;
                    break;

                case CombatStatus.FIGHTER_ACTION:
                    // Anunciar el uso de la habilidad
                    LogPanel.Write($"{this.fighters[this.fighterIndex].idName} uses {currentFighterSkill.skillName}.");

                    yield return null;

                    // Executing fighter skill
                    OnActionExecuted?.Invoke(this.fighters[this.fighterIndex], currentFighterSkill.MainTarget);
                    currentFighterSkill.Run();

                    // Esperar a que las corrutinas de la habilidad (VFX, QTE, Parry, contraataque, etc.) finalicen
                    while (currentFighterSkill != null && currentFighterSkill.IsRunning)
                    {
                        yield return null;
                    }

                    // Wait for fighter skill animation
                    if (currentFighterSkill != null && currentFighterSkill.actionDelay > 0f)
                    {
                        yield return new WaitForSeconds(currentFighterSkill.actionDelay);
                    }

                    yield return StartCoroutine(ResolvePendingReactions());

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
                    if (skillPanel != null)
                        skillPanel.Hide();

                    bool arePlayersAlive = ArePlayersAlive();
                    bool areEnemiesAlive = AreEnemiesAlive();

                    bool victory = areEnemiesAlive == false;
                    bool defeat = arePlayersAlive == false;

                    // [PERMADEATH FEATURE] Insta-Game Over if Main Character dies
                    bool isMainCharacterAlive = true;
                    if (globalDataBase != null && playerTeam.Length > 0)
                    {
                        // Validamos si el índice 0 o cualquiera con el flag isMainCharacter ha muerto
                        foreach (var fighter in playerTeam)
                        {
                            if (fighter is PlayerFighter pf)
                            {
                                if (pf.figherIndex >= 0 && pf.figherIndex < globalDataBase.EnemyDB.Count)
                                {
                                    if (globalDataBase.EnemyDB[pf.figherIndex].isMainCharacter && !pf.isAlive)
                                    {
                                        isMainCharacterAlive = false;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    else if (playerTeam.Length > 0 && !playerTeam[0].isAlive)
                    {
                        // Fallback si no hay DB: asumimos el índice 0 como MC
                        isMainCharacterAlive = false;
                    }

                    if (!isMainCharacterAlive) defeat = true;


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
                        // [PERMADEATH FEATURE] Process permanent deaths before saving
                        if (GameManager.Instance != null && playerTeam != null)
                        {
                            GameManager.Instance.ProcessPermanentDeaths(playerTeam);
                        }

                        if (playerTeam != null)
                        {
                            foreach (var f in playerTeam)
                            {
                                // [PERMADEATH FEATURE] Save only if alive
                                if (f is PlayerFighter pf && pf.isAlive)
                                    GameManager.Instance.SavePlayerState(pf);
                            }
                        }
                     
                        // ── 3. Victory audio + animation ───────────────────────────────
                        if (AudioManager.Instance != null && audioSource != null)
                        {
                            AudioManager.Instance.PlaySFX(audioSource.clip, audioSource.volume, false);
                        }
                        else if (audioSource != null)
                        {
                            audioSource.Play();
                        }
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
                     
                        // ── 5. Grant items to inventory (NEW SYSTEM ONLY) ──────────────
                        // The legacy InventoryManager path has been removed.
                        
                        if (InventoryNew.NewInventoryManager.Instance != null)
                        {
                            foreach (var entry in allLoot)
                            {
                                if (entry.newItemData == null)
                                {
                                    Debug.LogWarning("Loot entry without NewItemData was skipped. Please assign a NewItemData in the BodyPartLootTable.");
                                    continue;
                                }
 
                                InventoryNew.NewInventoryManager.Instance.AddItem(entry.newItemData, entry.amount);
 
                                // ── NOTIFICATION (queued — shown when world scene loads) ──
                                if (ItemNotificationManager.Instance != null)
                                {
                                    ItemNotificationManager.Instance.NotifyLoot(entry.newItemData, entry.amount);
                                }
                                else
                                {
                                    Debug.LogWarning($"[CombatManager] No se pudo notificar loot de '{entry.newItemData.itemName}' porque ItemNotificationManager.Instance es null.");
                                }
                                // ──────────────────────────────────────────────────────────
                            }
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
                        CursorManager.Instance?.ReleaseCursor(this);
                        GameManager.Instance.SetGameState(GameManager.GameStates.TOWN_STATE);
                        GameManager.Instance.enemyToBattle.Clear();
                     
                        var group = enemyTeam[0].GetComponent<EnemiesGroup>();
                        if (group != null)
                        {
                            GameManager.Instance.RegisterDefeatedEnemyGroup(group.GroupName);
                            Debug.Log("Registrando enemigo derrotado en memoria: " + group.GroupName);
                        }
                        else
                        {
                            Debug.LogWarning("[CombatManager] No se encontró EnemiesGroup en el equipo enemigo.");
                        }
                        SceneManager.LoadScene(GameManager.Instance.LastExplorationSceneIndex);
                    }

                    if (defeat)
                    {
                        if (GameManager.Instance != null)
                        {
                            GameManager.Instance.RegisterPartyDefeat(playerTeam);
                        }

                        LogPanel.Write("Defeat!");
                        this.isCombatActive = false;
                        yield return new WaitForSeconds(2f);
                        SceneManager.LoadSceneAsync(5);
                        
                        if (AudioManager.Instance != null && sonidoDeDerrota != null)
                        {
                            AudioManager.Instance.PlaySFX(sonidoDeDerrota.clip, sonidoDeDerrota.volume, false);
                        }
                        else if (sonidoDeDerrota != null)
                        {
                            sonidoDeDerrota.Play();
                        }
                    }

                    if (this.isCombatActive)
                    {
                        this.combatStatus = CombatStatus.NEXT_TURN;
                    }

                    yield return null;
                    break;
                case CombatStatus.NEXT_TURN:
                    if (!AreEnemiesAlive() || !ArePlayersAlive())
                    {
                        if (skillPanel != null)
                            skillPanel.Hide();
                        this.combatStatus = CombatStatus.CHECK_FOR_VICTORY;
                        break;
                    }

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
        OnTurnStarted?.Invoke(currentFighter);
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
    public void InvokeOnSkillMenuOpened() => OnSkillMenuOpened?.Invoke();
    public void InvokeOnSkillMenuClosed() => OnSkillMenuClosed?.Invoke();

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
    /// Retorna true si hay al menos un combatiente del equipo de jugadores con vida.
    /// </summary>
    public bool ArePlayersAlive()
    {
        if (this.playerTeam == null || this.playerTeam.Length == 0) return false;
        foreach (var fighter in this.playerTeam)
        {
            if (fighter != null && fighter.isAlive)
                return true;
        }
        return false;
    }

    /// <summary>
    /// Retorna true si hay al menos un combatiente del equipo enemigo con vida.
    /// </summary>
    public bool AreEnemiesAlive()
    {
        if (this.enemyTeam == null || this.enemyTeam.Length == 0) return false;
        foreach (var fighter in this.enemyTeam)
        {
            if (fighter != null && fighter.isAlive)
                return true;
        }
        return false;
    }

    public void EnqueueReaction(IEnumerator reaction)
    {
        if (reaction == null || !isCombatActive)
            return;

        pendingReactions.Enqueue(reaction);
    }

    /// <summary>
    /// Dispara inmediatamente la rutina de contraataque cuando se ejecuta un Parry exitoso.
    /// </summary>
    /// <param name="defender">El combatiente que realizó el Parry y ejecutará el contraataque.</param>
    /// <param name="attacker">El atacante original que recibirá el contraataque.</param>
    /// <param name="counterSkill">Habilidad específica opcional; si es null, usará el ataque básico del defensor.</param>
    public Coroutine TriggerCounterAttack(Fighter defender, Fighter attacker, Skill counterSkill = null)
    {
        if (defender == null || attacker == null || !isCombatActive)
            return null;

        return StartCoroutine(ExecuteCounterAttackRoutine(defender, attacker, counterSkill));
    }

    /// <summary>
    /// Corrutina que ejecuta el flujo del contraataque de forma inmediata con cálculo de daño, mensajes de log y efectos de cámara.
    /// </summary>
    public IEnumerator ExecuteCounterAttackRoutine(Fighter defender, Fighter attacker, Skill counterSkill = null)
    {
        if (defender == null || !defender.isAlive || attacker == null || !attacker.isAlive || !isCombatActive)
            yield break;

        LogPanel.Write($"¡PARRY! {defender.idName} ejecuta un Contraataque inmediato contra {attacker.idName}.");

        // Jugosidad e impacto visual
        if (CameraManager.Instance != null)
        {
            CameraManager.Instance.TriggerHitStop(0.08f);
            CameraManager.Instance.TriggerShake(1.2f);
        }

        yield return new WaitForSeconds(0.1f);

        Skill skillToUse = counterSkill;
        if (skillToUse == null && defender.skills != null && defender.skills.Length > 0)
        {
            // Usamos la primera habilidad utilizable (generalmente ataque básico)
            foreach (Skill s in defender.skills)
            {
                if (s != null && s.IsUsable(defender))
                {
                    skillToUse = s;
                    break;
                }
            }
        }

        if (skillToUse != null)
        {
            skillToUse.SetEmitter(defender);
            skillToUse.AddReceiver(attacker);
            skillToUse.Run(resolveBodyPartTargetOnRun: true);

            while (skillToUse.IsRunning)
            {
                yield return null;
            }

            float delay = Mathf.Max(0.35f, skillToUse.actionDelay);
            yield return new WaitForSeconds(delay);

            while (true)
            {
                string nextMessage = skillToUse.GetNextMessage();
                if (nextMessage == null) break;
                LogPanel.Write(nextMessage);
            }
        }
        else
        {
            // Fallback numérico mediante StandardDamageCalculator si no hay componente Skill disponible
            StandardDamageCalculator calculator = new StandardDamageCalculator();
            float baseAtk = defender.GetCurrentStats().attack;
            DamageCalculationContext ctx = new DamageCalculationContext(
                defender,
                attacker,
                null,
                BodyPart.Torso,
                -baseAtk,
                HealthModType.STAT_BASED,
                DamageType.Kinetic,
                PartStatus.None,
                0.15f,
                50f,
                0.40f,
                0f,
                true);

            DamageResult dmgResult = calculator.Calculate(ctx);
            attacker.ModifyHealth(dmgResult);
            LogPanel.Write($"{defender.idName} contraataca causando {-dmgResult.appliedAmount} de daño a {attacker.idName}.");

            yield return new WaitForSeconds(0.4f);
        }

        UpdateStatsUI();
    }

    private IEnumerator ResolvePendingReactions()
    {
        while (pendingReactions.Count > 0 && isCombatActive)
        {
            IEnumerator reaction = pendingReactions.Dequeue();

            IsProcessingReaction = true;
            yield return StartCoroutine(reaction);
            IsProcessingReaction = false;

            UpdateStatsUI();
            yield return null;
        }

        IsProcessingReaction = false;
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

    public void TryRunFromCombat(PlayerFighter runner)
    {
        if (escapeInProgress || runner == null || !isCombatActive)
            return;

        if (CurrentFighter != runner || combatStatus != CombatStatus.WAITING_FOR_FIGHTER)
        {
            Debug.LogWarning("[CombatManager.TryRunFromCombat] RUN can only be used by the active fighter.");
            return;
        }

        StartCoroutine(RunFromCombatRoutine(runner));
    }

    private IEnumerator RunFromCombatRoutine(PlayerFighter runner)
    {
        escapeInProgress = true;

        if (skillPanel != null)
            skillPanel.Hide();

        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);

        Tooltip.HideTooltip_static();

        float escapeChance = CalculateEscapeChance(runner);
        LogPanel.Write($"{runner.idName} tries to run.");

        yield return new WaitForSeconds(escapeResultDelay);

        bool escaped = Random.value <= escapeChance;
        if (escaped)
        {
            LogPanel.Write("Escaped!");
            
            // [PERMADEATH FEATURE] Process permanent deaths before saving on escape
            if (GameManager.Instance != null && playerTeam != null)
            {
                GameManager.Instance.ProcessPermanentDeaths(playerTeam);
            }

            SavePlayerTeamState();

            if (GameManager.Instance != null)
            {
                GameManager.Instance.RegisterEscapedEncounter(GetCurrentEnemyGroupName(), escapedEnemyStunDuration);
                GameManager.Instance.SetGameState(GameManager.GameStates.TOWN_STATE);
                GameManager.Instance.enemyToBattle.Clear();
            }

            yield return StartCoroutine(ApplyEscapeGlitch());

            isCombatActive = false;
            CursorManager.Instance?.ReleaseCursor(this);
            yield return new WaitForSeconds(escapeResultDelay);
            SceneManager.LoadScene(GameManager.Instance.LastExplorationSceneIndex);
            yield break;
        }

        LogPanel.Write("Could not escape!");
        combatStatus = CombatStatus.CHECK_FOR_VICTORY;
        escapeInProgress = false;
    }

    private IEnumerator ApplyEscapeGlitch()
    {
        Time.timeScale = 0.1f;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;

        if (analogGlitch != null && digitalGlitch != null)
        {
            analogGlitch.active = true;
            digitalGlitch.active = true;

            analogGlitch.scanLineJitter.Override(0.2f);
            analogGlitch.colorDrift.Override(0.4f);
            analogGlitch.horizontalShake.Override(0.2f);
            digitalGlitch.intensity.Override(0.2f);
        }

        yield return new WaitForSecondsRealtime(glitchDuration);

        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;
    }

    private float CalculateEscapeChance(PlayerFighter runner)
    {
        float runnerSpeed = runner.GetCurrentStats().speed;
        float fastestEnemySpeed = 0f;

        if (enemyTeam != null)
        {
            foreach (var enemy in enemyTeam)
            {
                if (enemy != null && enemy.isAlive)
                    fastestEnemySpeed = Mathf.Max(fastestEnemySpeed, enemy.GetCurrentStats().speed);
            }
        }

        float chance = baseEscapeChance + ((runnerSpeed - fastestEnemySpeed) * speedEscapeChancePerPoint);
        if (runner.legBroken)
            chance -= brokenLegEscapePenalty;

        return Mathf.Clamp01(chance);
    }

    private void SavePlayerTeamState()
    {
        if (GameManager.Instance == null || playerTeam == null)
            return;

        foreach (var fighter in playerTeam)
        {
            if (fighter is PlayerFighter playerFighter)
            {
                // [PERMADEATH FEATURE] Save only if alive
                if (playerFighter.isAlive)
                    GameManager.Instance.SavePlayerState(playerFighter);
            }
        }
    }

    private string GetCurrentEnemyGroupName()
    {
        if (GameManager.Instance != null)
        {
            string currentEncounterGroup = GameManager.Instance.GetCurrentEncounterGroupName();
            if (!string.IsNullOrEmpty(currentEncounterGroup))
                return currentEncounterGroup;
        }

        if (!string.IsNullOrEmpty(groupEnemyName))
            return groupEnemyName;

        if (enemyTeam == null)
            return string.Empty;

        foreach (var enemy in enemyTeam)
        {
            if (enemy == null) continue;

            EnemiesGroup group = enemy.GetComponentInParent<EnemiesGroup>();
            if (group == null)
                group = enemy.GetComponentInChildren<EnemiesGroup>();

            if (group != null && !string.IsNullOrEmpty(group.GroupName))
                return group.GroupName;
        }

        return string.Empty;
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

    [Header("Positions")]
    public List<Transform> playerSpawnPoints = new List<Transform>();
    public List<Transform> enemySpawnPoints = new List<Transform>();

    /// <summary>
    /// Spawns the selected party members for the battle scene and wires their UI references.
    /// </summary>
    private void InstantiatePlayerFighters()
    {
        if (globalDataBase == null)
        {
            Debug.LogError("[CombatManager] globalDataBase no está asignada.");
            return;
        }

        int spawnIdx = 0;
        var uiController = FindObjectOfType<StatusPanelController>();

        if (InstantiateActivePartyFromGameManager(uiController))
        {
            return;
        }

        // Usar la lista de reclutados de la DB para instanciar a los personajes activos
        for (int i = 0; i < globalDataBase.EnemyDB.Count; i++)
        {
            var dbEntry = globalDataBase.EnemyDB[i];
            
            // Un personaje pelea si es Main o si es Secondary Y está marcado como tal en la DB
            // (La DB actúa como el estado persistente de quién está en la party)
            if (dbEntry.isMainCharacter || dbEntry.isSecondaryCharacter)
            {
                Transform spawnPoint = null;
                if (spawnIdx < playerSpawnPoints.Count && playerSpawnPoints[spawnIdx] != null)
                {
                    spawnPoint = playerSpawnPoints[spawnIdx];
                }
                else
                {
                    Debug.LogWarning($"[CombatManager] No hay suficientes spawn points para el jugador {dbEntry.Name}. Usando fallback.");
                    spawnPoint = mainCharacterPos;
                }

                GameObject characterGO = Instantiate(
                    dbEntry.enemyPrefab,
                    spawnPoint.position,
                    Quaternion.Euler(-0.4f, -90, 0),
                    playerParent.transform
                );

                var playerFighter = characterGO.GetComponent<PlayerFighter>();

                playerFighter.GetSkillPanel(
                    skillPanel,
                    playerFighter.statusPanel,
                    enemiesPanel,
                    bodyPartPanel
                );

                // Registrar en el GameManager según sea main o secondary
                if (dbEntry.isMainCharacter)
                {
                    GameManager.Instance.SetMainCharacter(playerFighter);
                }
                else
                {
                    // Esto ahora lo añade a la lista activeParty internamente
                    GameManager.Instance.RegisterPartyMember(playerFighter);
                }

                GameManager.Instance.ApplySavedStatusToFighter(playerFighter);
                
                if (uiController != null)
                {
                    // uiController.RegisterPlayer(playerFighter);
                }

                spawnIdx++;
            }
        }
    }

    private bool InstantiateActivePartyFromGameManager(StatusPanelController uiController)
    {
        if (GameManager.Instance == null) return false;

        var activePartyIds = GameManager.Instance.GetActivePartyIds();
        if (activePartyIds == null || activePartyIds.Count == 0) return false;

        int spawnIdx = 0;
        foreach (int partyId in activePartyIds)
        {
            if (partyId < 0 || partyId >= globalDataBase.EnemyDB.Count) continue;

            var dbEntry = globalDataBase.EnemyDB[partyId];
            if (dbEntry.enemyPrefab == null)
            {
                Debug.LogWarning($"[CombatManager] El party member {dbEntry.Name} no tiene prefab asignado.");
                continue;
            }

            GameObject characterGO = Instantiate(
                dbEntry.enemyPrefab,
                GetPlayerSpawnPosition(spawnIdx, dbEntry.Name),
                Quaternion.Euler(-0.4f, -90, 0),
                playerParent.transform
            );

            var playerFighter = characterGO.GetComponent<PlayerFighter>();
            if (playerFighter == null)
            {
                Debug.LogWarning($"[CombatManager] El prefab de {dbEntry.Name} no tiene PlayerFighter.");
                continue;
            }

            playerFighter.GetSkillPanel(
                skillPanel,
                playerFighter.statusPanel,
                enemiesPanel,
                bodyPartPanel
            );

            if (spawnIdx == 0 || dbEntry.isMainCharacter)
            {
                GameManager.Instance.SetMainCharacter(playerFighter);
            }
            else
            {
                GameManager.Instance.RegisterPartyMember(playerFighter);
            }

            GameManager.Instance.ApplySavedStatusToFighter(playerFighter);

            if (uiController != null)
            {
                // uiController.RegisterPlayer(playerFighter);
            }

            spawnIdx++;
        }

        return spawnIdx > 0;
    }

    private Vector3 GetPlayerSpawnPosition(int spawnIdx, string fighterName)
    {
        if (spawnIdx < playerSpawnPoints.Count && playerSpawnPoints[spawnIdx] != null)
        {
            return playerSpawnPoints[spawnIdx].position;
        }

        if (spawnIdx == 0 && mainCharacterPos != null)
        {
            return mainCharacterPos.position;
        }

        if (spawnIdx == 1 && secondaryCharacterPos != null)
        {
            return secondaryCharacterPos.position;
        }

        Debug.LogWarning($"[CombatManager] No hay spawn point para {fighterName}. Usando fallback con offset.");
        Vector3 basePosition = mainCharacterPos != null ? mainCharacterPos.position : transform.position;
        return basePosition + Vector3.right * (1.5f * spawnIdx);
    }

}
