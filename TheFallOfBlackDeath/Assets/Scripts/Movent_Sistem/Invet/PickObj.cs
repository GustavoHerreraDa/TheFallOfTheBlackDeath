using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickObj : MonoBehaviour
{


    InventoryManager m;
    statsOBJ o;
    
    
    
    void Start()
    {
        o = GetComponent<statsOBJ>();
        m = GetComponent<InventoryManager>();
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
                if (hitinfo.collider.gameObject.tag == "Object" )
                {
                    if (hitinfo.collider.GetComponent<statsOBJ>() != null)                
                    {
                        Debug.Log("a");
                        statsOBJ i = hitinfo.collider.GetComponent<statsOBJ>();
                        m.AddItem(i.id, i.amount);
                    }
                    
                    
                    Destroy(hitinfo.collider.gameObject);
                }
            }
        }
    }

    

}
