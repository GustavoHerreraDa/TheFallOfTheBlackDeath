using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem.XR;

/// <summary>
/// Supports exploration and world-state flow by handling player control.
/// </summary>
public class PlayerControl : MonoBehaviour
{
    private CharacterController controller;
    private GameObject camara;
    private Camera_Main cameraMain;
    public Fighter fighter;

    [Header("Estadisticas Normales")]
    public float velocidad;
    public float velCorriendo;
    [SerializeField] private float alturaDeSalto;
    [SerializeField] private float tiempoAlGirar;

    [Header("Animacion por Piernas")]
    [SerializeField] private string movementParameter = "Movent";
    [SerializeField] private string brokenLegsParameter = "BrokenLegs";
    [SerializeField] private string oneLegBrokenParameter = "OneLegBroken";
    [SerializeField] private string bothLegsBrokenParameter = "BothLegsBroken";
    
    // --- NUEVAS VARIABLES PARA IDLE ---
    [SerializeField] private float idleAnimValue = 0f;
    [SerializeField] private float oneLegBrokenIdleAnimValue = 0.5f; // Ajusta este valor en el Inspector
    [SerializeField] private float bothLegsBrokenIdleAnimValue = 0.6f; // Ajusta este valor en el Inspector
    // ----------------------------------
    
    [SerializeField] private float walkAnimValue = 0.1f;
    [SerializeField] private float runAnimValue = 0.2f;
    [SerializeField] private float oneLegBrokenMoveAnimValue = 0.3f;
    [SerializeField] private float bothLegsBrokenMoveAnimValue = 0.4f;
    [SerializeField] private float oneLegBrokenSpeedMultiplier = 0.65f;
    [SerializeField] private float bothLegsBrokenSpeedMultiplier = 0.35f;

    [Header("Datos sobre el piso")]
    [SerializeField] private Transform detectaPiso;
    [SerializeField] private float distanciaPiso;
    [SerializeField] private LayerMask mascaraPiso;
    public bool stop;
    private bool isWalking = false;

    float velocidadGiro;
    public float gravedad = -9.81f;
    Vector3 velocity;
    bool tocaPiso;

    public Animator anim;
    Rigidbody playerRB;
    
    /// <summary>
    /// Initializes the component once the scene dependencies are ready.
    /// </summary>
    private void Start()
    {
        controller = GetComponent<CharacterController>();
        camara = GameObject.FindGameObjectWithTag("MainCamera");
        if (camara != null)
            cameraMain = camara.GetComponent<Camera_Main>();
        anim = GetComponentInChildren<Animator>();
        playerRB = GetComponentInChildren<Rigidbody>();
    }

    /// <summary>
    /// Updates the component each frame while it is active.
    /// </summary>
    private void Update()
    {
        bool isInspectingCharacter = cameraMain != null && cameraMain.IsInspectingCharacter;
        
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        Vector3 direccion = new Vector3(horizontal, 0, vertical).normalized;
        isWalking = !isInspectingCharacter && direccion.magnitude >= 0.1f;
        
        if (GameManager.Instance != null)
            GameManager.Instance.canGetEncounter = isWalking;

        tocaPiso = Physics.CheckSphere(detectaPiso.position, distanciaPiso, mascaraPiso);

        if (tocaPiso && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        if (Input.GetButtonDown("Jump") && tocaPiso)
        {
            velocity.y = Mathf.Sqrt(alturaDeSalto * -2 * gravedad);
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            stop = !stop;
        }

        velocity.y += gravedad * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        if (!stop)
        {
            int brokenLegs = GetBrokenLegCount();
            UpdateLegInjuryAnimatorParameters(brokenLegs);

            if (isInspectingCharacter)
            {
                // Ahora evalúa el Idle correcto basado en las piernas rotas
                SetMovementAnimation(GetIdleAnimValueForLegState(brokenLegs));
                if (GameManager.Instance != null)
                    GameManager.Instance.isWalking = false;

                return;
            }

            float objetivoAngulo = Mathf.Atan2(direccion.x, direccion.z) * Mathf.Rad2Deg + camara.transform.eulerAngles.y;
            float angulo = Mathf.SmoothDampAngle(transform.eulerAngles.y, objetivoAngulo, ref velocidadGiro, tiempoAlGirar);
            transform.rotation = Quaternion.Euler(0, angulo, 0);

            if (isWalking)
            {
                bool canRun = brokenLegs == 0;
                bool wantsRun = Input.GetKey(KeyCode.LeftShift) && canRun;
                float currentSpeed = GetMovementSpeedForLegState(brokenLegs, wantsRun);
                Vector3 mover = Quaternion.Euler(0, objetivoAngulo, 0) * Vector3.forward;

                controller.Move(mover.normalized * currentSpeed * Time.deltaTime);
                SetMovementAnimation(GetMovementAnimValueForLegState(brokenLegs, wantsRun));

                if (GameManager.Instance != null)
                    GameManager.Instance.isWalking = true;
            }
            else
            {
                // Ahora evalúa el Idle correcto basado en las piernas rotas
                SetMovementAnimation(GetIdleAnimValueForLegState(brokenLegs));
                if (GameManager.Instance != null)
                    GameManager.Instance.isWalking = false;
            }
        }
    }

    /// <summary>
    /// Executes the continue player workflow.
    /// </summary>
    public void ContinuePlayer()
    {
        stop = false;
        playerRB.isKinematic = false;
        if (GameManager.Instance != null)
            GameManager.Instance.isWalking = isWalking;
    }

    /// <summary>
    /// Executes the stop player workflow.
    /// </summary>
    /// <param name="seconds">The seconds.</param>
    public void StopPlayer(float seconds)
    {
        // También actualizamos el Idle aquí por si el jugador se detiene
        int brokenLegs = GetBrokenLegCount();
        SetMovementAnimation(GetIdleAnimValueForLegState(brokenLegs));
        
        stop = true;
        playerRB.isKinematic = true;
        StartCoroutine(WaitSeconds(seconds));
    }

    /// <summary>
    /// Applies late-frame adjustments after the main update loop has completed.
    /// </summary>
    private void LateUpdate()
    {
        if (!stop)
            playerRB.isKinematic = false;
    }

    /// <summary>
    /// Executes the wait seconds workflow.
    /// </summary>
    /// <param name="seconds">The seconds.</param>
    /// <returns>An enumerator that drives the coroutine sequence.</returns>
    IEnumerator WaitSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        Debug.Log("Han pasado 3 segundos");
        this.ContinuePlayer();
    }

    /// <summary>
    /// Responds to the corresponding Unity trigger callback for this component.
    /// </summary>
    /// <param name="other">The other.</param>
    private void OnTriggerStay(Collider other)
    {
        if (other.tag == "region1" && isWalking)
        {
            if (GameManager.Instance != null)
                GameManager.Instance.canGetEncounter = true;
            Debug.Log("Se produjo un encuentro");
        }
    }
    
    /// <summary>
    /// Responds to the corresponding Unity trigger callback for this component.
    /// </summary>
    /// <param name="other">The other.</param>
    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "region2")
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.cuRegions = 1;
                GameManager.Instance.canGetEncounter = false;
            }
        }

        if (other.tag == "region1")
        {
            if (GameManager.Instance != null)
                GameManager.Instance.cuRegions = 0;
        }
    }

    /// <summary>
    /// Executes the teleport player workflow.
    /// </summary>
    /// <param name="destinoPosition">The destino position.</param>
    public void TeleportPlayer(Vector3 destinoPosition)
    {
        controller.enabled = false;
        transform.position = destinoPosition;
        controller.enabled = true;
    }

    private int GetBrokenLegCount()
    {
        return fighter != null ? fighter.brokenLegCount : 0;
    }

    // --- NUEVO MÉTODO PARA EVALUAR EL IDLE ---
    private float GetIdleAnimValueForLegState(int brokenLegs)
    {
        if (brokenLegs >= 2)
            return bothLegsBrokenIdleAnimValue;

        if (brokenLegs == 1)
            return oneLegBrokenIdleAnimValue;

        return idleAnimValue; // Idle normal (0 piernas rotas)
    }
    // -----------------------------------------

    private float GetMovementSpeedForLegState(int brokenLegs, bool wantsRun)
    {
        if (brokenLegs >= 2)
            return velocidad * bothLegsBrokenSpeedMultiplier;

        if (brokenLegs == 1)
            return velocidad * oneLegBrokenSpeedMultiplier;

        return wantsRun ? velCorriendo : velocidad;
    }

    private float GetMovementAnimValueForLegState(int brokenLegs, bool wantsRun)
    {
        if (brokenLegs >= 2)
            return bothLegsBrokenMoveAnimValue;

        if (brokenLegs == 1)
            return oneLegBrokenMoveAnimValue;

        return wantsRun ? runAnimValue : walkAnimValue;
    }

    private void SetMovementAnimation(float value)
    {
        if (anim != null && HasAnimatorParameter(movementParameter, AnimatorControllerParameterType.Float))
            anim.SetFloat(movementParameter, value);
    }

    private void UpdateLegInjuryAnimatorParameters(int brokenLegs)
    {
        if (anim == null) return;

        if (HasAnimatorParameter(brokenLegsParameter, AnimatorControllerParameterType.Int))
            anim.SetInteger(brokenLegsParameter, brokenLegs);

        if (HasAnimatorParameter(oneLegBrokenParameter, AnimatorControllerParameterType.Bool))
            anim.SetBool(oneLegBrokenParameter, brokenLegs == 1);

        if (HasAnimatorParameter(bothLegsBrokenParameter, AnimatorControllerParameterType.Bool))
            anim.SetBool(bothLegsBrokenParameter, brokenLegs >= 2);
    }

    private bool HasAnimatorParameter(string parameterName, AnimatorControllerParameterType type)
    {
        if (anim == null || string.IsNullOrEmpty(parameterName))
            return false;

        foreach (var parameter in anim.parameters)
        {
            if (parameter.name == parameterName && parameter.type == type)
                return true;
        }

        return false;
    }
}