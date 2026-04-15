using UnityEngine;

public class FighterHeadTrack : MonoBehaviour
{
    [Header("Referencias")]
    public Transform headBone;          // El hueso de la cabeza/cuello
    private Camera mainCamera;

    [Header("Ajustes de Sutileza")]
    [Range(0f, 20f)] public float maxLookAngle = 10f; // Mucho más bajo para que sea sutil
    public float smoothSpeed = 2f;                   // Velocidad lenta para que se sienta pesado
    public float mouseSensitivity = 0.5f;            // Filtro para que el mouse no afecte tanto

    private Quaternion initialRotation;
    private Quaternion targetRotation;

    void Start()
    {
        // 1. Obtención automática de la cámara
        if (CameraManager.Instance != null && CameraManager.Instance.mainCamera != null)
            mainCamera = CameraManager.Instance.mainCamera;
        else
            mainCamera = Camera.main;

        // 2. Guardamos la rotación original de la animación
        if (headBone != null)
            initialRotation = headBone.localRotation;
    }

    void LateUpdate()
    {
        if (headBone == null || mainCamera == null) return;

        // 3. Calculamos la posición del mouse normalizada (-1 a 1)
        float mouseX = (Input.mousePosition.x / Screen.width) * 2f - 1f;
        float mouseY = (Input.mousePosition.y / Screen.height) * 2f - 1f;

        // 4. Creamos un offset sutil basado en el mouse
        // Multiplicamos por mouseSensitivity para reducir el impacto inicial
        float yaw = mouseX * maxLookAngle * mouseSensitivity;
        float pitch = -mouseY * maxLookAngle * mouseSensitivity;

        // 5. Construimos la rotación local deseada
        Quaternion lookOffset = Quaternion.Euler(pitch, yaw, 0f);
        
        // 6. Interpolación muy suave
        targetRotation = Quaternion.Slerp(targetRotation, lookOffset, Time.deltaTime * smoothSpeed);

        // 7. Aplicamos sobre la rotación que ya trae la animación
        // Esto hace que la cabeza siga animada pero con el "vicio" de mirar al mouse
        headBone.localRotation *= targetRotation;
    }
}