using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickObj : MonoBehaviour
{
    
    
    void Start()
    {  
        
    }

    // Update is called once per frame
    void Update()
    {
        
        if (Input.GetKeyDown(KeyCode.E))
        {
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
                        InventoryManager.instance.AddItem(i.id, i.amount);
                    }
                    Destroy(hitinfo.collider.gameObject);
                }
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        Debug.Log(other.gameObject.name);
        if (other.gameObject.tag == "Object" && Input.GetKeyDown(KeyCode.E))
        {
            if (other.gameObject.GetComponent<statsOBJ>() != null)
            {
                Debug.Log("a");
                statsOBJ i = other.gameObject.GetComponent<statsOBJ>();
                InventoryManager.instance.AddItem(i.id, i.amount);
            }
            Destroy(other.gameObject);
        }
    }
}
