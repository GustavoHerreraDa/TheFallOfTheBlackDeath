using UnityEngine;
using TMPro;

public class FloatingText : MonoBehaviour
{
    public TextMeshProUGUI textMesh;
    
    [Header("Movimiento")]
    public float normalSpeed = 1f;
    public float criticalSpeed = 2f; // Los críticos suben más rápido
    public Vector3 randomOffset = new Vector3(0.5f, 0, 0);
    
    [Header("Escala")]
    public float normalSize = 1f;
    public float criticalSize = 1.5f; // Los críticos son más grandes

    private float timer;
    private float activeDuration = 1.5f; // Cuánto dura antes de volver al pool
    private Vector3 startPos;
    private Transform mainCamera;

    void Awake()
    {
        mainCamera = Camera.main.transform;
    }

    // Reemplazamos SetText por Initialize para configurar todo de una vez
    public void Initialize(string message, Color color, bool isCritical)
    {
        textMesh.text = message;
        textMesh.color = color;
        timer = 0;

        // 1. Variación de Tamaño
        transform.localScale = Vector3.one * (isCritical ? criticalSize : normalSize);

        // 2. Variación de Posición (Random offset para que no se encimen)
        startPos = transform.position + new Vector3(
            Random.Range(-randomOffset.x, randomOffset.x),
            Random.Range(-randomOffset.y, randomOffset.y), 
            0
        );
        transform.position = startPos;
    }

    void Update()
    {
        // Billboard (Mirar a cámara)
        if (mainCamera != null)
            transform.rotation = Quaternion.LookRotation(transform.position - mainCamera.position);

        timer += Time.deltaTime;
        float percent = timer / activeDuration;

        // Movimiento simple hacia arriba
        transform.position = startPos + Vector3.up * (normalSpeed * percent);

        // Fade Out (Desvanecerse al final)
        if (percent > 0.5f) // Empieza a desvanecerse a la mitad de su vida
        {
            float alpha = Mathf.Lerp(1, 0, (percent - 0.5f) * 2);
            textMesh.color = new Color(textMesh.color.r, textMesh.color.g, textMesh.color.b, alpha);
        }

        // RETORNO AUTOMÁTICO AL MANAGER
        if (timer >= activeDuration)
        {
            FloatingTextManager.Instance.ReturnToPool(this);
        }
    }
}