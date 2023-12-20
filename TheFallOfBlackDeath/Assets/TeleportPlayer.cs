using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportPlayer : MonoBehaviour
{
    public Transform destino; // El destino al que quieres mover al jugador

    void OnTriggerStay(Collider other)
    {
        // Verificar si el objeto que está en el área del trigger es el jugador
        //if (other.CompareTag("Charecter"))
        //{
            // Verificar si la tecla "E" está siendo presionada
            if (Input.GetKeyDown(KeyCode.E))
            {
                MoverJugadorADestino(other.gameObject);
            }
        //}
    }

    void MoverJugadorADestino(GameObject player)
    {
        // Mover el jugador al destino
        if (destino != null)
        {
            // Obtener el componente de transformación del jugador
            //Transform jugadorTransform = transform;

            // Establecer la posición del jugador en la posición del destino
            player.transform.position = destino.position;

            Debug.Log("Jugador movido a la posición del destino.");
        }
        else
        {
            Debug.LogError("El destino no está asignado. Asigna un objeto de destino en el Inspector.");
        }
    }
}