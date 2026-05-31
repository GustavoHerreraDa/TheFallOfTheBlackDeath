using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using InventoryNew;

/// <summary>
/// Singleton que gestiona qué modelo de preview se muestra según el personaje seleccionado.
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

    [System.Serializable]
    public struct FighterModelEntry
    {
       public int fighterIndex;    // el figherIndex del PlayerFighter
       public GameObject model;    // el modelo 3D correspondiente
    }

    [Header("Modelos de preview — mapeados por figherIndex")]
    [SerializeField] private List<FighterModelEntry> previewModelEntries = new List<FighterModelEntry>();

    /// <summary>
    /// Personaje actualmente mostrado.
    /// </summary>
    public PlayerFighter CurrentFighter { get; private set; }

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
        // Desactivar todos los modelos al inicio
        foreach (var entry in previewModelEntries)
        {
            if (entry.model != null) entry.model.SetActive(false);
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
    /// Busca el modelo correspondiente a un fighterIndex.
    /// </summary>
    public GameObject GetModel(int fighterIndex)
    {
        var entry = previewModelEntries.FirstOrDefault(e => e.fighterIndex == fighterIndex);
        return entry.model;
    }

    /// <summary>
    /// Activa el modelo correspondiente al fighter recibido y notifica a los suscriptores.
    /// </summary>
    private void SetDisplayedFighter(PlayerFighter fighter)
    {
        if (fighter == null) return;

        // Desactivar todos los modelos
        foreach (var entry in previewModelEntries)
        {
            if (entry.model != null) entry.model.SetActive(false);
        }

        // Activar el modelo que corresponde al figherIndex
        GameObject targetModel = GetModel(fighter.figherIndex);
        
        if (targetModel != null)
        {
            targetModel.SetActive(true);
        }
        else
        {
            Debug.LogWarning($"[CharacterDisplayManager] No se encontró modelo para figherIndex {fighter.figherIndex}. " +
                             $"Entradas configuradas: {previewModelEntries.Count}");
        }

        CurrentFighter = fighter;
        OnDisplayedFighterChanged?.Invoke(fighter);
    }
}