using UnityEngine;
using Cinemachine;
using System.Collections.Generic;

/// <summary>
/// Cerebro central del sistema de cámaras. Orquesta las transiciones de estado,
/// sincroniza cámaras técnicas y reacciona a los eventos del combate.
/// </summary>
public class CameraDirector : MonoBehaviour
{
    public static CameraDirector Instance { get; private set; }

    [Header("Cinemachine Cameras")]
    [SerializeField] private CinemachineVirtualCamera playerOverviewCam;
    [SerializeField] private CinemachineVirtualCamera enemyOverviewCam;
    [SerializeField] private CinemachineVirtualCamera actionCam;
    [SerializeField] private CinemachineVirtualCamera diegeticUiCam;
    [SerializeField] private Vector3 diegeticUiFollowOffset = new Vector3(0, 0, -1.5f);

    [SerializeField] private CinemachineVirtualCamera skillPanelCam;
    [SerializeField] private Vector3 skillPanelFollowOffset = new Vector3(0, 0.5f, -1.2f);

    [Header("Technical Cameras")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Camera shaderCamera;

    [Header("Sub-Systems")]
    [SerializeField] private CameraTargetSystem targetSystem;
    [SerializeField] private CameraFXManager fxManager;

    [Header("Panic Settings")]
    [Tooltip("Umbral de vida (0-1) bajo el cual se activa el efecto de respiración agitada.")]
    [SerializeField] private float lowHealthThreshold = 0.4f;

    [Header("Overview Look Settings")]
    [SerializeField] private float enemyLookBlend = 0.45f;
    [SerializeField] private Transform overviewLookTarget;
    
    // Constantes de prioridad para evitar números mágicos
    private const int PriorityActive = 50;
    private const int PriorityInactive = 10;

    public CameraState CurrentState => _currentState;
    private CameraState _stateBeforeUi = CameraState.Overview;
    public CameraState StateBeforeUi => _stateBeforeUi;

    private List<CinemachineVirtualCamera> _allVirtualCameras = new List<CinemachineVirtualCamera>();
    private CameraState _currentState;
    private CombatManager _combatManager;

    private ICinemachineCamera _cachedVCam;
    private CinemachineCameraModifier _cachedModifier;

    private Transform _currentPlayerTransform;
    private Transform _currentEnemyTransform;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        RegisterCameras();
    }

    private void RegisterCameras()
    {
        if (playerOverviewCam) _allVirtualCameras.Add(playerOverviewCam);
        if (enemyOverviewCam) _allVirtualCameras.Add(enemyOverviewCam);
        if (actionCam) _allVirtualCameras.Add(actionCam);
        if (diegeticUiCam) _allVirtualCameras.Add(diegeticUiCam);
        if (skillPanelCam) _allVirtualCameras.Add(skillPanelCam);
        if (targetSystem && targetSystem.GroupActionCam) _allVirtualCameras.Add(targetSystem.GroupActionCam);
    }

    private void Start()
    {
        _combatManager = FindObjectOfType<CombatManager>();
        
        if (_combatManager != null)
        {
            // Suscripción a eventos del CombatManager (Deberán ser implementados en CombatManager)
            _combatManager.OnTurnStarted += HandleTurnStarted;
            _combatManager.OnActionExecuted += HandleActionExecuted;
            _combatManager.OnSkillMenuOpened += () => ChangeState(CameraState.Ui);
            _combatManager.OnSkillMenuClosed += () => ChangeState(CameraState.Overview);
        }

        // Estado inicial
        ChangeState(CameraState.Overview);
    }

    private void LateUpdate()
    {
        // Sincronización limpia de FOV para cámaras de efectos/shaders
        if (mainCamera != null && shaderCamera != null)
        {
            shaderCamera.fieldOfView = mainCamera.fieldOfView;
        }

        // Actualización dinámica del factor de pánico basado en el estado del combatiente actual
        UpdateModifierPanicFactor();

        UpdateOverviewLookTarget();
    }

    private void UpdateModifierPanicFactor()
    {
        if (_combatManager == null || _combatManager.CurrentFighter == null) return;

        Fighter current = _combatManager.CurrentFighter;
        float panic = 0f;

        // Solo aplicamos pánico si es turno de un aliado (según lógica original)
        if (current.team == Team.PLAYERS)
        {
            var stats = current.GetCurrentStats();
            if (stats != null)
            {
                float hpPercent = stats.health / Mathf.Max(1f, stats.maxHealth);
                if (hpPercent <= lowHealthThreshold)
                {
                    // El pánico aumenta a medida que la vida baja del umbral
                    panic = 1f - (hpPercent / lowHealthThreshold);
                }
            }
        }

        // Inyectamos el factor en el modificador de la cámara que esté activa en el Brain
        CinemachineBrain brain = mainCamera.GetComponent<CinemachineBrain>();
        if (brain != null)
        {
            ICinemachineCamera activeVCam = brain.ActiveVirtualCamera;
            if (activeVCam != _cachedVCam)
            {
                _cachedVCam = activeVCam;
                _cachedModifier = activeVCam?.VirtualCameraGameObject.GetComponent<CinemachineCameraModifier>();
            }

            if (_cachedModifier != null)
            {
                _cachedModifier.panicFactor = panic;
            }
        }
    }

    /// <summary>
    /// Cambia el estado de la cámara gestionando las prioridades de Cinemachine.
    /// </summary>
    public void ChangeState(CameraState newState)
    {
        if (newState == CameraState.Ui || newState == CameraState.SkillPanel)
            _stateBeforeUi = _currentState;

        var brain = mainCamera != null ? mainCamera.GetComponent<CinemachineBrain>() : null;
        CinemachineCameraModifier mod = null;
        if (brain != null)
        {
            mod = brain.ActiveVirtualCamera?.VirtualCameraGameObject.GetComponent<CinemachineCameraModifier>();
        }

        _currentState = newState;
        ResetAllPriorities();

        switch (newState)
        {
            case CameraState.Overview:
                bool isPlayerTurn = _combatManager != null && _combatManager.CurrentFighter != null && 
                                   _combatManager.CurrentFighter.team == Team.PLAYERS;
                SetCameraPriority(isPlayerTurn ? playerOverviewCam : enemyOverviewCam, PriorityActive);
                if (mod != null) mod.enabled = true;
                break;

            case CameraState.Action:
                // Si el sistema de targets está activo, le damos prioridad a su cámara
                if (targetSystem != null && targetSystem.GroupActionCam != null)
                    SetCameraPriority(targetSystem.GroupActionCam, PriorityActive);
                else
                    SetCameraPriority(actionCam, PriorityActive);
                if (mod != null) mod.enabled = true;
                break;

            case CameraState.Ui:
                SetCameraPriority(diegeticUiCam, PriorityActive);
                if (mod != null) mod.enabled = false;
                break;

            case CameraState.SkillPanel:
                SetCameraPriority(skillPanelCam, PriorityActive);
                if (mod != null) mod.enabled = false;
                break;

            case CameraState.HitReaction:
                // Generalmente se mantiene la cámara de acción pero el FXManager dispara el shake
                if (mod != null) mod.enabled = true;
                break;
            
            case CameraState.Cinematic:
                if (mod != null) mod.enabled = true;
                break;
        }
    }

    private void ResetAllPriorities()
    {
        foreach (var cam in _allVirtualCameras)
        {
            cam.m_Priority = PriorityInactive;
        }
    }

    private void SetCameraPriority(CinemachineVirtualCamera cam, int priority)
    {
        if (cam != null) cam.m_Priority = priority;
    }

    /// <summary>
    /// Maneja el zoom visual al pasar el mouse sobre un enemigo sin alterar el flujo del turno.
    /// </summary>
    public void SetSelectionZoom(bool active, Fighter target)
    {
        if (fxManager != null)
        {
            fxManager.SetHoverDistortion(active);
        }

        if (active && target != null)
        {
            _currentEnemyTransform = target.transform;
            if (overviewLookTarget != null && _currentPlayerTransform != null)
            {
                overviewLookTarget.position = Vector3.Lerp(_currentPlayerTransform.position, _currentEnemyTransform.position, enemyLookBlend);
            }
        }
        else
        {
            _currentEnemyTransform = null;
            if (overviewLookTarget != null && _currentPlayerTransform != null)
            {
                overviewLookTarget.position = _currentPlayerTransform.position;
            }
        }
    }

    private void UpdateOverviewLookTarget()
    {
        if (_currentEnemyTransform != null && _currentPlayerTransform != null && overviewLookTarget != null)
        {
            overviewLookTarget.position = Vector3.Lerp(_currentPlayerTransform.position, _currentEnemyTransform.position, enemyLookBlend);
        }
    }

    /// <summary>
    /// Reasigna los objetivos Follow y LookAt de la cámara diegética de UI hacia el
    /// ancla del personaje recibido, logrando que la cámara se desplace físicamente
    /// de un monitor (espalda de un personaje) a otro en el entorno 3D.
    /// </summary>
    /// <param name="fighter">Personaje que pasa a estar seleccionado en el panel de estado.</param>
    public void FocusDiegeticUiOn(Fighter fighter)
    {
        if (diegeticUiCam == null || fighter == null) return;

        // Priorizamos el nuevo ancla específica para cámara, luego el uiAnchor, y finalmente el transform base.
        Transform anchor = fighter.diegeticCamAnchor != null ? fighter.diegeticCamAnchor : 
                         (fighter.uiAnchor != null ? fighter.uiAnchor : fighter.transform);

        diegeticUiCam.m_Follow = anchor;
        diegeticUiCam.m_LookAt = anchor;

        // Ajustamos el offset del Transposer para asegurar una distancia correcta al monitor.
        var transposer = diegeticUiCam.GetCinemachineComponent<CinemachineTransposer>();
        if (transposer != null)
        {
            transposer.m_FollowOffset = diegeticUiFollowOffset;
        }

        // Aseguramos que la cámara de UI esté activa al enfocar un nuevo monitor.
        if (_currentState != CameraState.Ui)
            ChangeState(CameraState.Ui);
    }

    /// <summary>
    /// Enfoca la cámara diegética específica para el panel de habilidades en el ancla del personaje.
    /// </summary>
    /// <param name="fighter">Personaje activo.</param>
    public void FocusSkillPanelOn(Fighter fighter)
    {
        if (skillPanelCam == null || fighter == null) return;

        // Priorizamos el nuevo ancla específica para cámara, luego el uiAnchor, y finalmente el transform base.
        Transform anchor = fighter.diegeticCamAnchor != null ? fighter.diegeticCamAnchor : 
                         (fighter.uiAnchor != null ? fighter.uiAnchor : fighter.transform);

        skillPanelCam.m_Follow = anchor;
        skillPanelCam.m_LookAt = anchor;

        var transposer = skillPanelCam.GetCinemachineComponent<CinemachineTransposer>();
        if (transposer != null)
        {
            transposer.m_FollowOffset = skillPanelFollowOffset;
        }

        if (_currentState != CameraState.SkillPanel)
            ChangeState(CameraState.SkillPanel);
    }

    private void HandleTurnStarted(Fighter fighter)
    {
        // Cuando empieza un turno, volvemos a Overview y enfocamos al luchador actual
        if (fighter != null)
        {
            if (fighter.team == Team.PLAYERS && playerOverviewCam != null)
            {
                _currentPlayerTransform = fighter.transform;
                playerOverviewCam.m_Follow = fighter.CameraPivot != null ? fighter.CameraPivot : fighter.transform;
                playerOverviewCam.m_LookAt = overviewLookTarget != null ? overviewLookTarget : fighter.transform;
            }
            else if (enemyOverviewCam != null)
            {
                enemyOverviewCam.m_Follow = fighter.CameraPivot != null ? fighter.CameraPivot : fighter.transform;
                enemyOverviewCam.m_LookAt = fighter.transform;
            }
        }
        
        ChangeState(CameraState.Overview);
    }

    private void HandleActionExecuted(Fighter attacker, Fighter defender)
    {
        // Configuramos el encuadre dinámico entre ambos combatientes
        if (targetSystem != null && attacker != null && defender != null)
        {
            targetSystem.SetupDynamicFraming(attacker.transform, defender.transform);
            ChangeState(CameraState.Action);
        }
    }
    
    private void OnDestroy()
    {
        // Unsubscribe para evitar memory leaks
        if (_combatManager != null)
        {
            _combatManager.OnTurnStarted -= HandleTurnStarted;
            _combatManager.OnActionExecuted -= HandleActionExecuted;
        }
    }
}
