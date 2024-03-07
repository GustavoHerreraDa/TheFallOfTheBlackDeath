using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OgreTrigger : MonoBehaviour
{
    public GameObject Ogre;

    void OnTriggerEnter(Collider other)
    {
        if (Ogre != null)
            Ogre.SetActive(true);
    }
}
