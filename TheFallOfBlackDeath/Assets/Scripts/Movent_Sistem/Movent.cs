using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movent : MonoBehaviour
{

    [SerializeField] private Rigidbody RGB;
    [SerializeField] private int Speed;
    [SerializeField] private Transform Camera;
    [SerializeField] private float Transition;

    void Start()
    {
        
    }

    private void FixedUpdate()
    {
        float Horizontal = Input.GetAxis("Horizontal");
        float Vertical = Input.GetAxis("Vertical");


        if (Horizontal !=0 || Vertical !=0)
        {
            Vector3 forward = Camera.forward;
            forward.y = 0;
            forward.Normalize();

            Vector3 right = Camera.right;
            right.y = 0;
            right.Normalize();

            Vector3 direction = forward * Vertical + right * Horizontal;
            direction.Normalize();

            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), Transition); 


            

        }

        Vector3 input = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        input.Normalize();
        RGB.velocity = input * Time.deltaTime * Speed;




    }

}
