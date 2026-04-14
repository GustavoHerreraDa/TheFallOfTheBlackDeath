
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles teleport player for the current project workflow.
/// </summary>
public class TeleportPlayer : Interactable
{
    public GameObject destino; // El destino al que quieres mover al jugador

    public AudioSource teleportSound;

    public bool gotoSceneWorld;

    private string messageToWorld;

    /// <summary>
    /// Initializes the component once the scene dependencies are ready.
    /// </summary>
    private void Start()
    {
        base.Start();
        message = "Press E to Teleport.";
        messageToWorld = "Press E to go to the Mission.";
    }
    /// <summary>
    /// Executes the interact workflow.
    /// </summary>
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
    /// <summary>
    /// Responds to the corresponding Unity trigger callback for this component.
    /// </summary>
    /// <param name="other">The other.</param>
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
    /// <summary>
    /// Executes the mover jugador a destino workflow.
    /// </summary>
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

    /// <summary>
    /// Executes the goto scene workflow.
    /// </summary>
    void GotoScene()
    {
        SceneManager.LoadScene(1);
    }
}
