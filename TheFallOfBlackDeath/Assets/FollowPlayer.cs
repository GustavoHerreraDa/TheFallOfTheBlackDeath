using UnityEngine;
using UnityEngine.AI;

public class FollowPlayer : MonoBehaviour
{
    public enum EnemyState { Idle, Patrol, Chase, Death }

    [Header("Estado actual del enemigo")]
    public EnemyState currentState = EnemyState.Idle;

    [Header("Componentes")]
    public NavMeshAgent agent;
    public Animator anim;
    public AudioSource audioSource;
    public Transform player;

    [Header("Ajustes")]
    public float chaseEnterDistance = 5f;
    public float chaseExitDistance = 8f;
    public float normalSpeed = 3.5f;
    public float chaseSpeed = 6f;

    [Header("Patrulla")]
    public Transform puntoA;
    public Transform puntoB;
    private Transform destinoActual;

    private bool sonidoReproducido = false;

    void Start()
    {
        // Validaciones
        if (!agent) agent = GetComponent<NavMeshAgent>();
        if (!anim) anim = GetComponent<Animator>();
        if (!player)
        {
            Debug.LogError("FollowPlayer: No se asignó el jugador! (player)", this);
            enabled = false;
            return;
        }

        if (currentState == EnemyState.Patrol && puntoA != null)
        {
            destinoActual = puntoA;
            agent.SetDestination(destinoActual.position);
        }

        SetAnimationBooleans();
    }

    void Update()
    {
        CheckTransitions();

        switch (currentState)
        {
            case EnemyState.Idle: IdleTick(); break;
            case EnemyState.Patrol: PatrolTick(); break;
            case EnemyState.Chase: ChaseTick(); break;
            case EnemyState.Death: agent.isStopped = true; break;
        }
    }

    // ===================== ESTADOS =======================

    void IdleTick()
    {
        agent.isStopped = true;
    }

    void PatrolTick()
    {
        agent.isStopped = false;

        if (agent.remainingDistance < 0.3f)
        {
            destinoActual = (destinoActual == puntoA) ? puntoB : puntoA;
            agent.SetDestination(destinoActual.position);
        }
    }

    void ChaseTick()
    {
        agent.isStopped = false;

        // cache de destino y playerPos para evitar llamar .position mil veces
        Vector3 playerPos = player.position;

        // solo actualizar si cambió lo suficiente
        if ((playerPos - agent.destination).sqrMagnitude > 0.2f)
            agent.SetDestination(playerPos);

        if (!sonidoReproducido)
        {
            audioSource?.Play();
            sonidoReproducido = true;
        }
    }

    // ===================== TRANSICIONES =======================

    void CheckTransitions()
    {
        // Death siempre cancela las transiciones
        if (currentState == EnemyState.Death)
            return;

        float sqrDist = (transform.position - player.position).sqrMagnitude;
        float sqrEnter = chaseEnterDistance * chaseEnterDistance;
        float sqrExit = chaseExitDistance * chaseExitDistance;

        // Enter chase
        if (sqrDist < sqrEnter)
        {
            if (currentState != EnemyState.Chase)
                ChangeState(EnemyState.Chase);
            return;
        }

        // Exit chase
        if (currentState == EnemyState.Chase && sqrDist > sqrExit)
        {
            ChangeState(EnemyState.Patrol);
            return;
        }
    }

    // ===================== CAMBIAR ESTADO =======================

    void ChangeState(EnemyState newState)
    {
        if (currentState == newState) return;

        Debug.Log("Cambio de estado: " + currentState + " → " + newState);

        currentState = newState;
        sonidoReproducido = false;
        agent.isStopped = false;

        switch (newState)
        {
            case EnemyState.Idle:
                agent.speed = normalSpeed;
                break;

            case EnemyState.Patrol:
                agent.speed = normalSpeed;

                // Elegir el punto más cercano al volver desde Chase
                if (puntoA && puntoB)
                {
                    destinoActual =
                        Vector3.Distance(transform.position, puntoA.position) <
                        Vector3.Distance(transform.position, puntoB.position)
                        ? puntoA : puntoB;

                    agent.SetDestination(destinoActual.position);
                }
                break;

            case EnemyState.Chase:
                agent.speed = chaseSpeed;
                break;

            case EnemyState.Death:
                agent.isStopped = true;
                break;
        }

        SetAnimationBooleans();
    }

    void SetAnimationBooleans()
    {
        anim.SetBool("Idle", currentState == EnemyState.Idle);
        anim.SetBool("Patrol", currentState == EnemyState.Patrol);
        anim.SetBool("Chase", currentState == EnemyState.Chase);
        anim.SetBool("Death", currentState == EnemyState.Death);
    }
}
