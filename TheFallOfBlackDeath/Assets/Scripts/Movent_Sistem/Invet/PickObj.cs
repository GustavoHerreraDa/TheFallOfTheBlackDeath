using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickObj : Interactable
{
    public AudioSource pickSound;

    private void Start()
    {
        base.Start();
        message = "Press E to pick up item.";
    }
    public override void Interact()
    {
        player_Animator.Play("Pick");
        pickSound.Play();
        statsOBJ i = objCollider.GetComponent<statsOBJ>();

        InventoryManager.instance.AddItem(i.id, i.amount, i.uso);
        playerControl.StopPlayer(1.3f);
        InteractMeessage.SetActive(false);
        canInteract = false;
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
