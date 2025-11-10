using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class DitherToggleUI : MonoBehaviour
{
    [Header("Renderer Data")]
    public UniversalRendererData rendererData;

    [Header("UI")]
    public Button toggleButton;
    public Text buttonText;

    public DitherFeature ditherFeature;

    void Start()
    {
        // Buscar el feature
        ditherFeature = rendererData.rendererFeatures.Find(f => f is DitherFeature) as DitherFeature;

        if (ditherFeature == null)
        {
            Debug.LogError("no se encontró el ditherRenderFeature en el renderer asignado.");
            return;
        }

        // Asignar listener al botón
        if (toggleButton != null)
            toggleButton.onClick.AddListener(ToggleDither);

        UpdateButtonLabel();
    }

    public void ToggleDither()
    {
        if (ditherFeature == null) return;

        ditherFeature.settings.enabled = !ditherFeature.settings.enabled;
        rendererData.SetDirty(); //forzar actualización del pipeline
        UpdateButtonLabel();
    }

    private void UpdateButtonLabel()
    {
        if (buttonText != null)
            buttonText.text = ditherFeature.settings.enabled ? "Dither: ON" : "Dither: OFF";
    }
}
