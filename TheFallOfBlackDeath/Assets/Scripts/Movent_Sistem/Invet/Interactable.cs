using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Collections;

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

    public virtual void Start()
    {
        InteractMeessage.SetActive(false);
        ResponseMessage.SetActive(false);
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        // OBJETO (pickup)
        if (other.gameObject.CompareTag("Object"))
        {
            if (other.GetComponent<statsOBJ>() != null)
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
            if (other.GetComponent<Gate>() != null)
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


    private void OnTriggerExit(Collider other)
    {
        InteractMeessage.SetActive(false);
        canInteract = false;

    }

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

    public void ShowResponseMessage()
    {
        //Debug.Log("No se puede abrir cosa");
        Debug.Log("Mostrame la descripcion del item ctm 2");

        ResponseMessage.SetActive(true);
        input_responseMessage.text = responseMessage;
        StartCoroutine(DissableResponseMessage());
    }
    IEnumerator DissableResponseMessage()
    {
        yield return new WaitForSeconds(2f);
        ResponseMessage.SetActive(false);
        InteractMeessage.SetActive(false);

    }
}
