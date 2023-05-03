using UnityEngine.UI;
using UnityEngine;

public class tooltip : MonoBehaviour

{
    public Text skillNameTxT;
    // Start is called before the first frame update
    void Start()
    {
        skillNameTxT.enabled = false;
    }

    public void mouseOverPunch(string message)
    {
        skillNameTxT.enabled = true;
        skillNameTxT.GetComponent<Text>().text = "Golpeas con el baston al enemigo (daño fisico)";

        Debug.Log("mouse over");
    }

    public void disableSkillTxT()
    {
        skillNameTxT.enabled = false;
    }

    public void mouseOverMotivate(string message)
    {
        skillNameTxT.enabled = true;
        skillNameTxT.GetComponent<Text>().text = "Recordaste una charla ted y te motivaste (sube el ataque)";

        Debug.Log("mouse over");
    }

    public void mouseOverWeaken(string message)
    {
        skillNameTxT.enabled = true;
        skillNameTxT.GetComponent<Text>().text = "Disminuye considerablemente la defensa del rival";

        Debug.Log("mouse over");
    }

    public void mouseOverHeal(string message)
    {
        skillNameTxT.enabled = true;
        skillNameTxT.GetComponent<Text>().text = "Utilizas tus conocimientos medicos para sanarte";

        Debug.Log("mouse over");
    }


    // Update is called once per frame
    void Update()
    {

    }
}
