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
    [SerializeField]
    internal bool canInteract;
    internal Collider objCollider;
    public TMP_InputField nameMessage;
    internal string message;

    public abstract void Interact();

    public virtual void Start()
    {
        InteractMeessage.SetActive(false);
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
}
