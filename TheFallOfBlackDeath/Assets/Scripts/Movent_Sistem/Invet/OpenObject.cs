using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Movent_Sistem.Invet
{
    public class OpenObject : Interactable
    {
        //Animator animator;
        private void Start()
        {
            base.Start();
            message = "Press E to open.";
        }


        public override void Interact()
        {
            //animator.SetBool("IsOpen", true);
            Gate i = objCollider.GetComponent<Gate>();
            playerControl.StopPlayer(2f);
            i.OpenGate();
            player_Animator.Play("Open");
            InteractMeessage.SetActive(false);
            canInteract = false;
            nameMessage.text = message;

        }

        private void OnTriggerEnter(Collider other)
        {
            //Debug.Log("Object Enter");
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
    }
}