
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportPlayer : Interactable
{
    public GameObject destino; // El destino al que quieres mover al jugador

    public AudioSource teleportSound;

    private void Start()
    {
        base.Start();
        message = "Press E to Teleport.";
    }
    public override void Interact()
    {
        MoverJugadorADestino();
        //player_Animator.Play("Teleport");
        if (teleportSound != null)
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
                nameMessage.text = message;
                InteractMeessage.SetActive(true);
                objCollider = other;
                canInteract = true;
            }
        }
    }
    void MoverJugadorADestino()
    {
        // Mover el jugador al destino
        if (destino != null)
        {
            // Obtener el componente de transformación del jugador
            //Transform jugadorTransform = transform;

            // Establecer la posición del jugador en la posición del destino
            //Debug.Log("Jugador movido a la posición del destino. " + this.gameObject.name);
            //Debug.Log("Jugador movido a la posición del origen. " + this.gameObject.transform.position);
            //Debug.Log("Jugador movido a la posición del destino. " + destino.transform.position);
            //Debug.Log("Jugador movido a la posición del destino. " + destino.gameObject.name);

            //this.gameObject.transform.position = destino.transform.position;

            gameObject.GetComponent<PlayerControl>().TeleportPlayer(destino.transform.position);
           
        }
        else
        {
            Debug.LogError("El destino no está asignado. Asigna un objeto de destino en el Inspector.");
        }
    }
}