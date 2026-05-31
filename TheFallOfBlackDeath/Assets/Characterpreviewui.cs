using UnityEngine;

/// <summary>
/// Componente reutilizable para paneles con cámara de preview.
/// Escucha a CharacterDisplayManager y reposiciona la cámara frente al modelo activo.
/// Agregar una instancia de este componente en cada panel que tenga su propia cámara de preview.
/// </summary>
public class CharacterPreviewUI : MonoBehaviour
{
    [Header("Cámara de este panel")]
    [SerializeField] private Camera previewCamera;

    [Header("Posición relativa al modelo")]
    [SerializeField] private Vector3 cameraOffset = new Vector3(0f, 1.5f, -2f);

    [Header("Punto al que mira la cámara (relativo al modelo)")]
    [SerializeField] private Vector3 lookAtOffset = new Vector3(0f, 1f, 0f);

    private void OnEnable()
    {
        CharacterDisplayManager.OnDisplayedFighterChanged += UpdateCamera;

        // Sincronizar inmediatamente con el personaje actual al abrir el panel
        if (CharacterDisplayManager.Instance != null)
        {
            UpdateCamera(CharacterDisplayManager.Instance.CurrentFighter);
        }
    }

    private void OnDisable()
    {
        CharacterDisplayManager.OnDisplayedFighterChanged -= UpdateCamera;
    }

    /// <summary>
    /// Reposiciona la cámara frente al modelo de preview del fighter recibido.
    /// </summary>
    private void UpdateCamera(PlayerFighter fighter)
    {
        if (previewCamera == null)
        {
            Debug.LogWarning("[CharacterPreviewUI] No hay cámara asignada.");
            return;
        }

        if (fighter == null) return;

        if (CharacterDisplayManager.Instance == null) return;

        var models = CharacterDisplayManager.Instance.PreviewModels;
        if (models == null || fighter.figherIndex < 0 || fighter.figherIndex >= models.Count) return;

        GameObject modelo = models[fighter.figherIndex];
        if (modelo == null) return;

        // Posicionar la cámara relativa al modelo
        previewCamera.transform.position = modelo.transform.position + cameraOffset;

        // La cámara mira al centro del modelo más el offset vertical
        previewCamera.transform.LookAt(modelo.transform.position + lookAtOffset);
    }
}