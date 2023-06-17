using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickObj : MonoBehaviour
{
    public Animator Anim;
    public GameObject PickUpMeessage;
    public PlayerControl playerControl;


    void Start()
    {
        PickUpMeessage.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetKeyDown(KeyCode.E))
        {
            Ray ray = new Ray(transform.position, transform.forward);
            RaycastHit hitinfo;

            if (Physics.Raycast(ray, out hitinfo, 2f))
            {
                if (hitinfo.collider.gameObject.tag == "Object")
                {
                    if (hitinfo.collider.GetComponent<statsOBJ>() != null)
                    {

                        Anim.Play("Pick");
                        playerControl.stop = true;
                        Debug.Log("a");
                        statsOBJ i = hitinfo.collider.GetComponent<statsOBJ>();
                        InventoryManager.instance.AddItem(i.id, i.amount);
                        StartCoroutine(WaitOneSecond());
                        playerControl.stop = false;

                        PickUpMeessage.SetActive(false);
                    }
                    Destroy(hitinfo.collider.gameObject);
                }
            }

            if (Physics.Raycast(ray, out hitinfo, 2f))
            {
                if (hitinfo.collider.gameObject.tag == "Gate")
                {
                    if (hitinfo.collider.GetComponent<Gate>() != null)
                    {
                        Anim.Play("OpenDoor");
                        playerControl.stop = true;
                        Debug.Log("Open Door");
                        StartCoroutine(WaitOneSecond());

                        if (InventoryManager.instance.HasItemInIventory(8, 1))
                            Destroy(hitinfo.collider.gameObject);

                        playerControl.stop = false;

                    }

                }
            }
        }
    }

    IEnumerator WaitOneSecond()
    {
        yield return new WaitForSeconds(0.3f);

        // Aquí puedes agregar el código que deseas ejecutar después de esperar un segundo

        // Por ejemplo:
        Debug.Log("Han pasado 1 segundo");
        // Otras acciones que deseas realizar después de la espera
    }

    private void OnTriggerEnter(Collider other)
    {
        //Debug.Log("Object Enter");

        if (other.gameObject.tag == "Object")
        {
            //Debug.Log("Object Enter");

            if (other.gameObject.GetComponent<statsOBJ>() != null)
            {
                PickUpMeessage.SetActive(true);
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Object Enter " + collision.gameObject.tag);

        if (collision.gameObject.tag == "Object")
        {

            if (collision.gameObject.GetComponent<statsOBJ>() != null)
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
        //Debug.Log(other.gameObject.name);

    }
}
