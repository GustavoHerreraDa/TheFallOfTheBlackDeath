using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles the parry reaction QTE presentation, input window, and slow-motion timing.
/// It is intentionally unaware of combat, damage, or health resolution.
/// </summary>
public class QTEParryManager : MonoBehaviour
{
    public static QTEParryManager Instance { get; private set; }

    [Header("UI Elements")]
    [SerializeField] private GameObject qteContainer; // Contenedor principal para activar/desactivar todo el QTE
    [SerializeField] private Image ringImage;          // El anillo radial (Debe configurarse como Image Type: Filled)

    [Header("Input")]
    [SerializeField] private string parryButtonName = "Parry";

    [Header("Chromatic Zones Config")]
    [Tooltip("Duración en segundos (tiempo real) de la ventana perfecta inicial.")]
    [SerializeField] private float perfectWindowDuration = 0.12f;
    [SerializeField] private Color perfectColor = new Color(1f, 0.85f, 0f); // Dorado/Oro
    [Tooltip("Gradiente que define la transición de color una vez superada la ventana perfecta (ej. de Verde a Rojo).")]
    [SerializeField] private Gradient progressionGradient;

    private bool isParryActive;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // Aseguramos que el HUD del QTE comience oculto
        if (qteContainer != null)
            qteContainer.SetActive(false);
            
        // Inicialización de un gradiente por defecto (Verde -> Rojo) si no se asigna en el Inspector
        if (progressionGradient == null)
        {
            progressionGradient = new Gradient();
            var colorKeys = new GradientColorKey[2];
            colorKeys[0] = new GradientColorKey(Color.green, 0f);
            colorKeys[1] = new GradientColorKey(Color.red, 1f);
            var alphaKeys = new GradientAlphaKey[2];
            alphaKeys[0] = new GradientAlphaKey(1f, 0f);
            alphaKeys[1] = new GradientAlphaKey(1f, 1f);
            progressionGradient.SetKeys(colorKeys, alphaKeys);
        }
    }

    /// <summary>
    /// Opens a real-time parry window while the game runs in slow-motion.
    /// </summary>
    /// <param name=\"windowDuration\">Reaction window duration in unscaled seconds.</param>
    /// <param name=\"slowMoTimeScale\">Time scale used while the QTE is active.</param>
    /// <param name=\"onResult\">Callback invoked with true when the player parries in time.</param>
    public IEnumerator WaitForParry(float windowDuration, float slowMoTimeScale, Action<bool> onResult)
    {
        if (isParryActive)
        {
            Debug.LogWarning("A parry QTE is already active. The new request will fail safely.");
            onResult?.Invoke(false);
            yield break;
        }

        isParryActive = true;
        bool parried = false;
        float elapsed = 0f;

        // Activamos el feedback visual
        if (qteContainer != null)
            qteContainer.SetActive(true);

        if (ringImage != null)
            ringImage.fillAmount = 1f;

        // Aplicamos la escala de tiempo para el slow-motion
        Time.timeScale = Mathf.Clamp(slowMoTimeScale, 0.01f, 1f);

        while (elapsed < windowDuration)
        {
            // Registrar input del jugador
            if (Input.GetButtonDown(parryButtonName))
            {
                parried = true;
                break;
            }

            elapsed += Time.unscaledDeltaTime;

            // Actualizar el comportamiento visual del anillo radial
            if (ringImage != null)
            {
                // 1. El anillo se vacía progresivamente en tiempo real (de 1.0 a 0.0)
                float progressNormalized = Mathf.Clamp01(elapsed / windowDuration);
                ringImage.fillAmount = 1f - progressNormalized;

                // 2. Control cromático por zonas (Dorado inicial -> Gradiente Verde a Rojo)
                if (elapsed <= perfectWindowDuration)
                {
                    ringImage.color = perfectColor;
                }
                else
                {
                    // Calculamos el progreso remanente exclusivamente para la zona del gradiente
                    float remainingDuration = windowDuration - perfectWindowDuration;
                    float gradientTime = remainingDuration > 0f 
                        ? (elapsed - perfectWindowDuration) / remainingDuration 
                        : 1f;

                    ringImage.color = progressionGradient.Evaluate(Mathf.Clamp01(gradientTime));
                }
            }

            yield return null;
        }

        // Restauramos el flujo normal del tiempo del juego
        Time.timeScale = 1f;

        // Ocultamos el contenedor visual
        if (qteContainer != null)
            qteContainer.SetActive(false);

        isParryActive = false;
        onResult?.Invoke(parried);
    }
}