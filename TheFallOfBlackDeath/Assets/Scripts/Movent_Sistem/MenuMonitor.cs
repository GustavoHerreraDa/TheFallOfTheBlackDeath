using UnityEngine;
using UnityEngine.Events;

public class MenuMonitor : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform swayPivot;
    [SerializeField] private Transform meshMonitor;
    [SerializeField] private GameObject screenContent; // El texto/UI de la pantalla

    [Header("Configuración de Balanceo (Colgado)")]
    [SerializeField] private float swaySpeed = 1.5f;
    [SerializeField] private float swayAngle = 3.0f;

    [Header("Configuración de Rotación (Mouse)")]
    [SerializeField] private Vector3 rotacionApagado = new Vector3(20f, 0f, 0f); // Rotado sutilmente hacia abajo
    [SerializeField] private Vector3 rotacionEncendido = Vector3.zero; // Mirando de frente
    [SerializeField] private float rotationSpeed = 5f;

    private bool isHovered = false;
    private float randomOffset;

    void Start()
    {
        // Desfase aleatorio para que todos los monitores no se balanceen exactamente iguales
        randomOffset = Random.Range(0f, 100f);
        
        // Estado inicial
        if (screenContent != null) screenContent.SetActive(false);
        meshMonitor.localRotation = Quaternion.Euler(rotacionApagado);
    }

    void Update()
    {
        TargetSway();
        HandleInteraction();
    }

    // 1. Animación sutil de balanceo (Simula estar colgado)
    private void TargetSway()
    {
        float angleX = Mathf.Sin(Time.time * swaySpeed + randomOffset) * swayAngle;
        float angleZ = Mathf.Cos(Time.time * (swaySpeed * 0.8f) + randomOffset) * (swayAngle * 0.5f);
        
        swayPivot.localRotation = Quaternion.Euler(angleX, 0, angleZ);
    }

    // 2. Rotación suave hacia el frente o hacia abajo dependiendo del mouse
    private void HandleInteraction()
    {
        Vector3 targetRotation = isHovered ? rotacionEncendido : rotacionApagado;
        meshMonitor.localRotation = Quaternion.Slerp(
            meshMonitor.localRotation, 
            Quaternion.Euler(targetRotation), 
            Time.deltaTime * rotationSpeed
        );
    }

    // 3. Detección del Mouse (Requiere un Collider en este objeto)
    private void OnMouseEnter()
    {
        isHovered = true;
        if (screenContent != null) screenContent.SetActive(true);
        // Aquí podrías reproducir un sonido sutil de estática o "click" CRT
    }

    private void OnMouseExit()
    {
        isHovered = false;
        if (screenContent != null) screenContent.SetActive(false);
    }
}