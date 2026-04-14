using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Supports exploration and world-state flow by handling foot steps.
/// </summary>
public class FootSteps : MonoBehaviour
{
    public AudioSource footstepsSound, sprintSound;

    /// <summary>
    /// Updates the component each frame while it is active.
    /// </summary>
    void Update()
    {
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D))
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                footstepsSound.enabled = false;
                sprintSound.enabled = true;

            }
            else
            {
                footstepsSound.enabled = true;
                sprintSound.enabled = false;
            }
        }
        else
        {
            footstepsSound.enabled = false;


        }
    }
}
