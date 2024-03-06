
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TeleportPlayer : Interactable
{
    public GameObject destino; // El destino al que quieres mover al jugador

    public AudioSource teleportSound;

    public bool gotoSceneWorld;

    private string messageToWorld;

    private void Start()
    {
        base.Start();
        message = "Press E to Teleport.";
        messageToWorld = "Press E to go to the Mission.";
    }
    public override void Interact()
    {
        if (gotoSceneWorld)
        {
            GotoScene();
        }
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
                var portal = other.gameObject.GetComponent<Portal>();
                if (portal.gotoWorld)
                {
                    nameMessage.text = messageToWorld;
                    gotoSceneWorld = true;
                }
                else
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
            gameObject.GetComponent<PlayerControl>().TeleportPlayer(destino.transform.position);

        }
        else
        {
            Debug.LogError("El destino no está asignado. Asigna un objeto de destino en el Inspector.");
        }
    }

    void GotoScene()
    {
        SceneManager.LoadScene(1);
    }
}