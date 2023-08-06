using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class PlayerControl : MonoBehaviour
{
    private CharacterController controller;
    private GameObject camara;

    [Header("Estadisticas Normales")]
    [SerializeField] private float velocidad;
    [SerializeField] private float velCorriendo;
    [SerializeField] private float alturaDeSalto;
    [SerializeField] private float tiempoAlGirar;

    [Header("Datos sobre el piso")]
    [SerializeField] private Transform detectaPiso;
    [SerializeField] private float distanciaPiso;
    [SerializeField] private LayerMask mascaraPiso;
    public bool stop;
    private bool isWalking = false; // Agregamos una variable para rastrear si el jugador está caminando

    float velocidadGiro;
    public float gravedad = -9.81f;
    Vector3 velocity;
    bool tocaPiso;

    Animator anim;
    Rigidbody playerRB;
    private void Start()
    {
        controller = GetComponent<CharacterController>();
        camara = GameObject.FindGameObjectWithTag("MainCamera");
        anim = GetComponentInChildren<Animator>();
        playerRB = GetComponentInChildren<Rigidbody>();
    }

    private void Update()
    {
        // Detectar si el jugador está caminando (puedes ajustar las condiciones según tu juego)
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        Vector3 direccion = new Vector3(horizontal, 0, vertical).normalized;
        isWalking = direccion.magnitude >= 0.1f;
        // Actualizar canGetEncounter basado en si el jugador está caminando o no
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
                    Vector3 mover = Quaternion.Euler(0, objetivoAngulo, 0) * Vector3.forward;
                    controller.Move(mover.normalized * velCorriendo * Time.deltaTime);
                    anim.SetFloat("Movent", 0.2f);
                    GameManager.Instance.isWalking = true;
                }
                else
                {
                    Vector3 mover = Quaternion.Euler(0, objetivoAngulo, 0) * Vector3.forward;
                    controller.Move(mover.normalized * velocidad * Time.deltaTime);
                    anim.SetFloat("Movent", 0.1f);
                    GameManager.Instance.isWalking = true;
                }
            }
            else
            {
                anim.SetFloat("Movent", 0f);
                GameManager.Instance.isWalking = false;
            }
        }
    }

    public void ContinuePlayer()
    {
        stop = false;
        playerRB.isKinematic = false;
        GameManager.Instance.isWalking = isWalking; // Aseguramos que GameManager.isWalking esté actualizado
    }

    public void StopPlayer(float seconds)
    {
        anim.SetFloat("Movent", 0f);
        stop = true;
        playerRB.isKinematic = true;
        StartCoroutine(WaitSeconds(seconds));
    }

    private void LateUpdate()
    {
        if (!stop)
            playerRB.isKinematic = false;
    }

    IEnumerator WaitSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        Debug.Log("Han pasado 3 segundos");
        this.ContinuePlayer();
    }

    private void OnTriggerStay(Collider other)
    {
        // Verificar si el jugador está caminando antes de permitir encuentros
        if (other.tag == "region1" && isWalking)
        {
            GameManager.Instance.canGetEncounter = true;
            Debug.Log("Se produjo un encuentro");
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "region2")
        {
            GameManager.Instance.canGetEncounter = false;
        }

        if (other.tag == "region1")
        {
            GameManager.Instance.cuRegions = 0;
        }
    }

}