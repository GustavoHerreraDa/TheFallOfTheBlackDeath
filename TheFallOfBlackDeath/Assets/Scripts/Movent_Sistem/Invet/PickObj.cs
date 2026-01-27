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
        
        if (objCollider == null || objCollider.GetComponent<statsOBJ>() == null)
            return;

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
    /*public override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);

        if (other.CompareTag("Object"))
        {
            if (other.GetComponent<statsOBJ>() != null)
            {
                Debug.Log("PickObj detect� objeto");
                InteractMeessage.SetActive(true);
                objCollider = other;
                canInteract = true;
            }
        }
    }*/


}
