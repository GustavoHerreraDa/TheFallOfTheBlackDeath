using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickObj : MonoBehaviour
{

    public GameObject PickUpMeessage;
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
                if (hitinfo.collider.gameObject.tag == "Object")
                {
                    if (hitinfo.collider.GetComponent<statsOBJ>() != null)
                    {
                        Debug.Log("a");
                        statsOBJ i = hitinfo.collider.GetComponent<statsOBJ>();
                        InventoryManager.instance.AddItem(i.id, i.amount);
                        PickUpMeessage.SetActive(false);
                    }
                    Destroy(hitinfo.collider.gameObject);
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Object Enter");

        if (other.gameObject.tag == "Object")
        {
            Debug.Log("Object Enter");

            if (other.gameObject.GetComponent<statsOBJ>() != null)
            {
                PickUpMeessage.SetActive(true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        PickUpMeessage.SetActive(false);
    }

    private void OnTriggerStay(Collider other)
    {
        Debug.Log(other.gameObject.name);

    }
}
