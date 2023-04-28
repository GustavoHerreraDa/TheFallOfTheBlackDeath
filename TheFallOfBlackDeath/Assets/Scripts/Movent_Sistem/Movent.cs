using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Movent : MonoBehaviour
{
  
    [SerializeField] private Rigidbody RGB;
    [SerializeField] private int Speed;
    [SerializeField] private Transform Camera;
    [SerializeField] private float Transition;
    public GameObject itemIconPrefab;
    public Transform inventoryContent;
    private List<GameObject> uiInventory;
    private List<Item> inventory;
    private int potions = 0;
    private void Awake()
    {
        inventory = new List<Item>();
        uiInventory = new List<GameObject>();
    }



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
