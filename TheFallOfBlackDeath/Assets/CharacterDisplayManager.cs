using UnityEngine;
using System;
using System.Collections.Generic;
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

    [Header("Modelos de preview — índice coincide con figherIndex")]
    [SerializeField] private List<GameObject> previewModels = new List<GameObject>();

    /// <summary>
    /// Personaje actualmente mostrado.
    /// </summary>
    public PlayerFighter CurrentFighter { get; private set; }

    /// <summary>
    /// Lista de modelos de preview accesible para CharacterPreviewUI.
    /// </summary>
    public List<GameObject> PreviewModels => previewModels;

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
        foreach (var model in previewModels)
        {
            if (model != null) model.SetActive(false);
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
    /// Activa el modelo correspondiente al fighter recibido y notifica a los suscriptores.
    /// </summary>
    private void SetDisplayedFighter(PlayerFighter fighter)
    {
        if (fighter == null) return;

        // Desactivar todos los modelos
        foreach (var model in previewModels)
        {
            if (model != null) model.SetActive(false);
        }

        // Activar el modelo que corresponde al figherIndex
        if (fighter.figherIndex >= 0 && fighter.figherIndex < previewModels.Count)
        {
            var targetModel = previewModels[fighter.figherIndex];
            if (targetModel != null) targetModel.SetActive(true);
        }
        else
        {
            Debug.LogWarning($"[CharacterDisplayManager] figherIndex {fighter.figherIndex} fuera de rango. " +
                             $"Modelos disponibles: {previewModels.Count}");
        }

        CurrentFighter = fighter;
        OnDisplayedFighterChanged?.Invoke(fighter);
    }
}