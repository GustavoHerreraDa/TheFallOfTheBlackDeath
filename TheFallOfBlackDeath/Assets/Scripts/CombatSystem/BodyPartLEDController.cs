using UnityEngine;

/// <summary>
/// Controla el color de un LED basado en la integridad de una parte del cuerpo específica de un Fighter.
/// Utiliza MaterialPropertyBlock para optimizar el rendimiento y evitar la creación de instancias de materiales.
/// </summary>
public class BodyPartLEDController : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField, Tooltip("Referencia al Fighter que posee la parte del cuerpo.")]
    private Fighter targetFighter;

    [SerializeField, Tooltip("La parte del cuerpo que este LED monitorea.")]
    private BodyPart monitoredPart;

    [SerializeField, Tooltip("El Renderer del LED que cambiará de color.")]
    private Renderer ledRenderer;

    [Header("Configuración de Colores (Emisión)")]
    [SerializeField, ColorUsage(true, true)]
    private Color fullHealthColor = Color.green;

    [SerializeField, ColorUsage(true, true)]
    private Color damagedColor = Color.yellow;

    [SerializeField, ColorUsage(true, true)]
    private Color criticalColor = Color.red;

    [SerializeField, ColorUsage(true, true)]
    private Color destroyedColor = Color.black;

    private MaterialPropertyBlock _propBlock;
    private Fighter.BodyPartData _bodyPartData;
    private static readonly int EmissionColorProperty = Shader.PropertyToID("_Fresnel_Color");

    private void Awake()
    {
        _propBlock = new MaterialPropertyBlock();
    }

    private void Start()
    {
        if (targetFighter != null)
        {
            Initialize(targetFighter, monitoredPart);
        }
    }

    /// <summary>
    /// Inicializa el controlador con un Fighter y una parte del cuerpo específica.
    /// </summary>
    /// <param name="fighter">El Fighter a monitorear.</param>
    /// <param name="part">La parte del cuerpo a monitorear.</param>
    public void Initialize(Fighter fighter, BodyPart part)
    {
        targetFighter = fighter;
        monitoredPart = part;
        
        if (targetFighter != null)
        {
            _bodyPartData = targetFighter.GetBodyPart(monitoredPart);
        }

        if (_bodyPartData == null)
        {
            Debug.LogWarning($"[BodyPartLEDController] No se encontró la parte {monitoredPart} en {targetFighter?.name}");
        }
        
        UpdateLEDColor();
    }

    private void Update()
    {
        UpdateLEDColor();
    }

    /// <summary>
    /// Actualiza el color del LED basado en el estado actual de salud de la parte del cuerpo.
    /// </summary>
    private void UpdateLEDColor()
    {
        if (ledRenderer == null || _bodyPartData == null) return;

        Color finalColor;

        if (_bodyPartData.IsDestroyed)
        {
            finalColor = destroyedColor;
        }
        else
        {
            float healthPercentage = _bodyPartData.currentHealth / _bodyPartData.maxHealth;
            
            // Interpolación de colores: Verde (1.0) -> Amarillo (0.5) -> Rojo (0.0)
            if (healthPercentage > 0.5f)
            {
                // De Verde a Amarillo
                float t = (healthPercentage - 0.5f) * 2f;
                finalColor = Color.Lerp(damagedColor, fullHealthColor, t);
            }
            else
            {
                // De Amarillo a Rojo
                float t = healthPercentage * 2f;
                finalColor = Color.Lerp(criticalColor, damagedColor, t);
            }
        }

        // Aplicar el color usando MaterialPropertyBlock
        ledRenderer.GetPropertyBlock(_propBlock);
        _propBlock.SetColor(EmissionColorProperty, finalColor);
        ledRenderer.SetPropertyBlock(_propBlock);
    }
}
