using UnityEngine;
using UnityEngine.AI;

public class SeguirJugador : MonoBehaviour
{
    public NavMeshAgent navMeshAgent;
    public GameObject player;
    public float distanceToFollowPlayer = 5f; // Distancia a la que empezará a seguir al jugador (dependerá de la escala de vuestro escenario, modificable desde el Editor de Unity)
    public Transform returnDestination; // Destino al que la IA volverá si el jugador se aleja demasiado
    Vector3 currentTarget; // Almacena el objetivo actual al que se dirige (incluyendo al jugador o el destino de retorno)

    void Start()
    {
        currentTarget = returnDestination.position; // Establece el destino de retorno como objetivo inicial
        navMeshAgent.SetDestination(currentTarget); // Asigna el objetivo al que debe ir (destino de retorno)
    }

    void Update()
    {
        if (Vector3.Distance(player.transform.position, transform.position) < distanceToFollowPlayer) // Si el jugador está dentro de la distancia especificada para empezar a seguirlo ...
        {
            currentTarget = player.transform.position; // ... asigna como objetivo actual al jugador
        }
        else // Si el jugador se aleja demasiado ...
        {
            currentTarget = returnDestination.position; // ... asigna como objetivo actual el destino de retorno
        }

        if (navMeshAgent.destination != currentTarget) // Si el objetivo actual es diferente al destino actual del NavMeshAgent ...
        {
            navMeshAgent.SetDestination(currentTarget); // ... asigna el nuevo objetivo como destino del NavMeshAgent
        }
    }
}