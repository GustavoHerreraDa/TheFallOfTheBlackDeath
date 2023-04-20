using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera_Main : MonoBehaviour
{
    private Vector2 angle = new Vector2(90 * Mathf.Deg2Rad,0);
    
    [SerializeField] private Transform Follow;
    [SerializeField] private float Distance;
    [SerializeField] private float CameraAngleYPOS;
    [SerializeField] private float CameraAngleYNEG;
    


    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;

    }

    void Update()
    {
        float Horizontal = Input.GetAxis("Mouse X");

        if (Horizontal != 0)
        {
            angle.x += Horizontal * Mathf.Deg2Rad;
            
        }

        float Vertical = Input.GetAxis("Mouse Y");

        if (Vertical != 0)
        {
            angle.y += Vertical * Mathf.Deg2Rad;
            angle.y = Mathf.Clamp(angle.y, - CameraAngleYPOS * Mathf.Deg2Rad, CameraAngleYNEG * Mathf.Deg2Rad);
        }
    }

    void LateUpdate()
    {
        Vector3 orbit = new Vector3(Mathf.Cos(angle.x) * Mathf.Cos(angle.y), - Mathf.Sin(angle.y), - Mathf.Sin(angle.x) * Mathf.Cos(angle.y));

        
         
        transform.position = Follow.position + orbit * Distance;
        transform.rotation = Quaternion.LookRotation(Follow.position - transform.position);
        
        
        
        
    }
}
