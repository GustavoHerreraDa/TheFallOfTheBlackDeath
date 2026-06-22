using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Componente de solo lectura para modelos de preview en la UI del inventario.
/// Sincroniza la visibilidad de las partes del cuerpo y prótesis basándose en el estado del PlayerFighter seleccionado.
/// </summary>
public class PreviewModelVisualSync : MonoBehaviour
{
    private PlayerFighter _currentFighter;
    private Renderer[] _allRenderers;
    private const string PROSTHETIC_PREFIX = "Prosthetic_";

    private void Awake()
    {
        // Cacheamos todos los renderers del modelo de preview al inicio
        _allRenderers = GetComponentsInChildren<Renderer>(true);
    }

    private void OnEnable()
    {
        // Suscribirse al cambio de personaje mostrado en el CharacterDisplayManager
        CharacterDisplayManager.OnDisplayedFighterChanged += OnFighterChanged;

        // Si ya hay un fighter seleccionado al activarse, sincronizar inmediatamente
        if (CharacterDisplayManager.Instance != null && CharacterDisplayManager.Instance.CurrentFighter != null)
        {
            OnFighterChanged(CharacterDisplayManager.Instance.CurrentFighter);
        }
        else
        {
            // Intentar forzar una sincronización con el fighter actual del manager por si el evento ya pasó
            var manager = Object.FindFirstObjectByType<CharacterDisplayManager>();
            if (manager != null && manager.CurrentFighter != null)
            {
                OnFighterChanged(manager.CurrentFighter);
            }
        }
    }

    private void OnDisable()
    {
        // Desuscripción global
        CharacterDisplayManager.OnDisplayedFighterChanged -= OnFighterChanged;
        
        // Desuscripción del fighter específico
        UnsubscribeFromFighter();
    }

    private void OnFighterChanged(PlayerFighter newFighter)
    {
        // Evitar memory leaks desuscribiéndose del anterior
        UnsubscribeFromFighter();

        _currentFighter = newFighter;

        if (_currentFighter != null)
        {
            // Suscribirse a eventos de daño del nuevo fighter
            _currentFighter.OnBodyPartDestroyedEvent += OnBodyPartDestroyed;
            
            // Sincronización inicial
            SyncVisuals();
        }
    }

    private void UnsubscribeFromFighter()
    {
        if (_currentFighter != null)
        {
            _currentFighter.OnBodyPartDestroyedEvent -= OnBodyPartDestroyed;
            _currentFighter = null;
        }
    }

    private void OnBodyPartDestroyed(BodyPart part)
    {
        // Cuando una parte se destruye, refrescamos la visualización
        SyncVisuals();
    }

    /// <summary>
    /// Itera sobre las partes del cuerpo del fighter y ajusta la visibilidad de los renderers del modelo.
    /// </summary>
    public void SyncVisuals()
    {
        if (_currentFighter == null || _allRenderers == null) return;

        Debug.Log($"[PreviewModelVisualSync] Sincronizando visuales para {_currentFighter.idName}. Renderers cacheados: {_allRenderers.Length}");

        foreach (var partData in _currentFighter.bodyParts)
        {
            if (partData.part == BodyPart.None) continue;

            string partName = partData.part.ToString();
            bool isDestroyed = partData.IsDestroyed; // Usamos la propiedad IsDestroyed del Fighter
            bool hasProsthetic = partData.HasActiveProsthetic;

            // Mapeo especial para piernas por si acaso la nomenclatura no coincide exactamente con el Enum
            bool isLeg = partData.part == BodyPart.LeftLeg || partData.part == BodyPart.RightLeg;

            Debug.Log($"[PreviewModelVisualSync] Parte: {partName}, Destruida: {isDestroyed}, Salud: {partData.currentHealth}, Prótesis: {hasProsthetic}");

            foreach (var renderer in _allRenderers)
            {
                if (renderer == null) continue;

                string rendererName = renderer.name;

                // Lógica para el objeto de la prótesis (nombre exacto "Prosthetic_" + nombre de la parte)
                if (rendererName.Equals(PROSTHETIC_PREFIX + partName, System.StringComparison.OrdinalIgnoreCase))
                {
                    bool shouldBeActive = isDestroyed && hasProsthetic;
                    renderer.gameObject.SetActive(shouldBeActive);
                    Debug.Log($"[PreviewModelVisualSync] Prótesis {rendererName} -> Activa: {shouldBeActive}");
                }
                // Lógica para renderers orgánicos (contiene el nombre de la parte y NO empieza con "Prosthetic_")
                else if ((rendererName.IndexOf(partName, System.StringComparison.OrdinalIgnoreCase) >= 0 || 
                         (isLeg && rendererName.IndexOf("Leg", System.StringComparison.OrdinalIgnoreCase) >= 0 && rendererName.IndexOf(partName.Replace("Leg", ""), System.StringComparison.OrdinalIgnoreCase) >= 0)) && 
                         !rendererName.StartsWith(PROSTHETIC_PREFIX, System.StringComparison.OrdinalIgnoreCase))
                {
                    bool shouldBeActive = !isDestroyed;
                    renderer.gameObject.SetActive(shouldBeActive);
                    Debug.Log($"[PreviewModelVisualSync] Orgánico {rendererName} -> Activo: {shouldBeActive}");
                }
            }
        }
    }
}
