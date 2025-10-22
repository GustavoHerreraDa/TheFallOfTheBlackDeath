using UnityEngine;
using UnityEngine.AI;

public class SeguirJugador : MonoBehaviour
{
    [Header("Componentes")]
    public NavMeshAgent navMeshAgent;
    public GameObject player;
    public AudioSource audioSource; 

    [Header("Configuración de persecución")]
    public float distanceToFollowPlayer = 5f;
    public float velocidadNormal = 3.5f;
    public float velocidadPersecucion = 6f; 

    [Header("Patrulla")]
    public Transform puntoA;
    public Transform puntoB;
    private Transform destinoActual;

    private bool siguiendoJugador = false;
    private bool sonidoReproducido = false;

    void Start()
    {
        destinoActual = puntoA;
        navMeshAgent.speed = velocidadNormal;
        navMeshAgent.SetDestination(destinoActual.position);
    }

    void Update()
    {
        float distanciaJugador = Vector3.Distance(player.transform.position, transform.position);

        if (distanciaJugador < distanceToFollowPlayer)
        {
        
            if (!siguiendoJugador)
            {
                siguiendoJugador = true;
                navMeshAgent.speed = velocidadPersecucion;

                if (audioSource != null && !sonidoReproducido)
                {
                    audioSource.Play();
                    sonidoReproducido = true;
                }
            }

            navMeshAgent.SetDestination(player.transform.position);
        }
        else
        {
            if (siguiendoJugador)
            {
                siguiendoJugador = false;
                sonidoReproducido = false;
                navMeshAgent.speed = velocidadNormal;
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
