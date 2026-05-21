using UnityEngine;
using Cinemachine;

/// <summary>
/// Gestiona el encuadre dinámico de múltiples objetivos (atacante y defensor)
/// utilizando un CinemachineTargetGroup para mantener a ambos en pantalla con pesos dramáticos.
/// </summary>
public class CameraTargetSystem : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private CinemachineTargetGroup targetGroup;
    [SerializeField] private CinemachineVirtualCamera groupActionCam;

    [Header("Dynamic Framing Settings")]
    [Tooltip("Peso del atacante en el encuadre.")]
    [SerializeField] private float attackerWeight = 1f;
    [Tooltip("Peso del defensor en el encuadre (mayor peso = más enfoque en su reacción).")]
    [SerializeField] private float defenderWeight = 1.5f;
    [Tooltip("Radio de influencia de los objetivos en el encuadre.")]
    [SerializeField] private float targetRadius = 2f;

    public CinemachineVirtualCamera GroupActionCam => groupActionCam;

    /// <summary>
    /// Configura el Target Group para enfocar al atacante y al defensor.
    /// Optimizado para evitar asignaciones de memoria innecesarias.
    /// </summary>
    public void SetupDynamicFraming(Transform attacker, Transform defender)
    {
        if (targetGroup == null)
        {
            Debug.LogError("[CameraTargetSystem] CinemachineTargetGroup no asignado.");
            return;
        }

        // Reutilizamos el array si ya existe para minimizar el Garbage Collection
        if (targetGroup.m_Targets == null || targetGroup.m_Targets.Length != 2)
        {
            targetGroup.m_Targets = new CinemachineTargetGroup.Target[2];
        }

        // Configuración del atacante
        targetGroup.m_Targets[0].target = attacker;
        targetGroup.m_Targets[0].weight = attackerWeight;
        targetGroup.m_Targets[0].radius = targetRadius;

        // Configuración del defensor
        targetGroup.m_Targets[1].target = defender;
        targetGroup.m_Targets[1].weight = defenderWeight;
        targetGroup.m_Targets[1].radius = targetRadius;
    }

    /// <summary>
    /// Limpia los objetivos del encuadre.
    /// </summary>
    public void ClearTargets()
    {
        if (targetGroup != null)
        {
            targetGroup.m_Targets = new CinemachineTargetGroup.Target[0];
        }
    }
}
