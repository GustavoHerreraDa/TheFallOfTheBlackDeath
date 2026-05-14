using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Displays the post-combat loot panel.
/// Wire this to a Canvas Panel that is hidden by default (SetActive false).
///
/// Hierarchy suggestion:
///   LootPanel (this script)
///     ├── Title (TMP_Text)           → "Victory! You looted:"
///     ├── ItemContainer (Transform)  → vertical layout group, child items are spawned here
///     ├── NoLootText (TMP_Text)      → shown when nothing dropped
///     └── ContinueButton (Button)    → any input / space bar also works
/// </summary>
public class LootPanel : MonoBehaviour
{
    [Header("References")]
    public GameObject itemRowPrefab;        // A prefab with Image + TMP_Text (name) + TMP_Text (amount)
    public Transform itemContainer;         // Parent for spawned rows
    public TMP_Text titleText;
    public TMP_Text noLootText;
    public Button continueButton;

    [Header("Settings")]
    public string titleWhenLoot    = "Victory! You looted:";
    public string titleWhenNoLoot  = "Victory! Nothing to loot this time.";
    public KeyCode continueKey     = KeyCode.Space;

    // Called by CombatManager
    public System.Action OnContinue;

    private bool _waitingForInput = false;

    // -------------------------------------------------------------------------

    /// <summary>
    /// Populate the panel with resolved loot entries and show it.
    /// </summary>
    public void Show(List<BodyPartLootTable.LootEntry> lootEntries)
    {
        // Clear previous rows
        foreach (Transform child in itemContainer)
            Destroy(child.gameObject);

        bool hasLoot = lootEntries != null && lootEntries.Count > 0;

        if (titleText  != null) titleText.text  = hasLoot ? titleWhenLoot : titleWhenNoLoot;
        if (noLootText != null) noLootText.gameObject.SetActive(!hasLoot);

        if (hasLoot)
        {
            foreach (var entry in lootEntries)
                SpawnRow(entry);
        }

        gameObject.SetActive(true);
        _waitingForInput = true;

        if (continueButton != null)
        {
            continueButton.onClick.RemoveAllListeners();
            continueButton.onClick.AddListener(Continue);
        }
    }

    private void Update()
    {
        if (_waitingForInput && Input.GetKeyDown(continueKey))
            Continue();
    }

    private void Continue()
    {
        if (!_waitingForInput) return;
        _waitingForInput = false;
        gameObject.SetActive(false);
        OnContinue?.Invoke();
    }

    // -------------------------------------------------------------------------

    private void SpawnRow(BodyPartLootTable.LootEntry entry)
    {
        if (itemRowPrefab == null || itemContainer == null) return;

        GameObject row = Instantiate(itemRowPrefab, itemContainer);

        // Determine icon and name based on new or legacy data
        Sprite spriteToDisplay = entry.itemSprite;
        string nameToDisplay = entry.itemDisplayName;

        if (entry.newItemData != null)
        {
            if (entry.newItemData.icon != null) spriteToDisplay = entry.newItemData.icon;
            nameToDisplay = entry.newItemData.itemName;
        }

        // Try to fill icon
        Image icon = row.transform.Find("Icon")?.GetComponent<Image>();
        if (icon != null && spriteToDisplay != null)
            icon.sprite = spriteToDisplay;

        // Item name
        TMP_Text nameText = row.transform.Find("ItemName")?.GetComponent<TMP_Text>();
        if (nameText != null)
            nameText.text = nameToDisplay;

        // Amount
        TMP_Text amountText = row.transform.Find("Amount")?.GetComponent<TMP_Text>();
        if (amountText != null)
            amountText.text = $"x{entry.amount}";
    }
}
