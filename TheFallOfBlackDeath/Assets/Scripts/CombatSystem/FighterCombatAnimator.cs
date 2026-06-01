using UnityEngine;

/// <summary>
/// Se encarga exclusivamente de actualizar las posturas de combate (Idles y Stances) en el Animator
/// utilizando un Blend Tree de tipo Direct basado en Floats.
/// </summary>
public class FighterCombatAnimator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Fighter fighter;
    [SerializeField] private Animator animator;

    [Header("Animator Parameters (Must be FLOATS)")]
    [SerializeField] private string baseWeightParam = "BaseWeight";
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
        UpdateCombatStance();
    }

    private void HandleBodyPartDestroyed(BodyPart part)
    {
        UpdateCombatStance();
    }

    /// <summary>
    /// Fuerza la actualización de los parámetros Float del Animator.
    /// </summary>
    public void UpdateCombatStance()
    {
        if (fighter == null || animator == null) return;

        // 1. Empezamos con todos los pesos en 0
        float baseWeight = 0f;
        float leftArmWeight = 0f;
        float rightArmWeight = 0f;
        float oneLegWeight = 0f;
        float bothLegsWeight = 0f;

        // 2. Evaluamos desde la herida más grave a la más leve
        if (fighter.bothLegsBroken)
        {
            bothLegsWeight = 1f; // Pierde las dos piernas (Postura en el suelo)
        }
        else if (fighter.oneLegBroken)
        {
            oneLegWeight = 1f; // Pierde una pierna (Postura cojeando)
        }
        else if (IsPartDestroyed(BodyPart.RightArm))
        {
            rightArmWeight = 1f; // Pierde brazo derecho
        }
        else if (IsPartDestroyed(BodyPart.LeftArm))
        {
            leftArmWeight = 1f; // Pierde brazo izquierdo
        }
        else
        {
            baseWeight = 1f; // Personaje sano (Postura normal)
        }

        // 3. Enviamos los valores al Animator (Solo UNO va a ser 1f, el resto 0f)
        animator.SetFloat(baseWeightParam, baseWeight);
        animator.SetFloat(isMissingLeftArmParam, leftArmWeight);
        animator.SetFloat(isMissingRightArmParam, rightArmWeight);
        animator.SetFloat(isMissingOneLegParam, oneLegWeight);
        animator.SetFloat(isMissingBothLegsParam, bothLegsWeight);
    }
    private bool IsPartDestroyed(BodyPart partType)
    {
        var partData = fighter.GetBodyPart(partType);
        return partData != null && partData.IsDestroyed;
    }
}