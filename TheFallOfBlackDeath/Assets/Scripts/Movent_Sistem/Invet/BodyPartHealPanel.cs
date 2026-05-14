using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Panel que lista las partes del cuerpo dañadas y permite seleccionar cuál curar.
/// </summary>
public class BodyPartHealPanel : MonoBehaviour
{
    public GameObject buttonPrefab;   // mismo prefab que usás en BodyPartPanel
    public Transform buttonContainer;

    private Action<BodyPart> _onPartSelected;
    private List<GameObject> _spawnedButtons = new List<GameObject>();

    public void Show(PlayerFighter target, float healAmount, Action<BodyPart> onPartSelected)
    {
        _onPartSelected = onPartSelected;

        // Limpiar botones anteriores
        foreach (var btn in _spawnedButtons)
            Destroy(btn);
        _spawnedButtons.Clear();

        // Solo mostrar partes que estén dañadas pero NO destruidas
        foreach (var partData in target.bodyParts)
        {
            if (partData == null) continue;
            if (partData.IsDestroyed) continue;
            if (partData.currentHealth >= partData.maxHealth) continue; // ya está full

            GameObject btnGO = Instantiate(buttonPrefab, buttonContainer);
            _spawnedButtons.Add(btnGO);

            // Label: nombre de la parte + HP actual/max
            var label = btnGO.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null)
                label.text = $"{partData.part}  " +
                             $"{partData.currentHealth}/{partData.maxHealth}" +
                             $"  (+{Mathf.Min(healAmount, partData.maxHealth - partData.currentHealth)})";

            // Click
            BodyPart capturedPart = partData.part;
            btnGO.GetComponent<Button>().onClick.AddListener(() => OnPartClick(capturedPart));
        }

        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private void OnPartClick(BodyPart part)
    {
        Hide();
        _onPartSelected?.Invoke(part);
    }
}
