using UnityEngine;

/// <summary>
/// Script diseñado para mostrar una imagen de tutorial cuando se ejecuta la habilidad LethalQTESkill.
/// </summary>
public class LethalQTETutorialTrigger : MonoBehaviour
{
    [Header("Configuración de UI")]
    [Tooltip("El objeto que contiene la imagen o el panel del tutorial.")]
    [SerializeField] private GameObject tutorialContainer;

    private void OnEnable()
    {
        // Nos suscribimos al evento estático de la habilidad
        LethalQTESkill.OnLethalSkillExecuted += HandleLethalSkillExecuted;
        LethalQTESkill.OnLethalSkillFinished += HandleLethalSkillFinished;
    }

    private void OnDisable()
    {
        // Siempre desuscribirse para evitar fugas de memoria o errores
        LethalQTESkill.OnLethalSkillExecuted -= HandleLethalSkillExecuted;
        LethalQTESkill.OnLethalSkillFinished -= HandleLethalSkillFinished;
    }

    private void HandleLethalSkillExecuted()
    {
        // Activamos el contenedor del tutorial
        if (tutorialContainer != null)
        {
            tutorialContainer.SetActive(true);
            Debug.Log("[LethalQTETutorialTrigger] Tutorial activado.");
        }
        else
        {
            Debug.LogWarning("[LethalQTETutorialTrigger] No se ha asignado el tutorialContainer en el inspector.");
        }
    }

    private void HandleLethalSkillFinished()
    {
        // Ocultamos el contenedor del tutorial al finalizar la habilidad (sea por parry o por fin de duración)
        if (tutorialContainer != null)
        {
            tutorialContainer.SetActive(false);
        }
    }
}
