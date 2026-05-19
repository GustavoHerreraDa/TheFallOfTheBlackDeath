using UnityEngine;
using TMPro; // Usamos TextMeshPro para el texto

public class PatchNotesManager : MonoBehaviour
{
    [Header("Configuración de Versión")]
    [SerializeField] private string currentVersion = "1.0.2"; // Cambiá esto cada vez que actualices el juego

    [Header("Componentes de UI")]
    [SerializeField] private GameObject patchNotesPanel; // El panel que contiene toda la UI de las notas
    [SerializeField] private TextMeshProUGUI titleText;   // Texto para el título (ej: "¡Nueva Actualización v1.0.2!")
    [SerializeField] private TextMeshProUGUI bodyText;    // Texto donde van los cambios reales

    [Header("Contenido de las Notas (Escribir acá)")]
    [TextArea(10, 20)] // Esto te da un espacio cómodo en el Inspector para escribir
    [SerializeField] private string patchNotesContent = 
        "- Corregido un bug que rompía el inventario.\n" +
        "- Ajustada la iluminación retro/dithering en el escenario principal.\n" +
        "- Se agregó soporte para pantallas wide.\n" +
        "- Mejoras generales de optimización.";

    private const string VersionPreferenceKey = "LastSeenVersion";

    void Start()
    {
        CheckForUpdates();
    }

    private void CheckForUpdates()
    {
        // Obtenemos la última versión que vio el jugador
        string lastSeenVersion = PlayerPrefs.GetString(VersionPreferenceKey, "");

        // Si es la primera vez que juega o si la versión actual es distinta a la última que vio
        if (string.IsNullOrEmpty(lastSeenVersion) || lastSeenVersion != currentVersion)
        {
            ShowPatchNotes();
        }
        else
        {
            // Si ya la vio, nos aseguramos de que el panel esté cerrado
            if (patchNotesPanel != null) patchNotesPanel.SetActive(false);
        }
    }

    private void ShowPatchNotes()
    {
        if (patchNotesPanel == null || titleText == null || bodyText == null)
        {
            Debug.LogError("Faltan asignar componentes en el PatchNotesManager.");
            return;
        }

        // Asignamos los textos de forma manual desde el Inspector
        titleText.text = $"Actualización v{currentVersion}";
        bodyText.text = patchNotesContent;

        // Mostramos el panel
        patchNotesPanel.SetActive(true);
    }

    // Este método lo tenés que asignar al botón de "Cerrar" o "Entendido" en la UI
    public void ClosePatchNotes()
    {
        // Guardamos que el jugador ya vio ESTA versión para que no vuelva a saltar
        PlayerPrefs.SetString(VersionPreferenceKey, currentVersion);
        PlayerPrefs.Save();

        // Ocultamos el panel
        if (patchNotesPanel != null) patchNotesPanel.SetActive(false);
    }
}