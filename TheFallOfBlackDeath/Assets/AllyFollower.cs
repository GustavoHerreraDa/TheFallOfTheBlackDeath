using UnityEngine;
using UnityEngine.AI;

public class AllyFollower : MonoBehaviour
{
    [Header("Componentes")]
    public NavMeshAgent agent;
    public Animator anim;
    public Transform target;
    private PlayerControl playerControl;

    [Header("Configuración de Seguimiento")]
    public float stoppingDistance = 2f;
    public float sprintDistance = 6f; // Distancia a la que empieza a correr para alcanzar al player

    void Start()
    {
        if (!agent) agent = GetComponent<NavMeshAgent>();
        if (!anim) anim = GetComponent<Animator>();
        
        if (agent != null)
        {
            agent.stoppingDistance = stoppingDistance;
        }

        // Intentar encontrar el PlayerControl si no tenemos target o para sincronizar velocidad
        if (target != null)
        {
            playerControl = target.GetComponent<PlayerControl>();
        }
        
        if (playerControl == null)
        {
            playerControl = FindObjectOfType<PlayerControl>();
            if (playerControl != null && target == null)
            {
                target = playerControl.transform;
            }
        }
    }

    void Update()
    {
        if (target == null || agent == null || !agent.isActiveAndEnabled || !agent.isOnNavMesh) 
        {
            UpdateAnimations();
            return;
        }

        float distance = Vector3.Distance(transform.position, target.position);

        if (distance > stoppingDistance)
        {
            agent.isStopped = false;
            agent.SetDestination(target.position);
            AdaptSpeed(distance);
        }
        else
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
        }

        UpdateAnimations();
    }

    void AdaptSpeed(float distance)
    {
        if (playerControl == null || agent == null || !agent.isActiveAndEnabled) return;

        // Si el jugador está corriendo o el aliado está muy lejos, corre también
        bool shouldChase = Input.GetKey(KeyCode.LeftShift) || distance > sprintDistance;
        
        // Obtenemos las velocidades del player script para adaptarnos
        // Usamos una pequeña compensación (1.1f) para que el aliado pueda alcanzar al player si este se mueve
        float targetSpeed = shouldChase ? playerControl.velCorriendo : playerControl.velocidad;
        agent.speed = targetSpeed * 1.1f;
    }

    void UpdateAnimations()
    {
        if (anim == null) return;

        bool isMoving = false;
        bool isChasing = false;

        // Solo consultamos el agente si está activo y en el NavMesh
        if (agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh)
        {
            float speed = agent.velocity.magnitude;
            isMoving = !agent.isStopped && speed > 0.1f;
            
            if (isMoving && playerControl != null)
            {
                // Si la velocidad es mayor que la de caminar, consideramos que está en "Chase" (corriendo)
                isChasing = speed > (playerControl.velocidad + 0.5f);
            }
        }
        else if (agent != null)
        {
            // Fallback si no está en NavMesh: usar la velocidad del Rigidbody o transform si existiera, 
            // pero para un NavMeshAgent desconectado, lo más seguro es guiarle a Idle o usar su velocity actual si no es cero.
            float speed = agent.velocity.magnitude;
            isMoving = speed > 0.1f;
        }

        anim.SetBool("Idle", !isMoving);
        anim.SetBool("Walk", isMoving && !isChasing);
        anim.SetBool("Chase", isMoving && isChasing);
    }
}
