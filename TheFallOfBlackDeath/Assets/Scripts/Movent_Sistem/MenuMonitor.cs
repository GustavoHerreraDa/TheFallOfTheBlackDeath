using UnityEngine;

public class MenuMonitor : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform swayPivot;
    [SerializeField] private Transform meshMonitor;
    
    [Tooltip("El Renderer del objeto que contiene el texto o la pantalla")]
    [SerializeField] private Renderer screenRenderer; 

    [Header("Configuración de Balanceo (Colgado)")]
    [SerializeField] private float swaySpeed = 1.5f;
    [SerializeField] private float swayAngle = 3.0f;

    [Header("Configuración de Rotación (Mouse)")]
    [SerializeField] private Vector3 rotacionApagado = new Vector3(20f, 0f, 0f);
    [SerializeField] private Vector3 rotacionEncendido = Vector3.zero;
    [SerializeField] private float rotationSpeed = 5f;

    [Header("Configuración de Iluminación (Emissive)")]
    [ColorUsage(true, true)] [SerializeField] private Color colorApagado = new Color(0.2f, 0.2f, 0.2f, 1f); // Gris oscuro, sin brillo
    [ColorUsage(true, true)] [SerializeField] private Color colorEncendido = Color.cyan; // Color brillante
    [SerializeField] private float colorTransitionSpeed = 8f;

    private bool isHovered = false;
    private float randomOffset;
    private Material screenMaterial;

    void Start()
    {
        randomOffset = Random.Range(0f, 100f);
        
        // Configuramos el material al inicio
        if (screenRenderer != null)
        {
            // Al acceder a .material, Unity crea una instancia única para este monitor
            screenMaterial = screenRenderer.material;
            
            // Nos aseguramos de que la emisión esté habilitada en el shader
            screenMaterial.EnableKeyword("_EMISSION");
            screenMaterial.SetColor("_EmissionColor", colorApagado);
        }

        meshMonitor.localRotation = Quaternion.Euler(rotacionApagado);
    }

    void Update()
    {
        TargetSway();
        HandleInteraction();
        HandleEmission();
    }

    private void TargetSway()
    {
        float angleX = Mathf.Sin(Time.time * swaySpeed + randomOffset) * swayAngle;
        float angleZ = Mathf.Cos(Time.time * (swaySpeed * 0.8f) + randomOffset) * (swayAngle * 0.5f);
        
        swayPivot.localRotation = Quaternion.Euler(angleX, 0, angleZ);
    }

    private void HandleInteraction()
    {
        Vector3 targetRotation = isHovered ? rotacionEncendido : rotacionApagado;
        meshMonitor.localRotation = Quaternion.Slerp(
            meshMonitor.localRotation, 
            Quaternion.Euler(targetRotation), 
            Time.deltaTime * rotationSpeed
        );
    }

    // Nueva función para interpolar suavemente el brillo
    private void HandleEmission()
    {
        if (screenMaterial == null) return;

        Color targetColor = isHovered ? colorEncendido : colorApagado;
        Color currentColor = screenMaterial.GetColor("_EmissionColor");
        
        // Transición suave entre el estado apagado y encendido
        screenMaterial.SetColor("_EmissionColor", Color.Lerp(currentColor, targetColor, Time.deltaTime * colorTransitionSpeed));
    }

    private void OnMouseEnter()
    {
        isHovered = true;
        // Aquí podrías reproducir un sonido sutil de estática o "click" CRT
    }

    private void OnMouseExit()
    {
        isHovered = false;
    }
}