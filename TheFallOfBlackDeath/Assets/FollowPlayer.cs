using UnityEngine;
using UnityEngine.AI;
using System.Collections;

/// <summary>
/// Handles follow player for the current project workflow.
/// </summary>
public class FollowPlayer : MonoBehaviour
{
    /// <summary>
    /// Defines the named values used by enemy state.
    /// </summary>
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

    [Header("Vision")]
    [Range(1f, 180f)]
    public float fieldOfView = 100f;
    public float eyeHeight = 1.6f;
    public LayerMask visionMask = Physics.DefaultRaycastLayers;
    public float lostSightGraceTime = 1f;

    [Header("Patrulla")]
    public Transform puntoA;
    public Transform puntoB;
    private Transform destinoActual;

    private bool sonidoReproducido = false;
    private float lostSightTimer = 0f;
    private bool isStunned = false;
    private Coroutine stunRoutine;

    /// <summary>
    /// Initializes the component once the scene dependencies are ready.
    /// </summary>
    void Start()
    {
        if (!agent) agent = GetComponent<NavMeshAgent>();
        if (!anim) anim = GetComponent<Animator>();

        if (!player)
        {
            var playerfigther = FindObjectOfType<PlayerControl>();
            if (playerfigther != null)
            {
                player = playerfigther.transform;
            }
            else
            {
                Debug.LogError("FollowPlayer: No se asigno el jugador! (player)", this);
            }
        }

        if (currentState == EnemyState.Patrol && puntoA != null)
        {
            destinoActual = puntoA;
            agent.SetDestination(destinoActual.position);
        }

        SetAnimationBooleans();
    }

    /// <summary>
    /// Updates the component each frame while it is active.
    /// </summary>
    void Update()
    {
        if (isStunned)
        {
            if (agent != null)
            {
                agent.isStopped = true;
                agent.velocity = Vector3.zero;
            }

            return;
        }

        CheckTransitions();

        switch (currentState)
        {
            case EnemyState.Idle: IdleTick(); break;
            case EnemyState.Patrol: PatrolTick(); break;
            case EnemyState.Chase: ChaseTick(); break;
            case EnemyState.Death: agent.isStopped = true; break;
        }
    }

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

        Vector3 playerPos = player.position;
        if ((playerPos - agent.destination).sqrMagnitude > 0.2f)
            agent.SetDestination(playerPos);

        if (!sonidoReproducido)
        {
            audioSource?.Play();
            sonidoReproducido = true;
        }
    }

    void CheckTransitions()
    {
        if (isStunned || currentState == EnemyState.Death || player == null)
            return;

        float sqrDist = (transform.position - player.position).sqrMagnitude;
        float sqrEnter = chaseEnterDistance * chaseEnterDistance;
        float sqrExit = chaseExitDistance * chaseExitDistance;
        bool hasLineOfSight = CanSeePlayer();

        if (sqrDist < sqrEnter && hasLineOfSight)
        {
            if (currentState != EnemyState.Chase)
                ChangeState(EnemyState.Chase);

            lostSightTimer = 0f;
            return;
        }

        if (currentState == EnemyState.Chase)
        {
            if (hasLineOfSight && sqrDist <= sqrExit)
            {
                lostSightTimer = 0f;
                return;
            }

            lostSightTimer += Time.deltaTime;

            if (sqrDist > sqrExit || lostSightTimer >= lostSightGraceTime)
            {
                ChangeState(EnemyState.Patrol);
                return;
            }
        }
    }

    bool CanSeePlayer()
    {
        if (isStunned || player == null)
            return false;

        Vector3 enemyEyePos = transform.position + Vector3.up * eyeHeight;
        Vector3 playerAimPos = player.position + Vector3.up * eyeHeight;
        Vector3 toPlayer = playerAimPos - enemyEyePos;

        float sqrDistance = toPlayer.sqrMagnitude;
        if (sqrDistance > chaseEnterDistance * chaseEnterDistance)
            return false;

        Vector3 directionToPlayer = toPlayer.normalized;
        float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);
        if (angleToPlayer > fieldOfView * 0.5f)
            return false;

        if (Physics.Raycast(enemyEyePos, directionToPlayer, out RaycastHit hit, Mathf.Sqrt(sqrDistance), visionMask, QueryTriggerInteraction.Ignore))
        {
            return hit.transform == player || hit.transform.IsChildOf(player);
        }

        return false;
    }

    void ChangeState(EnemyState newState)
    {
        if (currentState == newState) return;

        Debug.Log("Cambio de estado: " + currentState + " -> " + newState);

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

    public void StopEnemyForTransition()
    {
        if (agent != null)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
        }

        this.enabled = false;
    }

    public void StunForSeconds(float duration)
    {
        if (duration <= 0f)
            return;

        if (stunRoutine != null)
            StopCoroutine(stunRoutine);

        stunRoutine = StartCoroutine(StunRoutine(duration));
    }

    private IEnumerator StunRoutine(float duration)
    {
        isStunned = true;
        sonidoReproducido = false;
        lostSightTimer = 0f;

        if (agent != null)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
        }

        currentState = EnemyState.Idle;
        SetAnimationBooleans();

        yield return new WaitForSeconds(duration);

        isStunned = false;

        EnemyState nextState = (puntoA != null && puntoB != null) ? EnemyState.Patrol : EnemyState.Idle;
        currentState = nextState == EnemyState.Patrol ? EnemyState.Idle : EnemyState.Patrol;
        ChangeState(nextState);
        stunRoutine = null;
    }
}
