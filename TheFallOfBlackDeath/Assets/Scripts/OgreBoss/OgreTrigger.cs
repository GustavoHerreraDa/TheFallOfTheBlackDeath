using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles ogre trigger for the current project workflow.
/// </summary>
public class OgreTrigger : MonoBehaviour
{
    public GameObject Ogre;

    /// <summary>
    /// Responds to the corresponding Unity trigger callback for this component.
    /// </summary>
    /// <param name="other">The other.</param>
    void OnTriggerEnter(Collider other)
    {
        if (Ogre != null)
            Ogre.SetActive(true);
    }
}
