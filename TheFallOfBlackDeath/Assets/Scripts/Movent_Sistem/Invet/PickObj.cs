using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickObj : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            //Debug.Log("A");
            


            Ray ray = new Ray(transform.position, transform.forward);
            RaycastHit hitinfo;

            if (Physics.Raycast(ray, out hitinfo))
            {
                if (hitinfo.collider.gameObject.tag == "Object")
                {
                    Destroy(hitinfo.collider.gameObject);
                }
            }
        }
    }

    

}
