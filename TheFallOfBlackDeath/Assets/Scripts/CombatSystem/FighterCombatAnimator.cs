using UnityEngine;

/// <summary>
/// Se encarga exclusivamente de actualizar las posturas de combate (Idles y Stances) en el Animator
/// dependiendo de las partes del cuerpo que le falten al Fighter.
/// </summary>
public class FighterCombatAnimator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Fighter fighter;
    [SerializeField] private Animator animator;

    [Header("Animator Parameters")]
    [SerializeField] private string isMissingLeftArmParam = "IsMissingLeftArm";
    [SerializeField] private string isMissingRightArmParam = "IsMissingRightArm";
    [SerializeField] private string isMissingOneLegParam = "IsMissingOneLeg";
    [SerializeField] private string isMissingBothLegsParam = "IsMissingBothLegs";

    private void Awake()
    {
        if (fighter == null) fighter = GetComponent<Fighter>();
        if (animator == null) animator = GetComponent<Animator>();
        
        if (animator == null && fighter != null)
        {
            animator = fighter.animator;
        }
    }

    private void OnEnable()
    {
        if (fighter != null)
        {
            fighter.OnBodyPartDestroyedEvent += HandleBodyPartDestroyed;
        }
    }

    private void OnDisable()
    {
        if (fighter != null)
        {
            fighter.OnBodyPartDestroyedEvent -= HandleBodyPartDestroyed;
        }
    }

    private void Start()
    {
        // Forzar actualización inicial por si ya está herido al empezar
        UpdateCombatStance();
    }

    /// <summary>
    /// Suscripción al evento de destrucción de partes del cuerpo.
    /// </summary>
    private void HandleBodyPartDestroyed(BodyPart part)
    {
        UpdateCombatStance();
    }

    /// <summary>
    /// Fuerza la actualización de los parámetros del Animator basándose en el estado actual del Fighter.
    /// </summary>
    public void UpdateCombatStance()
    {
        if (fighter == null || animator == null) return;

        // Brazos
        bool missingLeftArm = IsPartDestroyed(BodyPart.LeftArm);
        bool missingRightArm = IsPartDestroyed(BodyPart.RightArm);

        animator.SetBool(isMissingLeftArmParam, missingLeftArm);
        animator.SetBool(isMissingRightArmParam, missingRightArm);

        // Piernas (usando las propiedades existentes del Fighter)
        animator.SetBool(isMissingOneLegParam, fighter.oneLegBroken);
        animator.SetBool(isMissingBothLegsParam, fighter.bothLegsBroken);
    }

    private bool IsPartDestroyed(BodyPart partType)
    {
        var partData = fighter.GetBodyPart(partType);
        return partData != null && partData.IsDestroyed;
    }
}
