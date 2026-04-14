using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Supports exploration and world-state flow by handling camera main.
/// </summary>
public class Camera_Main : MonoBehaviour
{
    private Vector2 angle = new Vector2(0 * Mathf.Deg2Rad, 0);

    [SerializeField] private Transform Follow;
    [SerializeField] private float Distance;
    [SerializeField] private float CameraAngleYPOS;
    [SerializeField] private float CameraAngleYNEG;

    [Header("Collision")]
    [SerializeField] private LayerMask collisionMask;
    [SerializeField] private float cameraRadius = 0.3f;
    [SerializeField] private float minDistance = 1f;

    /// <summary>
    /// Initializes the component once the scene dependencies are ready.
    /// </summary>
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    /// <summary>
    /// Updates the component each frame while it is active.
    /// </summary>
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
            angle.y = Mathf.Clamp(angle.y, -CameraAngleYPOS * Mathf.Deg2Rad, CameraAngleYNEG * Mathf.Deg2Rad);
        }
    }

    /// <summary>
    /// Applies late-frame adjustments after the main update loop has completed.
    /// </summary>
    void LateUpdate()
    {
        Vector3 orbit = new Vector3(
            Mathf.Cos(angle.x) * Mathf.Cos(angle.y),
            -Mathf.Sin(angle.y),
            -Mathf.Sin(angle.x) * Mathf.Cos(angle.y)
        );

        Vector3 desiredPosition = Follow.position + orbit * Distance;

        RaycastHit hit;
        
        if (Physics.SphereCast(Follow.position, cameraRadius, orbit, out hit, Distance, collisionMask))
        {
            float adjustedDistance = Mathf.Clamp(hit.distance - 0.2f, minDistance, Distance);
            desiredPosition = Follow.position + orbit * adjustedDistance;
        }

        transform.position = desiredPosition;
        transform.rotation = Quaternion.LookRotation(Follow.position - transform.position);
    }
}
