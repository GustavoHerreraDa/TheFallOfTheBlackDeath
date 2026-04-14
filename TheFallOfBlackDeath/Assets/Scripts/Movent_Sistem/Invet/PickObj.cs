using Assets.Scripts.Movent_Sistem.Invet;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Supports inventory and interaction flow by handling pick obj.
/// </summary>
public class PickObj : Interactable, IInteratable
{
    public AudioSource pickSound;
    public const string PickObjMessage = "You pick up a #objectName. Press key I to checkout.";
    /// <summary>
    /// Initializes the component once the scene dependencies are ready.
    /// </summary>
    private void Start()
    {
        base.Start();
        message = "Press E to pick up item.";
        responseMessage = PickObjMessage;
    }
    /// <summary>
    /// Executes the interact workflow.
    /// </summary>
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
