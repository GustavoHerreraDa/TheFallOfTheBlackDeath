
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles lore interact for the current project workflow.
/// </summary>
/*public class LoreInteract : Interactable
{
    public TextMeshProUGUI titleLoreMessage;
    public TMP_InputField loreDescription;

    public GameObject LoreMessage;

    /// <summary>
    /// Initializes the component once the scene dependencies are ready.
    /// </summary>
    private void Start()
    {
        base.Start();
        message = "Press E to Read.";

    }
    /// <summary>
    /// Executes the interact workflow.
    /// </summary>
    public override void Interact()
    {
        InteractMeessage.SetActive(false);

        LoreMessage.SetActive(true);
    }

    // Update is called once per frame
    /// <summary>
    /// Responds to the corresponding Unity trigger callback for this component.
    /// </summary>
    /// <param name="other">The other.</param>
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

    /// <summary>
    /// Responds to the corresponding Unity trigger callback for this component.
    /// </summary>
    /// <param name="other">The other.</param>
    private void OnTriggerExit(Collider other)
    {
        LoreMessage.SetActive(false);
    }

}
*/