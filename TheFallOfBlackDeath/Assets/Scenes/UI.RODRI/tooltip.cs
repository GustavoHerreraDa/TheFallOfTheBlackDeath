using UnityEngine.UI;
using UnityEngine;
using System.Collections;

public class tooltip : MonoBehaviour

{
    public SkillManager skillManager;
    private static tooltip instance;
    public Image tool;
    public Text skillNameTxT;
    // Start is called before the first frame update
    void Start()
    {
        skillManager = FindObjectOfType<SkillManager>();
        tool.enabled = false;
        skillNameTxT.enabled = false;
        instance = this;
    }

    public void mouseOverPunch(string message)
    {
        tool.enabled = true;
        skillNameTxT.enabled = true;
        skillNameTxT.GetComponent<Text>().text = "Golpeas con el baston al enemigo (daño fisico)";

        Debug.Log("mouse over");
    }
    public void mouseOver(int SkillIndex)
    {
        tool.enabled = true;
        skillNameTxT.enabled = true;
        skillNameTxT.GetComponent<Text>().text = skillManager.GetSkillDescription(SkillIndex);

        var couritine = StartCoroutine("HideTooltipEnum");
        Debug.Log("mouse over");
    }
    public void disableSkillTxT()
    {
        tool.enabled = false;
        skillNameTxT.enabled = false;
    }

    public void mouseOverMotivate(string message)
    {
        tool.enabled = true;
        skillNameTxT.enabled = true;
        skillNameTxT.GetComponent<Text>().text = "Recordaste una charla ted y te motivaste (sube el ataque)";

        Debug.Log("mouse over");
    }

    public void mouseOverWeaken(string message)
    {
        tool.enabled = true;
        skillNameTxT.enabled = true;
        skillNameTxT.GetComponent<Text>().text = "Disminuye considerablemente la defensa del rival";

        Debug.Log("mouse over");
    }

    public void mouseOverHeal(string message)
    {
        tool.enabled = true;
        skillNameTxT.enabled = true;
        skillNameTxT.GetComponent<Text>().text = "Utilizas tus conocimientos medicos para sanarte";

        Debug.Log("mouse over");
    }


    // Update is called once per frame
    void Update()
    {

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
        yield return new WaitForSeconds(20f);
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
