using UnityEngine;
using UnityEngine.AI;

public class SeguirJugador : MonoBehaviour
{
    public NavMeshAgent navMeshAgent;
    public GameObject player;
    public float distanceToFollowPlayer = 5f; 

    [Header("Patrulla")]
    public Transform puntoA;
    public Transform puntoB;
    private Transform destinoActual;

    private bool siguiendoJugador = false;

    void Start()
    {
        destinoActual = puntoA; 
        navMeshAgent.SetDestination(destinoActual.position);
    }

    void Update()
    {
        float distanciaJugador = Vector3.Distance(player.transform.position, transform.position);

        if (distanciaJugador < distanceToFollowPlayer) 
        {
            siguiendoJugador = true;
            navMeshAgent.SetDestination(player.transform.position);
        }
        else
        {
            if (siguiendoJugador) 
            {
                siguiendoJugador = false;
                navMeshAgent.SetDestination(destinoActual.position);
            }

            Patrullar();
        }
    }

    void Patrullar()
    {
        if (siguiendoJugador) return;

        
        if (!navMeshAgent.pathPending && navMeshAgent.remainingDistance < 0.5f)
        {
            destinoActual = destinoActual == puntoA ? puntoB : puntoA;
            navMeshAgent.SetDestination(destinoActual.position);
        }
    }
}
