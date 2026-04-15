using UnityEngine;

/// <summary>
/// Maneja los efectos visuales de la cámara específicamente para el menú principal.
/// </summary>
public class MenuCameraEffects : MonoBehaviour
{
    [Header("Mouse Tracking Settings")]
    public float maxRotationAngle = 3f;      // Qué tanto rotará la cámara
    public float maxPositionOffset = 0.2f;   // El leve deslizamiento lateral (como el video)
    public float smoothing = 5f;             // Suavidad del movimiento

    [Header("Breathing Effect")]
    public bool enableBreathing = true;
    public float breathSpeed = 1.5f;
    public float breathAmplitude = 0.3f;

    private Vector3 startPosition;
    private Quaternion startRotation;
    private float breathTimer;

    void Start()
    {
        // Guardamos la posición y rotación inicial del menú
        startPosition = transform.localPosition;
        startRotation = transform.localRotation;
    }

    void Update()
    {
        ApplyMouseTracking();
        if (enableBreathing) ApplyBreathing();
    }

    private void ApplyMouseTracking()
    {
        // 1. Obtenemos la posición del mouse normalizada (-1 a 1)
        float mouseX = (Input.mousePosition.x / Screen.width) * 2f - 1f;
        float mouseY = (Input.mousePosition.y / Screen.height) * 2f - 1f;

        // 2. Calculamos la rotación objetivo
        Quaternion targetRotation = startRotation * Quaternion.Euler(-mouseY * maxRotationAngle, mouseX * maxRotationAngle, 0f);

        // 3. Calculamos el desplazamiento de posición (el "deslize")
        Vector3 targetPosition = startPosition + new Vector3(mouseX * maxPositionOffset, mouseY * maxPositionOffset, 0f);

        // 4. Aplicamos ambos suavemente
        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, Time.deltaTime * smoothing);
        transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, Time.deltaTime * smoothing);
    }

    private void ApplyBreathing()
    {
        breathTimer += Time.deltaTime * breathSpeed;
        float offset = Mathf.Sin(breathTimer) * breathAmplitude;
        
        // Sumamos un pequeño pulso a la posición actual para que el menú "viva"
        transform.localPosition += Vector3.up * (offset * Time.deltaTime);
    }
}