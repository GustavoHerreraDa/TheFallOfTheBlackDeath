using UnityEngine.UI;
using UnityEngine;
using System.Collections;
using TMPro;
/// <summary>
/// Handles tooltip for the current project workflow.
/// </summary>
public class tooltip : MonoBehaviour

{
    public SkillManager skillManager;
    private static tooltip instance;
    public Image tool;
    public TextMeshProUGUI skillNameTxT;
    public GameObject ActionsButtonsPanel;

    public GameObject fondoUi;
    // Start is called before the first frame update
    /// <summary>
    /// Initializes the component once the scene dependencies are ready.
    /// </summary>
    void Start()
    {
        skillManager = FindObjectOfType<SkillManager>();
        tool.enabled = false;
        skillNameTxT.enabled = false;
        instance = this;
        fondoUi.SetActive(false);
    }


    /// <summary>
    /// Executes the mouse over workflow.
    /// </summary>
    /// <param name="SkillIndex">The skill index.</param>
    public void mouseOver(int SkillIndex)
    {
        tool.enabled = true;
        fondoUi.SetActive(true);
        //var couritine = StartCoroutine("HideTooltipEnum"); 
        skillNameTxT.enabled = true;
        skillNameTxT.GetComponent<TextMeshProUGUI>().text = skillManager.GetSkillDescription(SkillIndex);
    }
    /// <summary>
    /// Executes the disable skill tx t workflow.
    /// </summary>
    public void disableSkillTxT()
    {
        tool.enabled = false;
        fondoUi.SetActive(false);
        skillNameTxT.enabled = false;
    }


    // Update is called once per frame
    /// <summary>
    /// Updates the component each frame while it is active.
    /// </summary>
    void Update()
    {
        if (ActionsButtonsPanel == null)
            return;

        if (!ActionsButtonsPanel.activeSelf)
            disableSkillTxT();
    }
    /// <summary>
    /// Shows the tooltip.
    /// </summary>
    /// <param name="tooltipString">The tooltip string.</param>
    private void ShowTooltip(string tooltipString)
    {
        gameObject.SetActive(true);
    }
    /// <summary>
    /// Hides the tooltip.
    /// </summary>
    private void HideTooltip()
    {
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Hides the tooltip enum.
    /// </summary>
    /// <returns>An enumerator that drives the coroutine sequence.</returns>
    IEnumerator HideTooltipEnum()
    {
        yield return new WaitForSeconds(10f);
        disableSkillTxT();
    }
    /// <summary>
    /// Shows the tooltip static.
    /// </summary>
    /// <param name="tooltipString">The tooltip string.</param>
    public static void ShowTooltip_static(string tooltipString)
    {
        instance.ShowTooltip(tooltipString);
    }

    /// <summary>
    /// Shows the tooltip static.
    /// </summary>
    public static void ShowTooltip_static()
    {
        instance.HideTooltip();
    }
}
