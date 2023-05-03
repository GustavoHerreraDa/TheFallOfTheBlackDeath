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
        skillNameTxT.GetComponent<Text>().text = "Ataque Fisico";

        Debug.Log("mouse over");
    }

    public void disableSkillTxT()
    {
        skillNameTxT.enabled = false;
    }


    // Update is called once per frame
    void Update()
    {

    }
}
