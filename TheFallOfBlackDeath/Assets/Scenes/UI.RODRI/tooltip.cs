using UnityEngine.UI;
using UnityEngine;
using System.Collections;

public class tooltip : MonoBehaviour

{
    public SkillManager skillManager;
    private static tooltip instance;
    public Image tool;
    public Text skillNameTxT;
    public GameObject ActionsButtonsPanel;
    // Start is called before the first frame update
    void Start()
    {
        skillManager = FindObjectOfType<SkillManager>();
        tool.enabled = false;
        skillNameTxT.enabled = false;
        instance = this;
    }


    public void mouseOver(int SkillIndex)
    {
        tool.enabled = true;
        skillNameTxT.enabled = true;
        skillNameTxT.GetComponent<Text>().text = skillManager.GetSkillDescription(SkillIndex);

        //var couritine = StartCoroutine("HideTooltipEnum");
        Debug.Log("mouse over");
    }
    public void disableSkillTxT()
    {
        tool.enabled = false;
        skillNameTxT.enabled = false;
    }


    // Update is called once per frame
    void Update()
    {
        if (ActionsButtonsPanel == null)
            return;

        if (!ActionsButtonsPanel.activeSelf)
            disableSkillTxT();
    }
    private void ShowTooltip(string tooltipString)
    {
        gameObject.SetActive(true);
    }
    private void HideTooltip()
    {
        gameObject.SetActive(false);
    }

    IEnumerator HideTooltipEnum()
    {
        yield return new WaitForSeconds(10f);
        disableSkillTxT();
    }
    public static void ShowTooltip_static(string tooltipString)
    {
        instance.ShowTooltip(tooltipString);
    }

    public static void ShowTooltip_static()
    {
        instance.HideTooltip();
    }
}
