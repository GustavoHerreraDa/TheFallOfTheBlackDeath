using UnityEngine.UI;
using UnityEngine;
using System.Collections;
using TMPro;

/// <summary>
/// Handles tooltip for the current project workflow.
/// </summary>
public class Tooltip : MonoBehaviour // Idealmente las clases en C# empiezan con mayúscula
{
    public SkillManager skillManager;
    public static Tooltip instance;
    public Image tool;
    public TextMeshProUGUI skillNameTxT;
    public GameObject actionsButtonsPanel;
    public GameObject fondoUi;

    void Awake()
    {
        // Inicializamos el Singleton temprano
        instance = this;
    }

    /// <summary>
    /// Initializes the component once the scene dependencies are ready.
    /// </summary>
    void Start()
    {
        skillManager = FindObjectOfType<SkillManager>();
        disableSkillTxT(); // Reutilizamos tu propio método para setear el estado inicial apagado
    }

    /// <summary>
    /// Executes the mouse over workflow.
    /// </summary>
    /// <param name="SkillIndex">The skill index.</param>
    public void mouseOver(int SkillIndex)
    {
        tool.enabled = true;
        fondoUi.SetActive(true);
        skillNameTxT.enabled = true;
        
        // Asignación directa, más óptima sin GetComponent
        skillNameTxT.text = skillManager.GetSkillDescription(SkillIndex);
    }

    /// <summary>
    /// Executes the disable skill tx t workflow.
    /// </summary>
    public void disableSkillTxT()
    {
        tool.enabled = false;
        if (fondoUi != null) fondoUi.SetActive(false);
        skillNameTxT.enabled = false;
        skillNameTxT.text = string.Empty; // Limpiamos el texto para evitar basura visual
    }

    /// <summary>
    /// Updates the component each frame while it is active.
    /// </summary>
    void Update()
    {
        if (actionsButtonsPanel == null)
            return;

        if (!actionsButtonsPanel.activeSelf)
            disableSkillTxT();
    }

    /// <summary>
    /// Shows the tooltip with a specific string.
    /// </summary>
    /// <param name="tooltipString">The tooltip string.</param>
    private void ShowTooltip(string tooltipString)
    {
        // Ahora sí aplicamos el texto al mostrar el tooltip por string
        skillNameTxT.text = tooltipString;
        tool.enabled = true;
        fondoUi.SetActive(true);
        skillNameTxT.enabled = true;
    }

    /// <summary>
    /// Hides the tooltip.
    /// </summary>
    private void HideTooltip()
    {
        disableSkillTxT(); // Usamos tu método seguro en lugar de apagar el gameObject
    }

    /// <summary>
    /// Hides the tooltip after a delay.
    /// </summary>
    IEnumerator HideTooltipEnum()
    {
        yield return new WaitForSeconds(10f);
        disableSkillTxT();
    }

    /// <summary>
    /// Shows the tooltip statically.
    /// </summary>
    /// <param name="tooltipString">The tooltip string.</param>
    public static void ShowTooltip_static(string tooltipString)
    {
        if (instance != null)
            instance.ShowTooltip(tooltipString);
    }

    /// <summary>
    /// Hides the tooltip statically.
    /// </summary>
    public static void HideTooltip_static() // Corregido el nombre para reflejar la acción real
    {
        if (instance != null)
            instance.HideTooltip();
    }
}