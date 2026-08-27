using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Representa los posibles resultados de la evaluación del QTE de Parry.
/// - Miss: Entrada fuera de tiempo o sin respuesta (100% de daño recibido).
/// - Guard: Bloqueo parcial en la ventana de guardia (daño reducido, ej. 50%).
/// - Parry: Bloqueo perfecto en la ventana dorada (0% de daño + contraataque inmediato).
/// </summary>
public enum QTEParryResult
{
    Miss,
    Guard,
    Parry
}

/// <summary>
/// Administrador del QTE de Parry y Guardia en tiempo real durante combates por turnos.
/// Gestiona la presentación visual (anillo radial cromático), ralentización del tiempo (slow-motion),
/// detección de entrada precisa y emisión del resultado (Miss, Guard, Parry).
/// Desacoplado de la lógica de combate directa, comunicándose mediante callbacks y eventos.
/// </summary>
public class QTEParryManager : MonoBehaviour
{
    public static QTEParryManager Instance { get; private set; }

    [Header("UI Elements")]
    [Tooltip("Contenedor principal para activar/desactivar todo el HUD del QTE.")]
    [SerializeField] private GameObject qteContainer;

    [Tooltip("Imagen del anillo radial (Debe configurarse como Image Type: Filled).")]
    [SerializeField] private Image ringImage;

    [Header("Input Configuration")]
    [Tooltip("Permite presionar el botón principal del mouse (Click Izquierdo) para activar la defensa.")]
    [SerializeField] private bool allowMouseInput = true;

    [Tooltip("Tecla de teclado alternativa para activar el Parry/Guardia.")]
    [SerializeField] private KeyCode parryKey = KeyCode.Space;

    [Header("Timing Windows")]
    [Tooltip("Duración en segundos (tiempo real) de la ventana de Parry Perfecto inicial.")]
    [SerializeField] private float perfectWindowDuration = 0.12f;

    [Header("Damage Multipliers")]
    [Tooltip("Multiplicador de daño recibido durante una Guardia exitosa (0.5 = 50% de daño).")]
    [Range(0f, 1f)]
    [SerializeField] private float guardDamageMultiplier = 0.5f;

    [Header("Chromatic Zones Config")]
    [Tooltip("Color del anillo durante la ventana de Parry Perfecto.")]
    [SerializeField] private Color perfectColor = new Color(1f, 0.85f, 0f); // Dorado/Oro

    [Tooltip("Gradiente que define la transición de color una vez superada la ventana perfecta (ej. Verde a Rojo).")]
    [SerializeField] private Gradient progressionGradient;

    [Header("Juiciness & Feedback (Camera Effects)")]
    [Tooltip("Duración del congelamiento de fotograma (HitStop) en segundos tras un Parry exitoso.")]
    [SerializeField] private float parryHitStopDuration = 0.1f;

    [Tooltip("Fuerza de la sacudida de cámara (Camera Shake) al concretar un Parry exitoso.")]
    [SerializeField] private float parryShakeForce = 1.4f;

    [Tooltip("Fuerza de sacudida leve para Guard.")]
    [SerializeField] private float guardShakeForce = 0.5f;

    // Eventos públicos para observadores externos
    public event Action OnQTEStarted;
    public event Action<QTEParryResult> OnParryEvaluated;

    private bool isParryActive;

    public float GuardDamageMultiplier => guardDamageMultiplier;
    public float PerfectWindowDuration => perfectWindowDuration;

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

        // Inicialización de un gradiente por defecto (Verde -> Rojo) si no se asignó en el Inspector
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
    /// Inicia la ventana de reacción de Parry/Guardia con soporte para los 3 estados: Miss, Guard y Parry.
    /// </summary>
    /// <param name="windowDuration">Duración total de la ventana de reacción en segundos de tiempo real (sin escalar).</param>
    /// <param name="slowMoTimeScale">Escala de tiempo utilizada durante el QTE (ej. 0.2f para slow-motion).</param>
    /// <param name="onResult">Callback invocado con el resultado obtenido (Miss, Guard o Parry).</param>
    public IEnumerator WaitForParry(float windowDuration, float slowMoTimeScale, Action<QTEParryResult> onResult)
    {
        if (isParryActive)
        {
            Debug.LogWarning("[QTEParryManager] Ya hay un QTE de Parry activo. La nueva petición fallará de forma segura devolviendo Miss.");
            onResult?.Invoke(QTEParryResult.Miss);
            yield break;
        }

        isParryActive = true;
        QTEParryResult result = QTEParryResult.Miss;
        float elapsed = 0f;

        OnQTEStarted?.Invoke();

        // Activamos feedback visual inicial
        if (qteContainer != null)
            qteContainer.SetActive(true);

        if (ringImage != null)
        {
            ringImage.fillAmount = 1f;
            ringImage.color = perfectColor;
        }

        // Aplicamos la escala de tiempo para slow-motion
        Time.timeScale = Mathf.Clamp(slowMoTimeScale, 0.01f, 1f);

        while (elapsed < windowDuration)
        {
            // Verificamos si el jugador presionó el botón configurado
            bool inputPressed = (allowMouseInput && Input.GetMouseButtonDown(0)) || Input.GetKeyDown(parryKey);

            if (inputPressed)
            {
                // Si la pulsación ocurrió dentro del umbral de ventana perfecta -> PARRY
                if (elapsed <= perfectWindowDuration)
                {
                    result = QTEParryResult.Parry;
                }
                else // Pulsación tardía pero antes de que expire la ventana -> GUARD
                {
                    result = QTEParryResult.Guard;
                }
                break;
            }

            elapsed += Time.unscaledDeltaTime;

            // Actualización visual del anillo radial
            if (ringImage != null)
            {
                // El anillo se vacía progresivamente en tiempo real
                float progressNormalized = Mathf.Clamp01(elapsed / windowDuration);
                ringImage.fillAmount = 1f - progressNormalized;

                // Control cromático según la fase de tiempo
                if (elapsed <= perfectWindowDuration)
                {
                    ringImage.color = perfectColor;
                }
                else
                {
                    float remainingDuration = windowDuration - perfectWindowDuration;
                    float gradientTime = remainingDuration > 0f
                        ? (elapsed - perfectWindowDuration) / remainingDuration
                        : 1f;

                    ringImage.color = progressionGradient.Evaluate(Mathf.Clamp01(gradientTime));
                }
            }

            yield return null;
        }

        // Restauramos el flujo normal del tiempo
        Time.timeScale = 1f;

        // Ocultamos el contenedor visual
        if (qteContainer != null)
            qteContainer.SetActive(false);

        isParryActive = false;

        // Jugosidad / Efectos de cámara según el resultado
        TriggerJuiceEffects(result);

        OnParryEvaluated?.Invoke(result);
        onResult?.Invoke(result);
    }

    /// <summary>
    /// Sobrecarga para compatibilidad hacia atrás con sistemas que esperaban un callback booleano (true solo en Parry).
    /// </summary>
    public IEnumerator WaitForParry(float windowDuration, float slowMoTimeScale, Action<bool> onResult)
    {
        yield return WaitForParry(windowDuration, slowMoTimeScale, result =>
        {
            onResult?.Invoke(result == QTEParryResult.Parry);
        });
    }

    /// <summary>
    /// Aplica efectos visuales e impacto táctil desacoplados a través de CameraManager / CameraFXManager.
    /// </summary>
    private void TriggerJuiceEffects(QTEParryResult result)
    {
        if (CameraManager.Instance == null) return;

        switch (result)
        {
            case QTEParryResult.Parry:
                CameraManager.Instance.TriggerHitStop(parryHitStopDuration);
                CameraManager.Instance.TriggerShake(parryShakeForce);
                break;

            case QTEParryResult.Guard:
                CameraManager.Instance.TriggerShake(guardShakeForce);
                break;

            case QTEParryResult.Miss:
                break;
        }
    }
}