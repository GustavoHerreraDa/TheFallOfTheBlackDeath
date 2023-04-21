using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movent : MonoBehaviour
{
  
    [SerializeField] private Rigidbody RGB;
    [SerializeField] private int Speed;
    [SerializeField] private Transform Camera;
    [SerializeField] private float Transition;

    private Animator Anim;

    float movent = 0;
    

    void Start()
    {
        Anim = GetComponentInChildren<Animator>();
    }

    private void FixedUpdate()
    {
        float Horizontal = Input.GetAxis("Horizontal");
        float Vertical = Input.GetAxis("Vertical");
        Vector3 movement = Vector3.zero;

        if (Horizontal !=0 || Vertical !=0)
        {
            

            Vector3 forward = Camera.forward;
            forward.y = 0;
            forward.Normalize();

            Vector3 right = Camera.right;
            right.y = 0;
            right.Normalize();

            Vector3 direction = forward * Vertical + right * Horizontal;
            movent = Mathf.Clamp01(direction.magnitude);
            direction.Normalize();

            movement = direction * Speed * Time.deltaTime;

            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), Transition); 


        }

        
        RGB.velocity = movement;

        Anim.SetFloat("Movent", movent);



    }

}