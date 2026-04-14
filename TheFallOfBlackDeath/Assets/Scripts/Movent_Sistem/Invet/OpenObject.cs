using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Movent_Sistem.Invet
{
    /// <summary>
    /// Supports inventory and interaction flow by handling open object.
    /// </summary>
    public class OpenObject : Interactable
    {
        //Animator animator;
        /// <summary>
        /// Initializes the component once the scene dependencies are ready.
        /// </summary>
        private void Start()
        {
            base.Start();
            message = "Press E to open.";
            responseMessage = "You need a Key to open the Gate.";
        }


        /// <summary>
        /// Executes the interact workflow.
        /// </summary>
        public override void Interact()
        {
            //animator.SetBool("IsOpen", true);
            Gate i = objCollider.GetComponent<Gate>();
            playerControl.StopPlayer(2f);
            i.OpenGate();
            player_Animator.Play("Open");
         
            if (i.IsNeedKey == true)
            {
                player_Animator.Play("key");
                ShowResponseMessage();
            }

        else

        {
                
                InteractMeessage.SetActive(false);
                canInteract = false;
                nameMessage.text = message;
            }

        }

        /// <summary>
        /// Responds to the corresponding Unity trigger callback for this component.
        /// </summary>
        /// <param name="other">The other.</param>
        private void OnTriggerEnter(Collider other)
        {
            //Debug.Log("Open Object Enter");
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
