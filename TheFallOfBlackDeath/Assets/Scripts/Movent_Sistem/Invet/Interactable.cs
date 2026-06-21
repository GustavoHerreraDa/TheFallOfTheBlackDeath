using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Collections;

/// <summary>
/// Supports inventory and interaction flow by handling interactable.
/// </summary>
public abstract class Interactable : MonoBehaviour
{
    public Animator player_Animator;
    public PlayerControl playerControl;
    public GameObject ResponseMessage;
    [SerializeField]
    internal bool canInteract;
    internal Collider objCollider;
    public TMP_InputField input_responseMessage;
    internal string message;
    internal string responseMessage;

    public abstract void Interact();

    /// <summary>
    /// Initializes the component once the scene dependencies are ready.
    /// </summary>
    public virtual void Start()
    {
        if (ResponseMessage != null) ResponseMessage.SetActive(false);
    }

    /// <summary>
    /// Responds to the corresponding Unity trigger callback for this component.
    /// </summary>
    /// <param name="other">The other.</param>
    protected virtual void OnTriggerEnter(Collider other)
    {
        // OBJETO (pickup)
        if (other.gameObject.CompareTag("Object"))
        {
            // MIGRADO: reemplaza InteractMeessage
            InteractionPromptUI.Instance?.Show("[ E ] Recoger ítem");
            objCollider = other;
            canInteract = true;
            Debug.Log("apreta e para interactuar");
            return;
        }

        // PUERTA
        if (other.gameObject.CompareTag("Gate"))
        {
            // MIGRADO: reemplaza InteractMeessage
            InteractionPromptUI.Instance?.Show("[ E ] Abrir puerta");
            objCollider = other;
            canInteract = true;
            Debug.Log("apreta e para interactuar");
            return;
        }

        // PORTAL
        if (other.gameObject.CompareTag("Portal"))
        {
            if (other.GetComponent<Portal>() != null)
            {
                // MIGRADO: reemplaza InteractMeessage
                InteractionPromptUI.Instance?.Show("[ E ] Usar portal");
                objCollider = other;
                canInteract = true;
                Debug.Log("apreta e para interactuar");
                return;
            }
        }

        // NPC
        if (other.gameObject.CompareTag("NPC"))
        {
            if (other.GetComponent<DialogueInteractable>() != null)
            {
                // MIGRADO: reemplaza InteractMeessage
                InteractionPromptUI.Instance?.Show("[ E ] Hablar");
                objCollider = other;
                canInteract = true;
                Debug.Log("apreta e para interactuar");
                return;
            }
        }
    }


    /// <summary>
    /// Responds to the corresponding Unity trigger callback for this component.
    /// </summary>
    /// <param name="other">The other.</param>
    private void OnTriggerExit(Collider other)
    {
        // MIGRADO: reemplaza InteractMeessage
        InteractionPromptUI.Instance?.Hide();
        canInteract = false;
    }

    /// <summary>
    /// Updates the component each frame while it is active.
    /// </summary>
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (!canInteract)
                return;
            else
            {
                // ---> NUEVA LÓGICA AÑADIDA <---
                // Ocultar el panel inmediatamente al iniciar la interacción
                InteractionPromptUI.Instance?.Hide();
                this.Interact();
            }
        }
    }

    /// <summary>
    /// Shows the response message.
    /// </summary>
    public void ShowResponseMessage()
    {
        Debug.Log("Mostrame la descripcion del item ctm 2");

        if (ResponseMessage != null)
        {
            ResponseMessage.SetActive(true);
            if (input_responseMessage != null) input_responseMessage.text = responseMessage;
            StartCoroutine(DissableResponseMessage());
        }
    }

    /// <summary>
    /// Executes the dissable response message workflow.
    /// </summary>
    /// <returns>An enumerator that drives the coroutine sequence.</returns>
    IEnumerator DissableResponseMessage()
    {
        yield return new WaitForSeconds(2f);
        if (ResponseMessage != null) ResponseMessage.SetActive(false);
    }
}