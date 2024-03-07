
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoreInteract : Interactable
{
    public TextMeshProUGUI titleLoreMessage;
    public TMP_InputField loreDescription;

    public GameObject LoreMessage;

    private void Start()
    {
        base.Start();
        message = "Press E to Read.";

    }
    public override void Interact()
    {
        InteractMeessage.SetActive(false);

        LoreMessage.SetActive(true);
    }

    // Update is called once per frame
    private void OnTriggerEnter(Collider other)
    {
        //Debug.Log("Object Enter");

        if (other.gameObject.tag == "LoreDocument")
        {
            //Debug.Log("Object Enter");

            if (other.gameObject.GetComponent<LoreDocument>() != null)
            {
                var LoreDocument = other.gameObject.GetComponent<LoreDocument>();

                titleLoreMessage.text = LoreDocument.TextTitleLore;
                loreDescription.text = LoreDocument.TextLore;

                nameMessage.text = message;
                InteractMeessage.SetActive(true);
                objCollider = other;
                canInteract = true;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        LoreMessage.SetActive(false);
    }

}