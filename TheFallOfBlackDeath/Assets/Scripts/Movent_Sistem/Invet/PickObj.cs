using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickObj : MonoBehaviour
{
    public Animator Anim;
    public GameObject PickUpMeessage;
    public PlayerControl playerControl;
    [SerializeField]
    private bool canPickup;
    Collider objCollider;

    void Start()
    {
        PickUpMeessage.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (!canPickup)
                return;
            else
            {
                Anim.Play("Pick");
                statsOBJ i = objCollider.GetComponent<statsOBJ>();
                InventoryManager.instance.AddItem(i.id, i.amount);
                playerControl.StopPlayer();
                StartCoroutine(WaitOneSecond());
                PickUpMeessage.SetActive(false);
                canPickup = false;
            }

            //Ray ray = new Ray(transform.position, transform.forward);
            //RaycastHit hitinfo;

            //if (Physics.Raycast(ray, out hitinfo, 2f))
            //{
            //    if (hitinfo.collider.gameObject.tag == "Object")
            //    {
            //        Anim.Play("Pick");

            //        playerControl.StopPlayer();
            //        Debug.Log("Raycast Entro");
            //        statsOBJ i = hitinfo.collider.GetComponent<statsOBJ>();
            //        InventoryManager.instance.AddItem(i.id, i.amount);
            //        StartCoroutine(WaitOneSecond());
            //        PickUpMeessage.SetActive(false);
            //    }
            //}
        }

        ////    if (Physics.Raycast(ray, out hitinfo, 2f))
        ////    {
        ////        if (hitinfo.collider.gameObject.tag == "Gate")
        ////        {
        ////            if (hitinfo.collider.GetComponent<Gate>() != null)
        ////            {
        ////                Anim.Play("OpenDoor");
        ////                playerControl.stop = true;
        ////                Debug.Log("Open Door");
        ////                StartCoroutine(WaitOneSecond());

        ////                if (InventoryManager.instance.HasItemInIventory(8, 1))
        ////                    Destroy(hitinfo.collider.gameObject);

        ////                playerControl.stop = false;

        ////            }

        ////        }
        ////    }
        ////}
    }

    IEnumerator WaitOneSecond()
    {
        yield return new WaitForSeconds(1f);

        // Aquí puedes agregar el código que deseas ejecutar después de esperar un segundo

        // Por ejemplo:
        Debug.Log("Han pasado 3 segundo");
        playerControl.ContinuePlayer();
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
                objCollider = other;
                canPickup = true;
            }
        }
    }



    //void OnCollisionEnter(Collision collision)
    //{
    //    Debug.Log("Object Enter " + collision.gameObject.tag);

    //    if (collision.gameObject.tag == "Object")
    //    {

    //        if (collision.gameObject.GetComponent<statsOBJ>() != null)
    //        {
    //            PickUpMeessage.SetActive(true);
    //        }
    //    }
    //}

    private void OnTriggerExit(Collider other)
    {
        PickUpMeessage.SetActive(false);
        canPickup = false;

    }


}
