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
    public GameObject InteractMeessage;
    public GameObject ResponseMessage;
    [SerializeField]
    internal bool canInteract;
    internal Collider objCollider;
    public TMP_InputField nameMessage;
    public TMP_InputField input_responseMessage;
    internal string message;
    internal string responseMessage;

    public abstract void Interact();

    /// <summary>
    /// Initializes the component once the scene dependencies are ready.
    /// </summary>
    public virtual void Start()
    {
        InteractMeessage.SetActive(false);
        ResponseMessage.SetActive(false);
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
            
            {
                InteractMeessage.SetActive(true);
                objCollider = other;
                canInteract = true;
                nameMessage.text = "Press E to pick up item.";
                Debug.Log("apreta e para interactuar");
                return;
            }
        }

        // PUERTA
        if (other.gameObject.CompareTag("Gate"))
        {
            
            {
                InteractMeessage.SetActive(true);
                objCollider = other;
                canInteract = true;
                nameMessage.text = "Press E to open gate.";
                Debug.Log("apreta e para interactuar");
                return;
            }
        }

        // PORTAL
        if (other.gameObject.CompareTag("Portal"))
        {
            if (other.GetComponent<Portal>() != null)
            {
                InteractMeessage.SetActive(true);
                objCollider = other;
                canInteract = true;
                nameMessage.text = "Press E to use portal.";
                Debug.Log("apreta e para interactuar");
                return;
            }
        }

        // NPC
        if (other.gameObject.CompareTag("NPC"))
        {
            if (other.GetComponent<DialogueInteractable>() != null)
            {
                InteractMeessage.SetActive(true);
                objCollider = other;
                canInteract = true;
                nameMessage.text = "Press E to talk";
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
        InteractMeessage.SetActive(false);
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
                this.Interact();
            }
        }
    }

    /// <summary>
    /// Shows the response message.
    /// </summary>
    public void ShowResponseMessage()
    {
        //Debug.Log("No se puede abrir cosa");
        Debug.Log("Mostrame la descripcion del item ctm 2");

        ResponseMessage.SetActive(true);
        input_responseMessage.text = responseMessage;
        StartCoroutine(DissableResponseMessage());
    }
    /// <summary>
    /// Executes the dissable response message workflow.
    /// </summary>
    /// <returns>An enumerator that drives the coroutine sequence.</returns>
    IEnumerator DissableResponseMessage()
    {
        yield return new WaitForSeconds(2f);
        ResponseMessage.SetActive(false);
        InteractMeessage.SetActive(false);

    }
}
