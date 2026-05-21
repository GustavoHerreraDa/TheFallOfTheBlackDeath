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

    [Header("Technical Cameras")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Camera shaderCamera;

    [Header("Sub-Systems")]
    [SerializeField] private CameraTargetSystem targetSystem;
    [SerializeField] private CameraFXManager fxManager;

    [Header("Panic Settings")]
    [Tooltip("Umbral de vida (0-1) bajo el cual se activa el efecto de respiración agitada.")]
    [SerializeField] private float lowHealthThreshold = 0.4f;
    
    // Constantes de prioridad para evitar números mágicos
    private const int PriorityActive = 50;
    private const int PriorityInactive = 10;

    private List<CinemachineVirtualCamera> _allVirtualCameras = new List<CinemachineVirtualCamera>();
    private CameraState _currentState;
    private CombatManager _combatManager;

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
            if (activeVCam != null)
            {
                // Buscamos el modificador en el objeto de la cámara virtual activa
                var modifier = activeVCam.VirtualCameraGameObject.GetComponent<CinemachineCameraModifier>();
                if (modifier != null)
                {
                    modifier.panicFactor = panic;
                }
            }
        }
    }

    /// <summary>
    /// Cambia el estado de la cámara gestionando las prioridades de Cinemachine.
    /// </summary>
    public void ChangeState(CameraState newState)
    {
        _currentState = newState;
        ResetAllPriorities();

        switch (newState)
        {
            case CameraState.Overview:
                bool isPlayerTurn = _combatManager != null && _combatManager.CurrentFighter != null && 
                                   _combatManager.CurrentFighter.team == Team.PLAYERS;
                SetCameraPriority(isPlayerTurn ? playerOverviewCam : enemyOverviewCam, PriorityActive);
                break;

            case CameraState.Action:
                // Si el sistema de targets está activo, le damos prioridad a su cámara
                if (targetSystem != null && targetSystem.GroupActionCam != null)
                    SetCameraPriority(targetSystem.GroupActionCam, PriorityActive);
                else
                    SetCameraPriority(actionCam, PriorityActive);
                break;

            case CameraState.Ui:
                SetCameraPriority(diegeticUiCam, PriorityActive);
                break;

            case CameraState.HitReaction:
                // Generalmente se mantiene la cámara de acción pero el FXManager dispara el shake
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
        
        // Aquí se podría añadir lógica extra para que la cámara de Overview 
        // se incline levemente hacia el target usando un ThirdPersonFollow u Offset.
    }

    private void HandleTurnStarted(Fighter fighter)
    {
        // Cuando empieza un turno, volvemos a Overview y enfocamos al luchador actual
        if (fighter != null)
        {
            if (fighter.team == Team.PLAYERS && playerOverviewCam != null)
            {
                playerOverviewCam.m_Follow = fighter.CameraPivot != null ? fighter.CameraPivot : fighter.transform;
                playerOverviewCam.m_LookAt = fighter.transform;
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
