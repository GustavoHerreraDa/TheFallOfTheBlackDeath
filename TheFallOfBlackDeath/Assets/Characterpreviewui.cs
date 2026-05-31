using UnityEngine;

/// <summary>
/// Componente OPCIONAL para paneles que necesiten reaccionar al cambio de personaje.
/// 
/// La cámara de preview NO se mueve — debe estar posicionada en el editor apuntando
/// al modelo correspondiente. Este componente solo sirve si el panel necesita
/// hacer algo adicional cuando cambia el personaje (ej: actualizar texto de nombre).
///
/// Si tu panel solo muestra el modelo 3D via Render Texture y nada más,
/// NO necesitás este componente — CharacterDisplayManager ya activa el modelo correcto.
/// </summary>
public class CharacterPreviewUI : MonoBehaviour
{
    [Header("Opcional: callback cuando cambia el personaje mostrado")]
    [SerializeField] private bool logCambiosEnConsola = false;

    private void OnEnable()
    {
        CharacterDisplayManager.OnDisplayedFighterChanged += OnFighterChanged;

        // Sincronizar con el estado actual al activarse el panel
        if (CharacterDisplayManager.Instance?.CurrentFighter != null)
            OnFighterChanged(CharacterDisplayManager.Instance.CurrentFighter);
    }

    private void OnDisable()
    {
        CharacterDisplayManager.OnDisplayedFighterChanged -= OnFighterChanged;
    }

    /// <summary>
    /// Sobreescribí este método en una subclase para reaccionar al cambio de personaje.
    /// Por ejemplo: actualizar un TMP_Text con el nombre del personaje.
    /// </summary>
    protected virtual void OnFighterChanged(PlayerFighter fighter)
    {
        if (logCambiosEnConsola)
            Debug.Log($"[CharacterPreviewUI] Personaje activo: {fighter?.idName ?? "ninguno"}");
    }
}