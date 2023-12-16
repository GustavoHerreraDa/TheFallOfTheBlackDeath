using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OgreTrigger : MonoBehaviour
{
    public GameObject Ogre;
    
    void OnTriggerEnter(Collider other)
    {
        Ogre.SetActive(true);
    }
}
