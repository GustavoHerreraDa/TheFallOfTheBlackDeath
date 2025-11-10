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

    [Header("Modo de comportamiento")]
    public bool patrullar = true; // ✅ Si está en false, se quedará quieto (modo centinela)

    [Header("Puntos de patrulla")]
    public Transform puntoA;
    public Transform puntoB;

    private Transform destinoActual;
    private Vector3 posicionInicial;
    private bool siguiendoJugador = false;
    private bool sonidoReproducido = false;

    void Start()
    {
        posicionInicial = transform.position;
        navMeshAgent.speed = velocidadNormal;

        if (patrullar && puntoA != null)
        {
            destinoActual = puntoA;
            navMeshAgent.SetDestination(destinoActual.position);
        }
        else
        {
            navMeshAgent.SetDestination(posicionInicial);
        }
    }

    void Update()
    {
        float distanciaJugador = Vector3.Distance(player.transform.position, transform.position);

        // --- PERSEGUIR JUGADOR ---
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
            // --- DEJAR DE SEGUIR ---
            if (siguiendoJugador)
            {
                siguiendoJugador = false;
                sonidoReproducido = false;
                navMeshAgent.speed = velocidadNormal;

                // Si patrulla, vuelve a patrullar; si no, regresa a su posición inicial
                if (patrullar)
                {
                    if (destinoActual == null && puntoA != null)
                        destinoActual = puntoA;

                    navMeshAgent.SetDestination(destinoActual.position);
                }
                else
                {
                    navMeshAgent.SetDestination(posicionInicial);
                }
            }

            if (patrullar)
                Patrullar();
        }
    }

    void Patrullar()
    {
        if (siguiendoJugador || puntoA == null || puntoB == null) return;

        if (!navMeshAgent.pathPending && navMeshAgent.remainingDistance < 0.5f)
        {
            destinoActual = destinoActual == puntoA ? puntoB : puntoA;
            navMeshAgent.SetDestination(destinoActual.position);
        }
    }
}
