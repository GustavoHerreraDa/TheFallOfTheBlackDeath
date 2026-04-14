using UnityEngine;
using UnityEngine.Rendering; // Necesario para el Volume
using UnityEngine.Rendering.Universal; // Necesario si usas URP (si no usas URP, avÃ­same)

/// <summary>
/// Supports the combat system by handling bloom manager.
/// </summary>
public class BloomManager: MonoBehaviour
{
    public static BloomManager Instance;

    [Header("Settings")]
    public Volume globalVolume; // Arrastra tu Global Volume aquÃ­
    public Color combatTint = Color.green; // El color verde que quieres
    public float transitionSpeed = 5f;

    private Bloom _bloom;
    private Color _originalColor;
    private bool _isHoveringEnemy;

    /// <summary>
    /// Initializes cached references and runtime state before the component starts running.
    /// </summary>
    private void Awake()
    {
        // Singleton bÃ¡sico
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // Intentamos obtener el componente Bloom del perfil del volumen
        if (globalVolume.profile.TryGet(out _bloom))
        {
            _originalColor = _bloom.tint.value;
        }
    }

    /// <summary>
    /// Updates the component each frame while it is active.
    /// </summary>
    private void Update()
    {
        if (_bloom == null) return;

        // Interpolamos el color suavemente (Lerp) para que no sea un cambio brusco
        Color targetColor = _isHoveringEnemy ? combatTint : _originalColor;
        
        // Aplicamos el cambio al valor del Tint
        _bloom.tint.value = Color.Lerp(_bloom.tint.value, targetColor, Time.deltaTime * transitionSpeed);
    }

    /// <summary>
    /// Sets the enemy highlight.
    /// </summary>
    /// <param name="active">The active.</param>
    public void SetEnemyHighlight(bool active)
    {
        _isHoveringEnemy = active;
    }
}
