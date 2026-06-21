using UnityEngine;

/// <summary>
/// Define los estados posibles para el sistema de cámaras.
/// </summary>
public enum CameraState
{
    Overview,     // Vista general del campo de batalla
    Action,       // Vista de acción durante ataques
    Ui,           // Vista enfocada cuando se abren menús de habilidades
    SkillPanel,   // Vista diegética para el panel de habilidades del jugador activo
    HitReaction,  // Vista de impacto/reacción al recibir daño
    Cinematic     // Vista para momentos guionizados o ejecuciones
}
