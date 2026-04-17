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
    [SerializeField] private float Distance = 5f; // Distancia actual
    [SerializeField] private float CameraAngleYPOS;
    [SerializeField] private float CameraAngleYNEG;

    [Header("Zoom Settings")]
    [SerializeField] private float zoomSpeed = 2f;      // Velocidad del zoom
    [SerializeField] private float minZoomDist = 2f;    // Distancia mínima permitida
    [SerializeField] private float maxZoomDist = 15f;   // Distancia máxima permitida

    [Header("Collision")]
    [SerializeField] private LayerMask collisionMask;
    [SerializeField] private float cameraRadius = 0.3f;
    [SerializeField] private float minDistance = 1f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        // --- Rotación con el Mouse ---
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

        // --- Lógica de Zoom ---
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            // Restamos porque un scroll positivo usualmente significa "acercar"
            Distance -= scroll * zoomSpeed;
            // Limitamos la distancia para que no se pase de los rangos
            Distance = Mathf.Clamp(Distance, minZoomDist, maxZoomDist);
        }
    }

    void LateUpdate()
    {
        Vector3 orbit = new Vector3(
            Mathf.Cos(angle.x) * Mathf.Cos(angle.y),
            -Mathf.Sin(angle.y),
            -Mathf.Sin(angle.x) * Mathf.Cos(angle.y)
        );

        Vector3 desiredPosition = Follow.position + orbit * Distance;

        RaycastHit hit;
        
        // Colisión de cámara
        if (Physics.SphereCast(Follow.position, cameraRadius, orbit, out hit, Distance, collisionMask))
        {
            // Si hay algo en medio, la cámara se acerca más allá del zoom actual
            float adjustedDistance = Mathf.Clamp(hit.distance - 0.2f, minDistance, Distance);
            desiredPosition = Follow.position + orbit * adjustedDistance;
        }

        transform.position = desiredPosition;
        transform.rotation = Quaternion.LookRotation(Follow.position - transform.position);
    }
}