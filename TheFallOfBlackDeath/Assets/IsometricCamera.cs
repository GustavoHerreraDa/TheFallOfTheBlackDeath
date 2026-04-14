using UnityEngine;

/// <summary>
/// Handles isometric camera for the current project workflow.
/// </summary>
public class IsometricCamera : MonoBehaviour
{
    [Header("Target")]
    public Transform target; // El jugador u objeto a seguir

    [Header("Offsets")]
    public Vector3 offset = new Vector3(0, 10, -10); // Posición respecto al jugador
    public float rotationAngle = 45f; // Ángulo isométrico (45° típico)

    [Header("Movimiento")]
    public float followSpeed = 5f;

    [Header("Rotación manual (opcional)")]
    public bool allowRotation = false;
    public float rotationSpeed = 70f;

    /// <summary>
    /// Initializes the component once the scene dependencies are ready.
    /// </summary>
    private void Start()
    {
        // Inclinamos la cámara para vista isométrica
        transform.rotation = Quaternion.Euler(45f, rotationAngle, 0f);
    }

    /// <summary>
    /// Applies late-frame adjustments after the main update loop has completed.
    /// </summary>
    private void LateUpdate()
    {
        if (target == null) return;

        // Permitir rotar la cámara con el mouse
        if (allowRotation)
        {
            float horizontal = Input.GetAxis("Mouse X");
            if (Mathf.Abs(horizontal) > 0.01f)
            {
                rotationAngle += horizontal * rotationSpeed * Time.deltaTime;
            }
        }

        Quaternion rotation = Quaternion.Euler(45f, rotationAngle, 0f);
        Vector3 desiredPosition = target.position + rotation * offset;

        transform.position = Vector3.Lerp(transform.position, desiredPosition, followSpeed * Time.deltaTime);
        transform.rotation = rotation;
    }
}
