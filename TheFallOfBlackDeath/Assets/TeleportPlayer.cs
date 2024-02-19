
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportPlayer : Interactable
{
    public Transform destino; // El destino al que quieres mover al jugador

    public AudioSource teleportSound;

    private void Start()
    {
        base.Start();
        message = "Press E to Teleport.";
    }
    public override void Interact()
    {
        MoverJugadorADestino(this.gameObject);
        player_Animator.Play("Teleport");
        teleportSound.Play();
    }

    // Update is called once per frame
    private void OnTriggerEnter(Collider other)
    {
        //Debug.Log("Object Enter");

        if (other.gameObject.tag == "Portal")
        {
            //Debug.Log("Object Enter");

            if (other.gameObject.GetComponent<Portal>() != null)
            {
                InteractMeessage.SetActive(true);
                objCollider = other;
                canInteract = true;
            }
        }
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