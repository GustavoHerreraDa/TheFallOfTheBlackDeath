using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using InventoryNew;

/// <summary>
/// Singleton que gestiona qué modelo de preview se muestra según el personaje seleccionado.
/// Instancia dinámicamente los modelos 3D en un spawn point específico.
/// Escucha al PartyMemberSelectorUI y notifica a todos los paneles interesados.
/// </summary>
public class CharacterDisplayManager : MonoBehaviour
{
    public static CharacterDisplayManager Instance { get; private set; }

    /// <summary>
    /// Evento disparado cuando cambia el personaje mostrado. Los paneles se suscriben a esto.
    /// </summary>
    public static event Action<PlayerFighter> OnDisplayedFighterChanged;

    [Header("Referencia al selector de party")]
    [SerializeField] private PartyMemberSelectorUI memberSelector;

    [Header("Spawn Point para el modelo de preview")]
    [SerializeField] private Transform previewSpawnPoint;

    [System.Serializable]
    public struct FighterModelEntry
    {
        public int fighterIndex;      // el fighterIndex del PlayerFighter
        public GameObject modelPrefab; // el prefab 3D correspondiente
    }

    [Header("Modelos de preview — mapeados por fighterIndex")]
    [SerializeField] private List<FighterModelEntry> previewModelEntries = new List<FighterModelEntry>();

    /// <summary>
    /// Personaje actualmente mostrado.
    /// </summary>
    public PlayerFighter CurrentFighter { get; private set; }

    /// <summary>
    /// Instancia del modelo actualmente visible.
    /// </summary>
    private GameObject currentModelInstance;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        // Validar que el spawn point está configurado
        if (previewSpawnPoint == null)
        {
            Debug.LogError("[CharacterDisplayManager] No hay spawn point configurado para el preview del modelo.");
            return;
        }

        // Sincronizar con el personaje ya seleccionado si existe
        if (memberSelector != null && memberSelector.CurrentSelected != null)
        {
            SetDisplayedFighter(memberSelector.CurrentSelected);
        }
    }

    private void OnEnable()
    {
        if (memberSelector != null)
            memberSelector.OnMemberSelected += SetDisplayedFighter;
    }

    private void OnDisable()
    {
        if (memberSelector != null)
            memberSelector.OnMemberSelected -= SetDisplayedFighter;
    }

    /// <summary>
    /// Busca el prefab correspondiente a un fighterIndex.
    /// </summary>
    public GameObject GetModelPrefab(int fighterIndex)
    {
        var entry = previewModelEntries.FirstOrDefault(e => e.fighterIndex == fighterIndex);
        return entry.modelPrefab;
    }

    /// <summary>
    /// Instancia el modelo correspondiente al fighter y notifica a los suscriptores.
    /// </summary>
    private void SetDisplayedFighter(PlayerFighter fighter)
    {
        if (fighter == null) return;

        if (previewSpawnPoint == null)
        {
            Debug.LogError("[CharacterDisplayManager] No hay spawn point configurado.");
            return;
        }

        // Destruir el modelo anterior
        if (currentModelInstance != null)
        {
            Destroy(currentModelInstance);
            currentModelInstance = null;
        }

        // Obtener el prefab del fighter
        GameObject modelPrefab = GetModelPrefab(fighter.figherIndex);

        if (modelPrefab != null)
        {
            // Instanciar el modelo en el spawn point
            currentModelInstance = Instantiate(
                modelPrefab,
                previewSpawnPoint.position,
                previewSpawnPoint.rotation,
                previewSpawnPoint
            );

            Debug.Log($"[CharacterDisplayManager] Modelo instanciado para {fighter.idName}");
        }
        else
        {
            Debug.LogWarning($"[CharacterDisplayManager] No se encontró prefab para fighterIndex {fighter.figherIndex}. " +
                             $"Entradas configuradas: {previewModelEntries.Count}");
        }

        CurrentFighter = fighter;
        OnDisplayedFighterChanged?.Invoke(fighter);
    }

    /// <summary>
    /// Limpia el modelo cuando se destruye el manager.
    /// </summary>
    private void OnDestroy()
    {
        if (currentModelInstance != null)
        {
            Destroy(currentModelInstance);
        }
    }
}
