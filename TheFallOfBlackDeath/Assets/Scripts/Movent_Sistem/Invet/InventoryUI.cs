using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class InventoryUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Datos del item (seteado por InventoryManager)")]
    public int itemId = -1; // InventoryManager lo setea al popular el slot

    [Header("UI")]
    public TMP_Text amount;
    public TMP_Text itemName;
    public TMP_Text itemDescripcion;
    public Image sprite;
    public Image buttonSprite;

    [Header("Stats (seteado por InventoryManager)")]
    public string statAffected;
    public float amountAffected;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip equipSfx;
    public AudioClip unequipSfx;

    [Header("Colores")]
    public Color equippedColor   = Color.green;
    public Color unequippedColor = Color.white;

    private Color originalColor;
    public BodyPartHealItem healItem;
    // ─────────────────────────────────────────────────────────────────────────
    // Unity lifecycle
    // ─────────────────────────────────────────────────────────────────────────

    private void Awake()
    {
        // QUITAMOS DontDestroyOnLoad — el pool lo maneja InventoryManager
        originalColor = buttonSprite != null ? buttonSprite.color : Color.white;
    }

    private void OnEnable()
    {
        InventoryManager.OnCharacterChanged += OnCharacterChanged;
        InventoryManager.OnInventoryChanged += RefreshEquippedVisual;
    }

    private void OnDisable()
    {
        InventoryManager.OnCharacterChanged -= OnCharacterChanged;
        InventoryManager.OnInventoryChanged -= RefreshEquippedVisual;
    }

    private void OnCharacterChanged(PlayerFighter fighter)
    {
        // Cuando cambia el personaje activo, refrescar el visual de este slot
        RefreshEquippedVisual();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Botones de equipamiento
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Equipa o desequipa el item del personaje actualmente seleccionado en el InventoryManager.
    /// </summary>
    public void EquipToActiveCharacterBTN()
    {
        if (InventoryManager.instance == null) return;
        HandleEquipToggle(InventoryManager.instance.activeCharacterIndex);
    }

    /// <summary>
    /// Equipa o desequipa el item del personaje 1 (índice 0).
    /// </summary>
    public void Character1BTN()
    {
        HandleEquipToggle(0);
    }

    /// <summary>
    /// Equipa o desequipa el item del personaje 2 (índice 1).
    /// </summary>
    public void Character2BTN()
    {
        HandleEquipToggle(1);
    }

    private void HandleEquipToggle(int characterIndex)
    {
        if (itemId < 0 || InventoryManager.instance == null) return;

        if (InventoryManager.instance.IsEquippedByCharacter(itemId, characterIndex))
        {
            // Ya está equipado → desequipar
            InventoryManager.instance.Unequip(itemId, characterIndex);
            audioSource?.PlayOneShot(unequipSfx);
        }
        else
        {
            // No está equipado → equipar (Equip() maneja el desequipado del otro personaje)
            InventoryManager.instance.Equip(itemId, characterIndex);
            audioSource?.PlayOneShot(equipSfx);
        }

        RefreshEquippedVisual();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Visual
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Actualiza el color del botón consultando el estado persistido en InventoryManager.
    /// Llamado por InventoryManager al popular el slot y por los eventos de cambio.
    /// </summary>
    public void RefreshEquippedVisual()
    {
        if (buttonSprite == null || InventoryManager.instance == null) return;

        bool equippedByAny = InventoryManager.instance.IsEquippedByCharacter(itemId, 0) || 
                             InventoryManager.instance.IsEquippedByCharacter(itemId, 1);
        buttonSprite.color = equippedByAny ? equippedColor : originalColor;
    }
    
    public void UseBodyPartHealBTN()
    {
        if (healItem == null || InventoryManager.instance == null) return;

        // Obtener el fighter activo
        PlayerFighter target = GameManager.Instance?.character1;
        if (target == null) return;

        healItem.healAmount = amountAffected; // viene del InventoryManager al popular el slot
        healItem.Use(target, itemId);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (itemId < 0) return;
        CharacterSwitcher.NotifyStatsPreview(true, itemId);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        CharacterSwitcher.NotifyStatsPreview(false, -1);
    }
}