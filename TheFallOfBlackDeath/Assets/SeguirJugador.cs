using UnityEngine;
using UnityEngine.AI;

public class SeguirJugador: MonoBehaviour
{
    public NavMeshAgent navMeshAgent;
    public GameObject player;
    public GameObject[] destinations; // Usa un array de destinos para poder asignar tantos destinos como desees (excepto el jugador)
    public float distanceToFollowPlayer = 5f; // Distancia a la que empezar� a seguir al jugador (depender� de la escala de vuestro escenario, modificable desde el Editor de Unity)
    Vector3 currentTarget; // Almacena el objetivo actual al que se dirige (incluir� al jugador)
    int currentDestination = 0; // Controla el destino actual al que se dirige (del array de destinos)

    void Start()
    {
        currentTarget = destinations[currentDestination].transform.position; // Asigna el primer destino para empezar a moverse
    }

    void Update()
    {
        if (Vector3.Distance(destinations[currentDestination].transform.position, transform.position) < 0.1f) // Controla cuando alcanza el destino actual (no es recomendable poner "igual a 0")
        {
            if (currentDestination == destinations.Length - 1) // Si el destino actual es el �ltimo del array ...
            {
                currentDestination = 0; // ... volver� a empezar de nuevo
            }
            else // si no ...
            {
                currentDestination++; // ... continuar� con el siguiente destino
            }
        }

        if (Vector3.Distance(player.transform.position, transform.position) < distanceToFollowPlayer) // Si el jugador est� dentro de la distancia especificada para empezar a seguirlo ...
        {
            currentTarget = player.transform.position; // ... asigna como objetivo actual al jugador
        }
        else // si no ...
        {
            currentTarget = destinations[currentDestination].transform.position; // ... contin�a con el destino que le corresponde (tambi�n controla que el jugador consiga escapar si corre m�s que el enemigo)
        }

        navMeshAgent.destination = currentTarget; // Asigna el objetivo al que debe ir, ya sea destino o jugador
    }
}