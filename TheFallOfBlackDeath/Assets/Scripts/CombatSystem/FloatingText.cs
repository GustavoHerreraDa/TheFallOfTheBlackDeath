using UnityEngine;
using TMPro;

public class FloatingText : MonoBehaviour
{
    public TextMeshProUGUI text;
    public float floatSpeed = 1f;
    public float fadeTime = 5000f;
    public bool lookAtCamera = true;

    private Color originalColor;
    private float timer;
    private Transform mainCamera;

    void Start()
    {
        mainCamera = Camera.main.transform;
        originalColor = text.color;
    }

    void Update()
    {
        // 🔸 Hacer que el texto siempre mire a la cámara
        if (lookAtCamera && mainCamera != null)
        {
            transform.rotation = Quaternion.LookRotation(transform.position - mainCamera.position);
        }

        // 🔸 Mover el texto hacia arriba con el tiempo
        transform.position += Vector3.up * floatSpeed * Time.deltaTime;

        // 🔸 Desvanecer progresivamente
        timer += Time.deltaTime;
        float alpha = Mathf.Lerp(originalColor.a, 0, timer / fadeTime);
        text.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);

        // 🔸 Destruir cuando termina la animación
        if (timer >= fadeTime)
            Destroy(gameObject);
    }

    public void SetText(string message, Color color)
    {
        text.text = message;
        text.color = color;
        originalColor = color;
    }
}
