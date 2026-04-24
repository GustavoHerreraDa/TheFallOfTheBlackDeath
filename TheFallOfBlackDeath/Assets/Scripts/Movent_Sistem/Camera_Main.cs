using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Supports exploration and world-state flow by handling camera main.
/// </summary>
public class Camera_Main : MonoBehaviour
{
    private Vector2 angle = new Vector2(0f, 0f);

    public bool IsInspectingCharacter =>
        Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);

    [SerializeField] private Transform Follow;
    [SerializeField] private float Distance = 5f;
    [SerializeField] private float CameraAngleYPOS;
    [SerializeField] private float CameraAngleYNEG;

    [Header("Zoom Settings")]
    [SerializeField] private float zoomSpeed = 2f;
    [SerializeField] private float minZoomDist = 2f;
    [SerializeField] private float maxZoomDist = 15f;

    [Header("Collision")]
    [SerializeField] private LayerMask collisionMask;
    [SerializeField] private float cameraRadius = 0.3f;
    [SerializeField] private float minDistance = 1f;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        float horizontal = Input.GetAxis("Mouse X");
        if (horizontal != 0f)
        {
            angle.x += horizontal * Mathf.Deg2Rad;
        }

        float vertical = Input.GetAxis("Mouse Y");
        if (vertical != 0f)
        {
            angle.y += vertical * Mathf.Deg2Rad;
            angle.y = Mathf.Clamp(angle.y, -CameraAngleYPOS * Mathf.Deg2Rad, CameraAngleYNEG * Mathf.Deg2Rad);
        }

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0f)
        {
            Distance -= scroll * zoomSpeed;
            Distance = Mathf.Clamp(Distance, minZoomDist, maxZoomDist);
        }
    }

    private void LateUpdate()
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
