using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Helper script to link a UI Button to the CombatScannerSystem without losing references.
/// </summary>
[RequireComponent(typeof(Button))]
public class CombatScannerButtonLinker : MonoBehaviour
{
    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    private void Start()
    {
        button.onClick.AddListener(HandleClick);
    }

    private void Update()
    {
        // Optional: Manage button interactivity based on scanner availability
        if (CombatScannerSystem.Instance != null)
        {
            button.interactable = CombatScannerSystem.Instance.CanUseScannerUI();
        }
        else
        {
            button.interactable = false;
        }
    }

    private void HandleClick()
    {
        if (CombatScannerSystem.Instance != null)
        {
            CombatScannerSystem.Instance.ToggleScanner();
        }
    }
}
