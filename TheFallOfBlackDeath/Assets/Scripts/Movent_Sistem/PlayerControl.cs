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
    public Fighter fighter;

    [Header("Estadisticas Normales")]
    public float velocidad;
    public float velCorriendo;
    [SerializeField] private float alturaDeSalto;
    [SerializeField] private float tiempoAlGirar;

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
        anim = GetComponentInChildren<Animator>();
        playerRB = GetComponentInChildren<Rigidbody>();
    }

    /// <summary>
    /// Updates the component each frame while it is active.
    /// </summary>
    private void Update()
    {
        
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        Vector3 direccion = new Vector3(horizontal, 0, vertical).normalized;
        isWalking = direccion.magnitude >= 0.1f;
        
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
            float objetivoAngulo = Mathf.Atan2(direccion.x, direccion.z) * Mathf.Rad2Deg + camara.transform.eulerAngles.y;
            float angulo = Mathf.SmoothDampAngle(transform.eulerAngles.y, objetivoAngulo, ref velocidadGiro, tiempoAlGirar);
            transform.rotation = Quaternion.Euler(0, angulo, 0);

            if (isWalking)
            {
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    if(fighter.legBroken == false)
                    {
                        Vector3 mover = Quaternion.Euler(0, objetivoAngulo, 0) * Vector3.forward;
                        controller.Move(mover.normalized * velCorriendo * Time.deltaTime);
                        anim.SetFloat("Movent", 0.2f);
                        if (GameManager.Instance != null)

                            GameManager.Instance.isWalking = true;
                    }

                    if (fighter.legBroken == true)
                    {
                        Vector3 mover = Quaternion.Euler(0, objetivoAngulo, 0) * Vector3.forward;
                        controller.Move(mover.normalized * velocidad * Time.deltaTime);
                        anim.SetFloat("Movent", 0.1f);
                        if (GameManager.Instance != null)

                            GameManager.Instance.isWalking = true;
                    }
                        
                        
                }
                else
                {
                    Vector3 mover = Quaternion.Euler(0, objetivoAngulo, 0) * Vector3.forward;
                    controller.Move(mover.normalized * velocidad * Time.deltaTime);
                    anim.SetFloat("Movent", 0.1f);
                    if (GameManager.Instance != null)

                        GameManager.Instance.isWalking = true;
                }
            }
            else
            {
                
                anim.SetFloat("Movent", 0f);
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
        anim.SetFloat("Movent", 0f);
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

}
