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

    private void OnTriggerEnter(Collider other)
    {
        //Debug.Log("Object Enter");

        if (other.gameObject.tag == "Object")
        {
            //Debug.Log("Object Enter");

            if (other.gameObject.GetComponent<statsOBJ>() != null)
            {
                InteractMeessage.SetActive(true);
                objCollider = other;
                canInteract = true;
                nameMessage.text = message;
            }
        }
        if (other.gameObject.tag == "Gate")
        {
            if (other.gameObject.GetComponent<Gate>() != null)
            {
                InteractMeessage.SetActive(true);
                objCollider = other;
                canInteract = true;
                nameMessage.text = message;
            }
        }
        if (other.gameObject.tag == "Portal")
        {
            if (other.gameObject.GetComponent<Portal>() != null)
            {
                InteractMeessage.SetActive(true);
                objCollider = other;
                canInteract = true;
                nameMessage.text = message;
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
