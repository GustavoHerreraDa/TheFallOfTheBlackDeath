using Assets.Scripts.Movent_Sistem.Invet;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickObj : Interactable, IInteratable
{
    public AudioSource pickSound;
    public const string PickObjMessage = "You pick up a #objectName. Press key I to checkout.";
    private void Start()
    {
        base.Start();
        message = "Press E to pick up item.";
        responseMessage = PickObjMessage;
    }
    public override void Interact()
    {
        player_Animator.Play("Pick");
        pickSound.Play();
        statsOBJ i = objCollider.GetComponent<statsOBJ>();

        InventoryManager.instance.AddItem(i.id, i.amount, i.uso);
        var objectName = InventoryManager.instance.GetItemInformation(i.id);

        responseMessage = PickObjMessage.Replace("#objectName", objectName.name);

        playerControl.StopPlayer(1.3f);
        canInteract = false;
        ShowResponseMessage();
        Destroy(i.gameObject);
    }

    // Update is called once per frame
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
            }
        }
    }

}
