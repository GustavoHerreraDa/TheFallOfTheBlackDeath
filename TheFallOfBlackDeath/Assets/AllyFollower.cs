using UnityEngine;
using UnityEngine.AI;

public class AllyFollower : MonoBehaviour
{
    [Header("Componentes")]
    public NavMeshAgent agent;
    public Animator anim;
    public Transform target;

    [Header("Configuración de Seguimiento")]
    public float stoppingDistance = 2f;
    public float movementSpeed = 3.5f;

    void Start()
    {
        if (!agent) agent = GetComponent<NavMeshAgent>();
        if (!anim) anim = GetComponent<Animator>();
        
        if (agent != null)
        {
            agent.stoppingDistance = stoppingDistance;
            agent.speed = movementSpeed;
        }
    }

    void Update()
    {
        if (target == null) return;

        float distance = Vector3.Distance(transform.position, target.position);

        if (distance > stoppingDistance)
        {
            agent.isStopped = false;
            agent.SetDestination(target.position);
        }
        else
        {
            agent.isStopped = true;
        }

        UpdateAnimations();
    }

    void UpdateAnimations()
    {
        if (anim == null) return;

        // Suponiendo que el animador tiene un parámetro "Speed" o "IsWalking"
        // Si usa los mismos que FollowPlayer (Idle, Patrol, Chase, Death), los adaptamos.
        // Dado que es un aliado, simplificamos a Caminar/Idle.
        
        bool isMoving = !agent.isStopped && agent.velocity.magnitude > 0.1f;
        
        // Intentamos usar parámetros comunes, o los de FollowPlayer si el animator es compartido
        anim.SetBool("Idle", !isMoving);
        anim.SetBool("Chase", isMoving); // Usamos "Chase" como "Moving" para ser compatible con animators existentes
    }
}
